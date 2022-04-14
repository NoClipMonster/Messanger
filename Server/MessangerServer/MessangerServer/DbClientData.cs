using LinqToDB;
using LinqToDB.Mapping;
using static Protocol.Data;

namespace MessangerServer
{
    public class DbClientData : LinqToDB.Data.DataConnection
    {
        const string str = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\iaved\Desktop\Messanger\Server\myClientBase.mdf;Integrated Security=True;Connect Timeout=30";
        public DbClientData() : base(ProviderName.SqlServer2012, str)
        {
            activeSessions = GetTable<ActiveSession>();
            files = GetTable<File>();
            groups = GetTable<Group>();
            memberships = GetTable<Membership>();
            messages = GetTable<Message>();
            users = GetTable<User>();
        }

        #region Sessions
        ITable<ActiveSession> activeSessions;

        public bool CheckSession(byte[] sessionID)
        {
           
           // Task.Run(() => (activeSessions.Delete(aS => aS.LastActiveTime.AddDays(10) < DateTime.Now)));
            var sessions = activeSessions.Where(s => s.SessionID == sessionID).FirstOrDefault();
            if (sessions!=null && sessions.SessionID != Array.Empty<byte>())
            {
                if (sessions.LastActiveTime.AddDays(10) < DateTime.Now)
                {
                    activeSessions.Delete(s => s.SessionID == sessionID);
                    return false;
                }
                sessions.LastActiveTime = DateTime.Now;
                return true;
            }
            return false;
        }

        public void DeleteSession(byte[] id)
        {
            activeSessions.Delete(session => session.SessionID == id);
        }

        public Answer.Session AddSession(string UserId)
        {
            activeSessions.Delete(aS => aS.UserId == UserId); 
            
            byte[] SessionId = new byte[16];
            Random rand = new((int)DateTime.Now.Ticks);
            for (int i = 0; i < SessionId.Length; i++)
            {
                SessionId[i] = ((byte)rand.Next(0, 256));
            }
            this.Insert(new ActiveSession() { SessionID = SessionId, UserId = UserId, LastActiveTime = DateTime.Now });
            return new() { SessionId = SessionId };
        }
        #endregion

        #region Files

        ITable<File> files;

        public Answer.File GetFile(int FileId)
        {
            File f = files.FirstOrDefault(f => f.Id == FileId);
            return new() { FileData = f.FileData, FileName = f.FileName, Id = ID };
        }
        
        public int AddFile(string Name, byte[] fileData)
        {
            this.Insert(new File() { FileName = Name, FileData = fileData, Id = files.Count() });
            return files.Count() - 1;
        }

        #endregion

        #region Groups
        ITable<Group> groups;
        public bool CreateGroup(Command.CreateGroup group)
        {
            return CreateGroup(group.GroupId, group.GroupName, group.Description,group.AdminId);
        }
       
        public bool CreateGroup(string groupId, string groupName, string Description,string AdminID)
        {
            if (!groups.Any(g => g.Id == groupId))
            {
                this.Insert(new Group() { Id = groupId, Description = Description, Name = groupName });
                this.Insert(new Membership() { GroupId = groupId, UserId = AdminID, Level = 0 });
                return true;
            }
            return false;
        }

        public Answer.Group FindGroup(string groupId)
        {
            Group g = groups.FirstOrDefault(g => g.Id == groupId);
           if (g == null)
            {
                return new();
            }
            return new() { Id = g.Id, Description = g.Description, Name = g.Name };
        }

        #endregion

        #region Memberships

        ITable<Membership> memberships;

        public bool AddMembership(string UserId, string GroupId, int Level = 5)
        {
            if (users.Any(U => U.Id == UserId) && groups.Any(G => G.Id == GroupId) && !(from m in (from M in memberships where M.GroupId == GroupId select M) where m.UserId == UserId select m).Any())
            {
                this.Insert(new Membership() { GroupId = GroupId, UserId = UserId, Level = Level });
                return true;
            }
            return false;
        }
        public Answer.Membership[] GetMembershipsByUser(string UserId)
        {
            var msh = memberships.Where(m => m.UserId == UserId).DefaultIfEmpty().ToArray();
            List<Answer.Membership> members = new(msh.Length);
            foreach (Membership ms in msh)
            {
                members.Add(new Answer.Membership() { GroupId = ms.GroupId, Level = ms.Level, UserId = ms.UserId });
            }
            return members.ToArray();
        }
        public Answer.Membership[] GetMembershipsByGroup(string GroupId)
        {
            var msh = memberships.Where(m => m.GroupId == GroupId).DefaultIfEmpty().ToArray();
            List<Answer.Membership> members = new (msh.Length);
            foreach (Membership ms in msh)
            {
                members.Add(new Answer.Membership() { GroupId = ms.GroupId, Level = ms.Level, UserId = ms.UserId });
            }
            return members.ToArray();
        }

        #endregion

        #region Messages

        ITable<Message> messages;
        public Answer.Message[] GetMessages(string userId)
        {
            return GetMessages(userId, DateTime.MinValue, DateTime.MaxValue);
        }

        public Answer.Message[] GetMessages(string userId, DateTime From)
        {
            return GetMessages(userId, From, DateTime.MaxValue);
        }

