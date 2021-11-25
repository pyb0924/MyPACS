#include <boost/format.hpp>

#include "MyPACSDB.h"
#include <dcmtk/dcmdata/dcdeftag.h>

MyPACSDB::MyPACSDB()
{
}

MyPACSDB::MyPACSDB(const DBConfigInfo& config)
{
	dbConfig = config;
}

bool MyPACSDB::CreateTable(string createSQL)
{
	try
	{
		Session session(dbConfig.host, dbConfig.port, dbConfig.username, dbConfig.password);
		Table dcmTable = session.getSchema(dbConfig.schema).getTable(dbConfig.table);
		if (dcmTable.existsInDatabase())
		{
			return true;
		}

		session.sql(string("USE ") + string(dbConfig.schema)).execute();
		string createSQLWithName = boost::str(boost::format(createSQL) % dbConfig.table);
		session.sql(createSQLWithName).execute();
	}
	catch (std::exception ex)
	{
		return false;
	}
	return true;
}

bool MyPACSDB::InsertDcmToDB(DcmFileFormat& fileformat)
{
	DcmIndexInfo indexInfo;
	GetDcmIndexInfo(fileformat, indexInfo);
	Table table = GetTable();
	RowResult result = table.select("SOPInstanceUID").execute();
	Row row;
	bool flag = false;
	while ((row = result.fetchOne()))
	{
		if (std::string(row[0]) == indexInfo.SOPInstanceUID)
		{
			flag = true;
			break;
		}
	}
	if (flag)
	{
		return false;
	}
	try
	{
		table.insert("SOPInstanceUID", "patientName", "patientID", "studyID", "studyInstanceUID",
			"seriesDescription", "seriesInstanceUID", "seriesNumber")
			.values(indexInfo.SOPInstanceUID, indexInfo.patientName, indexInfo.patientID, indexInfo.studyID,
				indexInfo.studyInstanceUID, indexInfo.seriesDescription, indexInfo.seriesInstanceUID, indexInfo.seriesNumber)
			.execute();
	}
	catch (const std::exception&)
	{
		return false;
	}
	
	return true;
}

Table MyPACSDB::GetTable()
{
	Session session(dbConfig.host, dbConfig.port, dbConfig.username, dbConfig.password);
	Table dcmTable = session.getSchema(dbConfig.schema).getTable(dbConfig.table);
	return dcmTable;
}

void MyPACSDB::GetDcmIndexInfo(DcmFileFormat& fileformat, DcmIndexInfo& dcmIndexInfo)
{
	DcmDataset* datasetPtr = fileformat.getDataset();

	DcmElement* elementBuffer = nullptr;
	OFString stringBuffer;
	Sint32 shortBuffer;
	if (datasetPtr->findAndGetElement(DCM_SOPInstanceUID, elementBuffer).good())
	{
		assert(elementBuffer->getOFStringArray(stringBuffer).good());
		dcmIndexInfo.SOPInstanceUID = std::string(stringBuffer.c_str());
	}
	if (datasetPtr->findAndGetElement(DCM_PatientName, elementBuffer).good())
	{
		assert(elementBuffer->getOFStringArray(stringBuffer).good());
		dcmIndexInfo.patientName = std::string(stringBuffer.c_str());
	}
	if (datasetPtr->findAndGetElement(DCM_PatientID, elementBuffer).good())
	{
		assert(elementBuffer->getOFStringArray(stringBuffer).good());
		dcmIndexInfo.patientID = std::string(stringBuffer.c_str());
	}
	if (datasetPtr->findAndGetElement(DCM_StudyID, elementBuffer).good())
	{
		assert(elementBuffer->getOFStringArray(stringBuffer).good());
		dcmIndexInfo.studyID = std::string(stringBuffer.c_str());
	}
	if (datasetPtr->findAndGetElement(DCM_StudyInstanceUID, elementBuffer).good())
	{
		assert(elementBuffer->getOFStringArray(stringBuffer).good());
		dcmIndexInfo.studyInstanceUID = std::string(stringBuffer.c_str());
	}
	if (datasetPtr->findAndGetElement(DCM_SeriesDescription, elementBuffer).good())
	{
		assert(elementBuffer->getOFStringArray(stringBuffer).good());
		dcmIndexInfo.seriesDescription = std::string(stringBuffer.c_str());
	}
	if (datasetPtr->findAndGetElement(DCM_SeriesInstanceUID, elementBuffer).good())
	{
		assert(elementBuffer->getOFStringArray(stringBuffer).good());
		dcmIndexInfo.seriesInstanceUID = std::string(stringBuffer.c_str());
	}
	if (datasetPtr->findAndGetElement(DCM_SeriesNumber, elementBuffer).good())
	{
		assert(elementBuffer->getSint32(shortBuffer).good());
		dcmIndexInfo.seriesNumber = shortBuffer;
	}
}


