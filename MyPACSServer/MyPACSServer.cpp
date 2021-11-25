#include <boost/filesystem.hpp>
#include <boost/property_tree/ptree.hpp>
#include <boost/property_tree/ini_parser.hpp>
#include <mysqlx/xdevapi.h>
#include <dcmtk/dcmnet/diutil.h>

#include "MyPACSServer.h"

// TODO fill all methods
using namespace logging::trivial;
namespace fs = boost::filesystem;
namespace pt = boost::property_tree;
using namespace mysqlx;

static void StoreSCPCallback(void* callbackData, T_DIMSE_StoreProgress* progress, T_DIMSE_C_StoreRQ* req,
	char* imageFileName, DcmDataset** imageDataSet, T_DIMSE_C_StoreRSP* rsp, DcmDataset** statusDetail) {
	DIC_UI sopClass;
	DIC_UI sopInstance;
	if (progress->state == DIMSE_StoreEnd) {
		*statusDetail = nullptr;
		StoreCallbackData* cbdata = OFstatic_cast(StoreCallbackData*, callbackData);

		MyPACSDB db(cbdata->dbConfig);
		db.InsertDcmToDB(*(cbdata->dcmff));

		if (rsp->DimseStatus == STATUS_Success) {
			if (!DU_findSOPClassAndInstanceInDataSet(*imageDataSet,
				sopClass, sizeof(sopClass),
				sopInstance, sizeof(sopInstance))) {
				rsp->DimseStatus = STATUS_STORE_Error_CannotUnderstand;
			}
			else if (strcmp(sopClass, req->AffectedSOPClassUID) != 0) {
				rsp->DimseStatus = STATUS_STORE_Error_DataSetDoesNotMatchSOPClass;
			}
			else if (strcmp(sopInstance, req->AffectedSOPInstanceUID) != 0) {
				rsp->DimseStatus = STATUS_STORE_Error_DataSetDoesNotMatchSOPClass;
			}
		}
	}
}


MyPACSServer::MyPACSServer(std::string configPath)
{
	if (fs::exists(configPath))
	{
		BOOST_LOG_SEV(logger, error) << "config.ini not exists!";
	}
	pt::ptree scpNode, dbNode;
	pt::ini_parser::read_ini(configPath, ptConfig);
	scpConfig.port = GetProperty("SCP", "port", 0);
	scpConfig.m_aeTitle = GetProperty("SCP", "aeTitle", std::string());
	dbConfig.host = GetProperty("DB", "host", std::string());
	dbConfig.port = GetProperty("DB", "port", 0);
	dbConfig.username = GetProperty("DB", "username", std::string());
	dbConfig.password = GetProperty("DB", "password", std::string());
	dbConfig.schema = GetProperty("DB", "schema", std::string());
	dbConfig.table = GetProperty("DB", "table", std::string());
}

template <class T>
T MyPACSServer::GetProperty(std::string child, std::string key, const T& defaultValue)
{
	pt::ptree childNode = ptConfig.get_child(child);
	if (childNode.count(key) != 1)
	{
		BOOST_LOG_SEV(logger, error) << key << " node not exists!";
		return defaultValue;
	}
	return childNode.get<T>(key);
}


void MyPACSServer::run()
{
	while (1);
}

OFCondition MyPACSServer::AcceptAssociation(T_ASC_Network* net, DcmAssociationConfiguration& asccfg)
{
	// init network
	char buf[BUFSIZ];
	T_ASC_Association* assoc;
	OFCondition cond;
	OFString temp_str;
	const char* knownAbstractSyntaxes[] = {
		UID_VerificationSOPClass
	};
	const char* transferSyntaxes[] = {
		nullptr, nullptr, nullptr, nullptr, nullptr, nullptr, nullptr,
		nullptr, nullptr, nullptr, nullptr, nullptr, nullptr, nullptr
	};
	int numTransferSyntaxes = 0;

	cond = ASC_receiveAssociation(net, &assoc, ASC_DEFAULTMAXPDU);
	// deal with error
	if (cond.bad()) {
		DimseCondition::dump(temp_str, cond);
		std::cout << "½ÓÊÕ¹ØÁªÊ§°Ü: " << temp_str.c_str() << std::endl;
	}
	if (gLocalByteOrder == EBO_LittleEndian) { /* defined in dcxfer.h */
		transferSyntaxes[0] = UID_LittleEndianExplicitTransferSyntax;
		transferSyntaxes[1] = UID_BigEndianExplicitTransferSyntax;
	}
	else {
		transferSyntaxes[0] = UID_BigEndianExplicitTransferSyntax;
		transferSyntaxes[1] = UID_LittleEndianExplicitTransferSyntax;
	}
	transferSyntaxes[2] = UID_LittleEndianImplicitTransferSyntax;
	numTransferSyntaxes = 3;

	if (cond.good()) {
		cond = ASC_acceptContextsWithPreferredTransferSyntaxes(
			assoc->params, knownAbstractSyntaxes,
			DIM_OF(knownAbstractSyntaxes), transferSyntaxes, numTransferSyntaxes);
		if (cond.bad()) {
			DimseCondition::dump(temp_str, cond);
			std::cout << temp_str.c_str() << std::endl;
		}
	}

	if (cond.good()) {
		cond = ASC_acceptContextsWithPreferredTransferSyntaxes(
			assoc->params, dcmAllStorageSOPClassUIDs,
			numberOfDcmAllStorageSOPClassUIDs,
			transferSyntaxes, numTransferSyntaxes);
		if (cond.bad()) {
			DimseCondition::dump(temp_str, cond);
			std::cout << temp_str.c_str() << std::endl;
		}
	}

	// check aet title
	ASC_setAPTitles(assoc->params, nullptr, nullptr, scpConfig.m_aeTitle.c_str());

	if (cond.good()) {
		cond = ASC_getApplicationContextName(assoc->params, buf, sizeof(buf));
		if ((cond.bad()) || strcmp(buf, UID_StandardApplicationContext) != 0) {
			T_ASC_RejectParameters rej = {
				ASC_RESULT_REJECTEDPERMANENT,
				ASC_SOURCE_SERVICEUSER,
				ASC_REASON_SU_APPCONTEXTNAMENOTSUPPORTED
			};
			DimseCondition::dump(temp_str, cond);
			std::cout << "assocation failed: context name error! " << buf << std::endl;
			cond = ASC_rejectAssociation(assoc, &rej);
			if (cond.bad()) {
				DimseCondition::dump(temp_str, cond);
				std::cout << temp_str.c_str() << std::endl;
			}
		}
		else {
			cond = ASC_acknowledgeAssociation(assoc);
			if (cond.bad()) {
				DimseCondition::dump(temp_str, cond);
				std::cout << temp_str.c_str() << std::endl;
			}
		}
	}

	if (cond.good()) {

		DIC_AE callingTitle;
		DIC_AE calledTitle;
		ASC_getAPTitles(assoc->params, callingTitle, sizeof(callingTitle),
			calledTitle, sizeof(calledTitle), nullptr, 0).good();

		cond = ProcessCommands(assoc);
		if (cond == DUL_PEERREQUESTEDRELEASE) {
			cond = ASC_acknowledgeRelease(assoc);
		}
		else {
			DimseCondition::dump(temp_str, cond);
			std::cout << "DIMSE failed abort association: " << temp_str.c_str() << std::endl;

			cond = ASC_abortAssociation(assoc);
		}
	}

	// clean up
	cond = ASC_dropSCPAssociation(assoc);
	if (cond.bad()) {
		DimseCondition::dump(temp_str, cond);
		std::cout << temp_str.c_str() << std::endl;
	}
	cond = ASC_destroyAssociation(&assoc);
	if (cond.bad()) {
		DimseCondition::dump(temp_str, cond);
		std::cout << temp_str.c_str() << std::endl;
	}
	return cond;
}

