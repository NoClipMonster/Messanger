using Newtonsoft.Json;
using System.ComponentModel;

namespace Protocol
{
    public class Data
    {

        public class Answer
        {
            public class Session
            {
                public User? User;
                public byte[] SessionId = Array.Empty<byte>();
            }

            public class File
            {
                public int Id { get; set; } = -1;

                public byte[] FileData { get; set; } = Array.Empty<byte>();

                public string FileName { get; set; } = String.Empty;
            }

            public class Group
            {
                public string Id { get; set; } = String.Empty;

                public string Name { get; set; } = String.Empty;

                public string Description { get; set; } = String.Empty;
            }

            public class Message
            {
                public string SenderID { get; set; } = "";

                public string RecipientID { get; set; } = "";

                bool isgroup;

                public bool IsGroup { get { return isgroup; } set { isgroup = bool.Parse(value.ToString()); } }


                public DateTime Date { get; set; } = DateTime.Now;

                public string Text { get; set; } = "";

                public int? FileId { get; set; }

            }

            public class User
            {
                public string Id { get; set; } = "";

                public string Name { get; set; } = "";

                public string Description { get; set; } = "";
            }

            public class Membership
            {
                public string UserId { get; set; } = String.Empty;

                public string GroupId { get; set; } = String.Empty;

                public int Level { get; set; } = 5;
            }
        }
        public enum MessageType { DirectMessage, GroupMessage, Command, Status, SessionId, Users, User, Messages, File, Group, Groups };
        public enum StatusType { Ok, AuthorizationDenied, UserExists, InvalidSessionId, Error, TimeOut, Authorized,GroupExists,GroupCreated };

        //TODO: уточнить сериализацию
        public class BaseQuery
        {
            public byte[] SessionId = Array.Empty<byte>();
        }

        public class Message : BaseQuery
        {
            public string senderLogin = String.Empty;
            public DateTime sendingTime = DateTime.Now;
            public string message = String.Empty;
            public Dataset? dataset;
        }

        public class Command
        {
            public enum CommandType { Connection, Disconnection, Registration,FindUser, FindUsers, GetMessages, GetFile, CheckSession, CreateGroup,FindGroup }
            public class BaseCommand : BaseQuery
            {
                public CommandType commandType;
            }
            public class Connection : BaseCommand
            {
                public string Login;
                public string Password;

                public Connection(string login, string password)
                {
                    commandType = CommandType.Connection;
                    Login = login;
                    Password = password;
                }

            }
            public class Disconnection : BaseCommand
            {
                public Disconnection(byte[] sessionId)
                {
                    SessionId = sessionId;
                    commandType = CommandType.Disconnection;
                }
            }
            public class Registration : BaseCommand
            {
                public string Login;
                public string Password;
                public string Name;
                public string Description;

                public Registration(string login, string password, string name, string description)
                {
                    commandType = CommandType.Registration;
                    Login = login;
                    Password = password;
                    Name = name;
                    Description = description;
                }

            }
            public class FindUser : BaseCommand
            {

                public string Id;

                public FindUser(byte[] sessionId, string id)
                {
                    SessionId = sessionId;
                    commandType = CommandType.FindUser;
                    Id = id;
                }

            }
            public class GetMessages : BaseCommand
            {
                public string Id;
                public DateTime Start;
                public DateTime End;

                public GetMessages(byte[] sessionId, string id, DateTime start, DateTime end)
                {
                    SessionId = sessionId;
                    commandType = CommandType.GetMessages;
                    Id = id;
                    Start = start;
                    End = end;
                }

            }
            public class GetFile : BaseCommand
            {
                public int Id;

                public GetFile(byte[] sessionId, int id)
                {
                    SessionId = sessionId;
                    commandType = CommandType.GetFile;
                    Id = id;
                }

            }

            public class FindGroup : BaseCommand
            {
                public string GroupId = "";
                public FindGroup(byte[] sessionId, string groupId)
                {
                    SessionId = sessionId;
                    commandType = CommandType.FindGroup;
                   
                    GroupId = groupId;
                   
                }
            }
            public class CreateGroup : BaseCommand
            {
                public string AdminId = "";
                public string GroupId = "";
                public string GroupName = "";
                public string Description = "";

                public CreateGroup(byte[] sessionId, string adminId, string groupId, string groupName, string description)
                {
                    SessionId = sessionId;
                    commandType = CommandType.CreateGroup;
                    AdminId = adminId;
                    GroupId = groupId;
                    GroupName = groupName;
                    Description = description;
                }

            }

        }

        public class DirectMessage : Message
        {
            public string targetLogin;
            [JsonConstructor]
            public DirectMessage(string targetLogin, string senderLogin, DateTime sendingTime, string message, Dataset? dataset, byte[] sessionId)
            {
                this.SessionId = sessionId;
                this.targetLogin = targetLogin;
                this.senderLogin = senderLogin;
                this.message = message;
                this.sendingTime = DateTime.Now;
                this.dataset = dataset;
            }
            public DirectMessage(byte[] sessionId, string targetLogin, string senderLogin, string message)
            {
                this.SessionId = sessionId;
                this.targetLogin = targetLogin;
                this.senderLogin = senderLogin;
                this.message = message;
                this.sendingTime = DateTime.Now;
                this.dataset = null;
            }
            public DirectMessage(byte[] sessionId, string targetLogin, string senderLogin, string message, DateTime sendingTime, Dataset? dataset)
            {
                this.SessionId = sessionId;
                this.targetLogin = targetLogin;
                this.senderLogin = senderLogin;
                this.message = message;
                this.sendingTime = sendingTime;
                this.dataset = dataset;
            }

        }

        public class GroupMessage : Message
        {
            public string groupId;
            public GroupMessage(byte[] sessionId, string groupId, string senderLogin,string message)
            {
                this.SessionId = sessionId;
                this.groupId = groupId;
                this.senderLogin = senderLogin;
                this.sendingTime = DateTime.Now;
                this.message = message;
                this.dataset = null;
            }
            [JsonConstructor]
            public GroupMessage(byte[] sessionId, string groupId, string senderLogin, DateTime sendingTime, string message, Dataset? dataset)
            {
                this.SessionId = sessionId;
                this.groupId = groupId;
                this.senderLogin = senderLogin;
                this.sendingTime = sendingTime;
                this.message = message;
                this.dataset = dataset;
            }
        }

        public class Dataset
        {
            public BackgroundWorker fileWaiter;
            public Dataset(string path)
            {
                fileWaiter = new BackgroundWorker();
                fileWaiter.DoWork += FileWaiter_DoWork;
                fileWaiter.RunWorkerAsync(path);

                void FileWaiter_DoWork(object? sender, DoWorkEventArgs e)
                {
                    if (e.Argument != null)
                    {
                        someObject = File.ReadAllBytes((string)e.Argument);
                        sizeOfObject = someObject.Length;
                        nameOfObject = new FileInfo((string)e.Argument).Name;
                    }
                }
            }

            public byte[] someObject = Array.Empty<byte>();

            public int sizeOfObject = -1;

            public string nameOfObject = String.Empty;

        }

    }

}
