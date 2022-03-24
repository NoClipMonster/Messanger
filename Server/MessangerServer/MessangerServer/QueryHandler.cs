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

        byte[] CommandHandler(Data.Command command)
        {
            if ((command.type != Data.Command.CommandType.Connection && command.type != Data.Command.CommandType.Registration) && !dbClientData.CheckSession(command.SessionId))
                return protocol.SerializeToByte(Data.StatusType.InvalidSessionId, Data.MessageType.Status);

            switch (command.type)
            {
                case Data.Command.CommandType.Connection:
                    if (dbClientData.CheckPass(command.connection.Login, command.connection.Password))
                        return protocol.SerializeToByte(dbClientData.AddSession(command.connection.Login), Data.MessageType.Answer);
                    else return protocol.SerializeToByte(Data.StatusType.AuthorizationDenied, Data.MessageType.Status);

                case Data.Command.CommandType.Disconnection:
                    dbClientData.DeleteSession(command.SessionId);
                    return protocol.SerializeToByte(Data.StatusType.Ok, Data.MessageType.Status);

                case Data.Command.CommandType.Registration:
                    if (dbClientData.AddUser(command.registration))
                        return protocol.SerializeToByte(dbClientData.AddSession(command.registration.Login), Data.MessageType.Answer);
                    else return protocol.SerializeToByte(Data.StatusType.AuthorizationDenied, Data.MessageType.Status);

                case Data.Command.CommandType.FindUsers:
                    return protocol.SerializeToByte(dbClientData.FindUsers(command.findUser.Id), Data.MessageType.Answer);

                case Data.Command.CommandType.GetMessages:
                    return protocol.SerializeToByte(dbClientData.GetMessages(command.getMessages.Id, command.getMessages.Start, command.getMessages.End), Data.MessageType.Answer);

                case Data.Command.CommandType.GetFile:
                    return protocol.SerializeToByte(dbClientData.GetFile(command.getFile.Id), Data.MessageType.Answer);
            }
            return protocol.SerializeToByte(Data.StatusType.Error, Data.MessageType.Status);
        }
        public byte[] Answer(dynamic query)
        {
            if (query is Data.DirectMessage || query is Data.GroupMessage)
            {
                dbClientData.AddMessage(query);
                return protocol.SerializeToByte(Data.StatusType.Ok, Data.MessageType.Status);
            }
            if (query is Data.Command command)
            {
                return CommandHandler(command);
            }
            return protocol.SerializeToByte(Data.StatusType.Error, Data.MessageType.Status);
        }

    }
}