OFCondition MyPACSServer::ProcessCommands(T_ASC_Association* assoc)
{
	OFCondition cond = EC_Normal;
	T_DIMSE_Message msg;
	T_ASC_PresentationContextID presID = 0;
	DcmDataset* statusDetail = NULL;

	// start a loop to be able to receive more than one DIMSE command
	while (cond == EC_Normal || cond == DIMSE_NODATAAVAILABLE || cond == DIMSE_OUTOFRESOURCES)
	{
		// receive a DIMSE command over the network
		cond = DIMSE_receiveCommand(assoc, DIMSE_BLOCKING, 0, &presID, &msg, &statusDetail);
		if (cond == EC_Normal)
		{
			// storescp can only process a C-ECHO-RQ and a C-STORE-RQ
			switch (msg.CommandField)
			{
			case DIMSE_C_ECHO_RQ:
				// process C-ECHO-Request
				cond = EchoSCP(assoc, &msg, presID);
				break;
			case DIMSE_C_STORE_RQ:
				// process C-STORE-Request
				cond = StoreSCP(assoc, &msg, presID);
				break;
			case DIMSE_C_FIND_RQ:
				cond = FindSCP(assoc, &msg, presID);
				break;
			case DIMSE_C_GET_RQ:
				cond = GetSCP(assoc, &msg, presID);
				break;
			default:
				// we cannot handle this kind of message
				cond = DIMSE_BADCOMMANDTYPE;
				break;
			}
		}
	}
	return cond;
}

OFCondition MyPACSServer::EchoSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID)
{
	OFString temp_str;
	OFCondition cond = DIMSE_sendEchoResponse(assoc, presID, &msg->msg.CEchoRQ, STATUS_Success, nullptr);
	if (cond.bad()) {
		DimseCondition::dump(temp_str, cond);
		std::cout << "Echo SCP failed: " << temp_str.c_str() << std::endl;
	}
	else {
		std::cout << "Echo SCP success!" << std::endl;
	}
	return cond;
}

OFCondition MyPACSServer::StoreSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID)
{
	OFCondition cond = EC_Normal;
	T_DIMSE_C_StoreRQ* req;
	req = &msg->msg.CStoreRQ;
	StoreCallbackData callbackData;
	DcmFileFormat dcmff;

	callbackData.assoc = assoc;
	callbackData.dcmff = &dcmff;
	callbackData.writeTransferSyntax = EXS_Unknown;

	const char* aet = nullptr;
	const char* aec = nullptr;

	if (assoc && assoc->params) {
		aet = assoc->params->DULparams.callingAPTitle;
		aec = assoc->params->DULparams.calledAPTitle;
	}

	if (std::string(aec) != scpConfig.m_aeTitle) {
		std::cout << "aet title check error: " << aet << std::string(aec) << scpConfig.m_aeTitle;
	}
	else
	{
		DcmDataset* dset = dcmff.getDataset();
		cond = DIMSE_storeProvider(assoc, presID, req, nullptr, OFTrue, &dset,
			StoreSCPCallback, &callbackData, DIMSE_BLOCKING, 0);

		if (cond.bad()) {
			OFString temp_str;
			DimseCondition::dump(temp_str, cond);
			std::cout << "Store SCP failed!" << std::endl;
		}
		else {
			std::cout << "Store SCP success!" << std::endl;
		}
	}

	return cond;
}

OFCondition MyPACSServer::FindSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID)
{
	return OFCondition();
}

OFCondition MyPACSServer::GetSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID)
{
	return OFCondition();
}
