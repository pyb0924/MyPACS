#include <dcmtk/dcmjpeg/djrplol.h>
#include <dcmtk/dcmjpeg/djencode.h>

#include "StudySCU.h"

Uint8 FindPresentationContext(const OFString& sopClass, DcmSCU& scu)
{
	Uint8 pc;
	pc = scu.findPresentationContextID(sopClass, UID_LittleEndianExplicitTransferSyntax);
	if (!pc)
		pc = scu.findPresentationContextID(sopClass, UID_BigEndianExplicitTransferSyntax);
	if (!pc)
		pc = scu.findPresentationContextID(sopClass, UID_LittleEndianImplicitTransferSyntax);
	return pc;
}

bool ExecuteCStore(DcmSCU& scu, const OFString& sopClass, const OFString& filePath)
{
	T_ASC_PresentationContextID presID =  FindPresentationContext(sopClass, scu);
	if (presID == 0)
	{
		DCMNET_ERROR("There is no uncompressed presentation context for C-STORE");
		return false;
	}
	Uint16 rspStatusCode;
	OFCondition result;
	DcmDataset* datasetPtr = nullptr;
	result = scu.sendSTORERequest(presID, filePath, NULL, rspStatusCode);

	if (result.bad()) {
		DCMNET_ERROR("C-STORE Operation failed.");
		return false;
	}
	else {
		DCMNET_INFO("C-STORE Operation completed successfully.");
	}
	return true;
}
