using Protocol;
namespace MessangerServer
{
    public class QueryHandler
    {
        DbClientData dbClientData;
        Protocol.Protocol protocol;
        public QueryHandler()
        {
            this.dbClientData = new();
            protocol = new(4);
        }

        byte[] CommandHandler(dynamic command)
        {

            if (command is not Data.Answer.Session && command is not Data.Command.Connection && command is not Data.Command.Registration)
                if(!dbClientData.CheckSession(command.SessionId))
                return protocol.SerializeToByte(Data.StatusType.InvalidSessionId, Data.MessageType.Status);
           
            switch (command)
            {
                case Data.Command.Registration registration:
                    if (dbClientData.AddUser(registration))
                    {
                        return protocol.SerializeToByte(Data.StatusType.Authorized, Data.MessageType.Status);
                    }
                    else return protocol.SerializeToByte(Data.StatusType.UserExists, Data.MessageType.Status);

                case Data.Command.Connection connection:
                    if (dbClientData.CheckPass(connection.Login, connection.Password))
                    {
                        Data.Answer.Session session = dbClientData.AddSession(connection.Login);
                        session.User = dbClientData.FindUser(connection.Login);
                        return protocol.SerializeToByte(session, Data.MessageType.SessionId);
                    }
                    else return protocol.SerializeToByte(Data.StatusType.AuthorizationDenied, Data.MessageType.Status);

                case Data.Command.Disconnection disconnection:
                    dbClientData.DeleteSession(disconnection.SessionId);
                    return protocol.SerializeToByte(Data.StatusType.Ok, Data.MessageType.Status);

                case Data.Command.FindUser findUser:               
                    return protocol.SerializeToByte(dbClientData.FindUser(findUser.Id), Data.MessageType.User);

                case Data.Command.GetMessages getMessages:                   
                    return protocol.SerializeToByte(dbClientData.GetMessages(getMessages.Id, getMessages.Start, getMessages.End), Data.MessageType.Messages);

                case Data.Command.GetFile getFile:
                    return protocol.SerializeToByte(dbClientData.GetFile(getFile.Id), Data.MessageType.File);

                case Data.Answer.Session session:
                    return protocol.SerializeToByte(dbClientData.CheckSession(session.SessionId)? dbClientData.FindUser(session.SessionId) : Data.StatusType.InvalidSessionId,Data.MessageType.User);
                
                case Data.Command.CreateGroup createGroup:
                    return protocol.SerializeToByte(dbClientData.CreateGroup(createGroup)?Data.StatusType.GroupCreated:Data.StatusType.GroupExists, Data.MessageType.Status);

                case Data.Command.FindGroup findGroup:
                    return protocol.SerializeToByte(dbClientData.FindGroup(findGroup.GroupId), Data.MessageType.Group);
            }
            return protocol.SerializeToByte(Data.StatusType.Error, Data.MessageType.Status);
        }
        public byte[] Answer(dynamic query)
        {
           
            if ((query is Data.DirectMessage || query is Data.GroupMessage))
            {
                if (dbClientData.CheckSession(query.SessionId))
                {
                    dbClientData.AddMessage(query);
                    return protocol.SerializeToByte(Data.StatusType.Ok, Data.MessageType.Status);
                }
                else return protocol.SerializeToByte(Data.StatusType.InvalidSessionId, Data.MessageType.Status);
            }
            else
                return CommandHandler(query);
           
        }

    }
}
