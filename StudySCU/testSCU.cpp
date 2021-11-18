#include <cassert>
#include <Windows.h>

#include <boost/filesystem.hpp>

#include <dcmtk/config/osconfig.h>
#include <dcmtk/dcmnet/diutil.h>
#include <dcmtk/dcmnet/scu.h> 

#include "StudySCU.h"

namespace fs = boost::filesystem;

int main(int argc, char* argv[])
{
	if (argc != 2)
	{
		throw "number of parameters wrong! need 1";
	}
	std::string sDicomPath(argv[1]);
	fs::path pDicomPath(sDicomPath);

	OFLog::configure(OFLogger::DEBUG_LOG_LEVEL);
	DcmSCU scu;
	// set AET
	scu.setAETitle(APPLICATIONTITLE);
	// set peer SCP
	scu.setPeerHostName(PEERHOSTNAME);
	scu.setPeerPort(PEERPORT);
	scu.setPeerAETitle(PEERAPPLICATIONTITLE);

	// transfer syntaxes list
	OFList<OFString> ts;
	
	ts.push_back(UID_LittleEndianExplicitTransferSyntax);
	ts.push_back(UID_BigEndianExplicitTransferSyntax);
	ts.push_back(UID_LittleEndianImplicitTransferSyntax);
	scu.addPresentationContext(UID_VerificationSOPClass, ts); // C-ECHO: VerificationSOPClass
	scu.addPresentationContext(UID_MRImageStorage, ts); // C-STORE: VTImageStorage

	assert(scu.initNetwork().good());
	assert(scu.negotiateAssociation().good());

	// C-ECHO: test connection
	assert(scu.sendECHORequest(0).good());

	// C-STORE	
	OFString file;
	if (fs::is_directory(pDicomPath))
	{
		fs::recursive_directory_iterator endIter;
		for (fs::recursive_directory_iterator iter(pDicomPath); iter != endIter; iter++)
		{
			file = iter->path().string().c_str();
			assert(ExecuteCStore(scu, UID_MRImageStorage, file));
		}
	}
	else {
		file = sDicomPath.c_str();
		assert(ExecuteCStore(scu, UID_MRImageStorage, file));
	}

	// close connection
	scu.closeAssociation(DCMSCU_RELEASE_ASSOCIATION);

	return 0;
}
