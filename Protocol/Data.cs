using System.ComponentModel;

namespace Protocol
{
    public class Data
    {

        public class Answer
        {
            public class Session
            {
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

                public string Password { get; set; } = "";

                public string Name { get; set; } = "";

                public string Description { get; set; } = "";
            }

            public class Membership
            {
                public string UserId { get; set; } = String.Empty;

                public string GroupId { get; set; } = String.Empty;

                public string Level { get; set; } = String.Empty;
            }
        }
        public enum MessageType { Direct, Group, Command, Status, Answer };
        public enum StatusType { Ok, AuthorizationDenied, InvalidSessionId, Error, TimeOut };

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

        public class Command : BaseQuery
        {
            public enum CommandType { Connection, Disconnection, Registration, FindUsers, GetMessages, GetFile }
            public CommandType type;
            public Connection? connection;
            public Registration? registration;
            public FindUser? findUser;
            public GetMessages? getMessages;
            public GetFile? getFile;

            public class Connection
            {
                public string Login = String.Empty;
                public string Password = String.Empty;
            }
            public class Registration
            {
                public string Login = String.Empty;
                public string Password = String.Empty;
                public string Name = String.Empty;
                public string Description = String.Empty;
            }
            public class FindUser
            {
                public string Id = String.Empty;
            }
            public class GetMessages
            {
                public string Id = String.Empty;
                public DateTime Start = DateTime.MinValue;
                public DateTime End = DateTime.MaxValue;
            }
            public class GetFile
            {
                public int Id = 0;
            }
        }

        public class DirectMessage : Message
        {
            public string targetLogin = String.Empty;
        }

        public class GroupMessage : Message
        {
            public string groupId = String.Empty;
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
