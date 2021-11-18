#ifndef MY_PACS_SERVER
#define MY_PACS_SERVER

#include <list>

#include <boost/log/trivial.hpp>
#include <boost/log/sources/severity_channel_logger.hpp>


#include "dcmtk/dcmnet/diutil.h"
#include "dcmtk/dcmnet/dcasccfg.h"

namespace logging = boost::log;
namespace src = boost::log::sources;

struct SCPConfigInfo
{
	int port;
	std::string m_aeTitle;
};

struct DBConfigInfo
{
	std::string host;
	int port;
	std::string username;
	std::string password;
	std::string schema;
	std::string table;
};

class MyPACSServer
{
public:
	MyPACSServer();
	MyPACSServer(std::string configXMLPath);
	void run();
private:
	bool ParseConfig(std::string configXMLPath);

	OFCondition AcceptAssociation(T_ASC_Network* net, DcmAssociationConfiguration& asccfg);
	OFCondition ProcessCommands(T_ASC_Association* assoc);
	OFCondition EchoSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID);
	OFCondition StoreSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID);
	OFCondition FindSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID);
	OFCondition GetSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID);

	std::list<std::string> GetLocalPathFromDB(std::string selectSQL);
private:
	SCPConfigInfo scpConfig;
	DBConfigInfo dbConfig;
	src::severity_channel_logger<logging::trivial::severity_level, std::string> logger;
};

#endif // !MY_PACS_SERVER