        public Answer.Message[] GetMessages(string userId, DateTime From, DateTime To)
        {
            var ms = messages.Where(m => (m.SenderID == userId || m.RecipientID == userId) && m.Date.Between(From, To)).DefaultIfEmpty().ToArray();
            List<Answer.Message> mesAnsw = new();
            foreach (Message m in ms)
                mesAnsw.Add(new() { Date = m.Date, RecipientID = m.RecipientID, FileId = m.FileId, IsGroup = m.IsGroup, SenderID = m.SenderID, Text = m.Text });
            return mesAnsw.ToArray();
        }

        public void AddMessage(string Sender, string Recipient, string message, DateTime date, bool isGroup = false, int? fileId = null) => this.Insert(new Message() { SenderID = Sender, RecipientID = Recipient, Text = message, Date = date, FileId = fileId, IsGroup = isGroup });

        public void AddMessage(Protocol.Data.DirectMessage ms)
        {
            int? fileID = null;
            if (ms.dataset != null)
            {
                fileID = AddFile(ms.dataset.nameOfObject, ms.dataset.someObject);
            }
            AddMessage(ms.senderLogin, ms.targetLogin, ms.message, ms.sendingTime, false, fileID);
        }

        public void AddMessage(Protocol.Data.GroupMessage ms)
        {
            int? fileID = null;
            if (ms.dataset != null)
            {
                fileID = AddFile(ms.dataset.nameOfObject, ms.dataset.someObject);
            }
            AddMessage(ms.senderLogin, ms.groupId, ms.message, ms.sendingTime, true, fileID);
        }
        #endregion

        #region Users
        ITable<User> users;

        public bool AddUser(Protocol.Data.Command.Registration user)
        {
            return AddUser(user.Login, user.Password, user.Name, user.Description);
        }
        public bool AddUser(string login, string pass, string Name = "", string Description = "")
        {
            try
            {
                this.Insert(new User() { Id = login, Password = pass, Name = (Name == "") ? login : Name, Description = Description });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Answer.User FindUser(string login)
        {
            User? user = users.FirstOrDefault(us => us.Id == login);
            if (user == null)
            {
                return new();
            }
            return new() { Id = user.Id,Description = user.Description,Name = user.Name };
        }
        public Answer.User FindUser(byte[] sessionId)
        {
            User? user = users.FirstOrDefault(us=>us.Id == activeSessions.FirstOrDefault(ses => ses.SessionID == sessionId).UserId);
            if (user == null)
            {
                return new();
            }
            return new() { Id = user.Id, Description = user.Description, Name = user.Name };
        }

        public Answer.User[] FindUsers(string login)
        {
            var us = (from s in users where s.Id.Contains(login) select s).Take(5).DefaultIfEmpty().ToArray();
            List<Answer.User> usersAns = new(us.Length);
            foreach (User u in us)
                usersAns.Add(new() { Id = u.Id, Description = u.Description, Name = u.Name });
            return usersAns.ToArray();
        }

        public bool CheckPass(string login, string pass)
        {
            return users.Any(user => user.Id == login && user.Password == pass);
        }
        #endregion

    }

    #region Классы таблиц
    [Table("ActiveSessions")]
    public class ActiveSession
    {
        [Column("SessionID")]
        public byte[] SessionID { get; set; } = Array.Empty<byte>();

        [Column("UserId")]
        public string UserId { get; set; } = String.Empty;

        [Column("LastActiveTime")]
        public DateTime LastActiveTime { get; set; } = DateTime.MinValue;
    }

    [Table("Files")]
    public class File
    {
        [Column("Id")]
        public int Id { get; set; } = -1;

        [Column("File")]
        public byte[] FileData { get; set; } = Array.Empty<byte>();

        [Column("FileName")]
        public string FileName { get; set; } = String.Empty;
    }

    [Table("Groups")]
    public class Group
    {
        [Column("Id")]
        public string Id { get; set; } = String.Empty;

        [Column("Name")]
        public string Name { get; set; } = String.Empty;

        [Column("Description")]
        public string Description { get; set; } = String.Empty;
    }

    [Table("Memberships")]
    public class Membership
    {
        [Column("UserId")]
        public string UserId { get; set; } = String.Empty;

        [Column("GroupId")]
        public string GroupId { get; set; } = String.Empty;

        [Column("Level")]
        public int Level { get; set; } = 5;
    }

    [Table("Messages")]
    public class Message
    {
        [Column("SenderID")]
        public string SenderID { get; set; } = "";

        [Column("RecipientID")]
        public string RecipientID { get; set; } = "";

        bool isgroup = false;
        [Column("IsGroup")]
        public bool IsGroup { get { return isgroup; } set { isgroup = bool.Parse(value.ToString()); } }

        [Column("Date")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Column("Message")]
        public string Text { get; set; } = "";

        [Column("FileId")]
        public int? FileId { get; set; } = -1;

    }

    [Table("Users")]
    public class User
    {
        [Column("Id")]
        public string Id { get; set; } = "";

        [Column("Password")]
        public string Password { get; set; } = "";

        [Column("Name")]
        public string Name { get; set; } = "";

        [Column("Description")]
        public string Description { get; set; } = "";
    }
    #endregion  
}
