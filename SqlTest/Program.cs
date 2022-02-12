using LinqToDB;
using LinqToDB.Mapping;

namespace SqlTest
{
    public class SqlTest
    {

        static void Main()
        {
            //  var db = new LinqToDB.Data.DataConnection( );
            var db = new DbClientData();

            if (db.users.First(x => x.Id == "someOne") == null)
                db.Insert(new User() { Id = "someOne", Password = "1234" });

            foreach (var user in db.users)
                Console.WriteLine("{0} п {1}", user.Id, user.Password);

            foreach (var message in db.messages)
                Console.WriteLine("Сообщение от {0} пользователю {1} с текстом {2}", message.SenderID, message.RecipientID, message.Text);

            foreach (var message in db.ResivedMessages("admin"))
                Console.WriteLine("{0} от {1} : {2}", message.Text, message.SenderID, message.Date);

            foreach (var message in db.SendedMessages("admin"))
                Console.WriteLine("{0} получатель {1} : {2}", message.Text, message.RecipientID, message.Date);

            Console.Read();
        }
    }

    public class DbClientData : LinqToDB.Data.DataConnection
    {
        public DbClientData() : base(ProviderName.SqlServer2012, @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\iaved\Desktop\Messanger\myDataBase.mdf;Integrated Security=True;Connect Timeout=30")
        {
            users = GetTable<User>();
            messages = GetTable<Message>();
        }
        public ITable<User> users;
        public ITable<Message> messages;

        public List<Message> SendedMessages(string userId)
        {
            var query = from p in messages
                        where p.SenderID == userId
                        orderby p.Date descending
                        select p;
            return query.ToList();
        }
        public List<Message> ResivedMessages(string userId)
        {
            var query = from p in messages
                        where p.RecipientID == userId
                        orderby p.Date descending
                        select p;
            return query.ToList();
        }
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
    }
}
