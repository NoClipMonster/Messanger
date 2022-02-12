using LinqToDB;
using LinqToDB.Mapping;

namespace MessangerServer
{
    public class DbClientData : LinqToDB.Data.DataConnection
    {
        const string str = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\iaved\Desktop\Messanger\Server\myClientBase.mdf;Integrated Security=True;Connect Timeout=30";
        public DbClientData() : base(ProviderName.SqlServer2012, str)
        {
            users = GetTable<User>();
            messages = GetTable<Message>();
        }
        public ITable<User> users;
        public ITable<Message> messages;

        public bool CheckPass(string login, string pass)
        {
            var query = from us in users
                        where us.Id == login
                        select us;
            var arrQuery = query.ToArray();

            if (arrQuery.Length == 0)
                return false;
            if (arrQuery[0].Password == pass)
                return true;
            return false;
        }
        public bool AddUser(string login, string pass)
        {
            try
            {
                this.Insert(new User() { Id = login, Password = pass });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<Message> SendedMessages(string userId)
        {
            return (from p in messages
                    where p.SenderID == userId
                    orderby p.Date descending
                    select p).ToList();
        }
        public List<Message> ResivedMessages(string userId)
        {
            return (from p in messages
                    where p.RecipientID == userId
                    orderby p.Date descending
                    select p).ToList();
        }
        public void AddMessage(Message message) => this.Insert(message);

    }

    [Table("Users")]
    public class User
    {
        [Column("Id")]
        public string Id { get; set; } = "";

        [Column("Password")]
        public string Password { get; set; } = "";
    }

    [Table("Messages")]
    public class Message
    {

        [Column("Id")]
        public int Id { get; set; }
        [Column("SenderID")]
        public string SenderID { get; set; } = "";

        [Column("RecipientID")]
        public string RecipientID { get; set; } = "";

        [Column("Date")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Column("Message")]
        public string Text { get; set; } = "";

        [Column("File")]
        public byte[]? File { get; set; }
    }
}
