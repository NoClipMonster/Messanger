using LinqToDB;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
namespace MessangerApp2._0
{
    public class DbClientData : LinqToDB.Data.DataConnection
    {
        const string str = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\iaved\Desktop\Messanger\Server\myClientBase.mdf;Integrated Security=True;Connect Timeout=30";
        public DbClientData() : base(ProviderName.SqlServer2012, str)
        {         
            messages = GetTable<Message>();
        }
        public ITable<Message> messages;

        public List<Message> getMessages(string user1Id,string user2Id,DateTime dateTime)
        {
            return (from p in messages
                    where (((p.SenderID == user1Id && p.SenderID == user2Id)||(p.SenderID == user2Id && p.SenderID == user1Id))&&p.Date>=dateTime)
                    orderby p.Date descending
                    select p).ToList();
        }      
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
