using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MessangerApp2._0.Classes
{
    public class MessagesStorage
    {
        FileLoader<List<Data.Answer.Message>> loader;
        string path = @"Messages.json";
        public MessagesStorage()
        {
            loader = new(path);
            if(loader.Data==null)
                loader.Data = new List<Data.Answer.Message>();
        }
       
        public void Add(Data.Answer.Message[] messages)
        {
            this.loader.Data.AddRange(messages);
            loader.Save();
        }
        public void Add(Data.Answer.Message message)
        {
            loader.Data.Add(message);
            loader.Save();
        }
        public Data.Answer.Message[] GetAll()
        {
            return loader.Data.ToArray()??Array.Empty<Data.Answer.Message>();
        }
        public void DeleteAll()
        {
            loader.Data = new();
            loader.Save();
        }
    }
}
