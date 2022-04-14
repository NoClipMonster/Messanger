using System.Collections.Generic;

namespace MessangerApp2._0.Classes
{
    public class ContactsStorage
    {
        FileLoader<List<Contact>> loader;
        string path = @"Contacts.json";
        public ContactsStorage()
        {
            loader = new(path);
            if (loader.Data == null)
                loader.Data = new List<Contact>();
        }
        public class Contact
        {
            public string Name = "";
            public string Id = "";
            public string Description = "";
            public bool IsGroup = false;
            public bool HasNewMessage = false;
        }
        public void SetNewMessage(string Id, bool Show = true)
        {
            int cont = loader.Data.FindIndex(x => x.Id == Id);
            if (cont != -1)
            {
                loader.Data[cont].HasNewMessage = Show;
                loader.Save();
            }
        }
        public bool GetNewMessage(string Id)
        {
            int cont = loader.Data.FindIndex(x => x.Id == Id);
            if (cont != -1)
                return loader.Data[cont].HasNewMessage;
            return false;
        }
        public void Add(string Id, string Name, string Description, bool IsGroup = false)
        {
            loader.Data.Add(new Contact() { Id = Id, Name = Name, Description = Description, IsGroup = IsGroup });
            loader.Save();
        }
        public void Add(Protocol.Data.Answer.Group group)
        {
            loader.Data.Add(new Contact() { Id = group.Id, Name = group.Name, Description = group.Description, IsGroup = true });
            loader.Save();
        }
        public void Add(Protocol.Data.Answer.User user)
        {
            loader.Data.Add(new Contact() { Id = user.Id, Name = user.Name, Description = user.Description, IsGroup = false });
            loader.Save();
        }
        public void DeleteAll()
        {
            loader.Data.Clear();
            loader.Save();
        }
        public Contact[] GetAll()
        {
            return loader.Data.ToArray();
        }
    }
}
