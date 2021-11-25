#ifndef MY_PACS_SERVER
#define MY_PACS_SERVER

#include <list>

#include <boost/log/trivial.hpp>
#include <boost/log/sources/severity_channel_logger.hpp>
#include <boost/property_tree/ptree.hpp>

#include <dcmtk/config/osconfig.h>
#include <dcmtk/ofstd/ofcond.h>
#include <dcmtk/dcmnet/assoc.h>
#include <dcmtk/dcmnet/dimse.h>
#include <dcmtk/dcmnet/dcasccfg.h>
#include <dcmtk/dcmdata/dcfilefo.h>

#include "MyPACSDB.h"

namespace logging = boost::log;
namespace src = boost::log::sources;
namespace pt = boost::property_tree;

struct SCPConfigInfo
{
	int port;
	std::string m_aeTitle;
};


struct StoreCallbackData
{
	DcmFileFormat* dcmff;
	T_ASC_Association* assoc;
	E_TransferSyntax writeTransferSyntax = EXS_Unknown;
	DBConfigInfo dbConfig;
	pt::ptree ptConfig;
};

class MyPACSServer
{
public:
	MyPACSServer(std::string configPath);
	void run();
private:
	template <class T>
	T GetProperty(std::string child, std::string key, const T& defaultValue);

	OFCondition AcceptAssociation(T_ASC_Network* net, DcmAssociationConfiguration& asccfg);
	OFCondition ProcessCommands(T_ASC_Association* assoc);
	OFCondition EchoSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID);
	OFCondition StoreSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID);
	OFCondition FindSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID);
	OFCondition GetSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID);

private:
	SCPConfigInfo scpConfig;
	DBConfigInfo dbConfig;
	pt::ptree ptConfig;
	src::severity_channel_logger<logging::trivial::severity_level, std::string> logger;
};

#endif // !MY_PACS_SERVER


