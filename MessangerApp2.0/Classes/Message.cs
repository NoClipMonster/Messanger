using System;

namespace MessangerApp2._0
{
    public partial class MainWindow
    {
        class Message
        {
            public Message(string sender,string receiver, string text, DateTime dateTime)
            {
                this.text = text;
                this.dateTime = dateTime;
                this.sender = sender;
                this.receiver = receiver;
            }
            public string receiver;
            public string sender;
            public string text;
            public DateTime dateTime;
        }
    }
}
