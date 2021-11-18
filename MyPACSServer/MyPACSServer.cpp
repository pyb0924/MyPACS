#include "MyPACSServer.h"

// TODO fill all methods
using namespace logging::trivial;

MyPACSServer::MyPACSServer()
{
	scpConfig.port = 104;
	BOOST_LOG_SEV(logger, debug) << "Server Started. Listening at port[ " << scpConfig.port << "]";
}

MyPACSServer::MyPACSServer(std::string configXMLPath)
{
}
	

void MyPACSServer::run()
{
	while (1);
}

bool MyPACSServer::ParseConfig(std::string configXMLPath)
{
	return false;
}

OFCondition MyPACSServer::AcceptAssociation(T_ASC_Network* net, DcmAssociationConfiguration& asccfg)
{
	return OFCondition();
}

OFCondition MyPACSServer::ProcessCommands(T_ASC_Association* assoc)
{
	return OFCondition();
}

OFCondition MyPACSServer::EchoSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID)
{
	return OFCondition();
}

OFCondition MyPACSServer::StoreSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID)
{
	return OFCondition();
}

OFCondition MyPACSServer::FindSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID)
{
	return OFCondition();
}

OFCondition MyPACSServer::GetSCP(T_ASC_Association* assoc, T_DIMSE_Message* msg, T_ASC_PresentationContextID presID)
{
	return OFCondition();
}

std::list<std::string> MyPACSServer::GetLocalPathFromDB(std::string selectSQL)
{
	return std::list<std::string>();
}
