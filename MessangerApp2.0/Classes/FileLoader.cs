using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MessangerApp2._0.Classes
{
    internal class FileLoader<T>
    {
        string path;
        [JsonProperty("Data")]
        internal T Data;
        public FileLoader(string Path)
        {
            path = Path;
            try
            {
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Save();
            }
        }

        public void Load()
        {
            if (System.IO.File.Exists(path))
            {
                object? obj = JsonConvert.DeserializeObject<T>(System.IO.File.ReadAllText(path));
                if (obj is T data)
                {
                    this.Data = data;
                }
                else
                { 
                    throw new Exception("Данные в файле не соответствуют настройкам");
                }
            }
            else
            {
                throw new Exception("Отсутствует файл настроек");
            }
        }

        public void Save()
        {
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(Data, Formatting.Indented));
        }
    }
}
