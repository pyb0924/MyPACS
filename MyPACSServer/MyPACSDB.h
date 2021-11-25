#include <string>
#include <dcmtk/dcmdata/dcfilefo.h>
#include <mysqlx/xdevapi.h>

using namespace mysqlx;

struct DcmIndexInfo
{
	std::string SOPInstanceUID;
	std::string patientName;
	std::string patientID;
	std::string studyID;
	std::string studyInstanceUID;
	std::string seriesDescription;
	std::string seriesInstanceUID;
	short seriesNumber = 0;
};

struct DBConfigInfo
{
	std::string host;
	int port = 104;
	std::string username;
	std::string password;
	std::string schema;
	std::string table;
};

class MyPACSDB
{
public:
	MyPACSDB();
	MyPACSDB(const DBConfigInfo& config);
	bool CreateTable(string createSQL);
	bool InsertDcmToDB(DcmFileFormat& fileformat);

private:
	Table GetTable();
	void GetDcmIndexInfo(DcmFileFormat& fileformat, DcmIndexInfo& dcmIndexInfo);
	DBConfigInfo dbConfig;
};

