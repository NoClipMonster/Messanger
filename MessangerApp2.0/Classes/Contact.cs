using System.Collections.Generic;

namespace MessangerApp2._0
{


    public partial class MainWindow
    {
        class Contact
        {
            List<Message> messages;
            public string name;
            public ContactUC contactUC;
            public Contact(string name, ContactUC contactUC)
            {
                this.name = name;
                messages = new List<Message>();
                this.contactUC = contactUC;
            }
            public void AddMessage(Message message)
            {
                messages.Add(message);
                contactUC.ChangeMessage(message.text, message.dateTime);
            }
        }
    }
}
