using Newtonsoft.Json;
using SIMA.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Helper
{
    public class JsonFile<T>
    {
        private string _fileName = "stock.json";

        private string _filePath ;

        private List<T> _servicesList;

        public string FilePath { get => _filePath; set => _filePath = value; }

        public string FileName { get => _fileName; set => _fileName = value; }

        public List<T> ServicesList => _servicesList;

        public JsonFile() {
            _filePath = parsePath(_fileName, "Infrastructure/data");
        }

        public JsonFile(string filename)
        {

            _fileName = filename;
            _filePath = parsePath(_fileName, "Infrastructure/data");
        }

        public JsonFile(string filename, string path)
        {

            _fileName = filename;
            _filePath = parsePath(_fileName, path);
        }


        private string parsePath(string filename, string path) {
            string rpath = string.Empty;
            string[] sppath = path.Split('/');
            if (!string.IsNullOrEmpty(path)) {
                foreach (var item in sppath) {
                    rpath = Path.Combine(rpath, item);
                }
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rpath, filename);
        }

        public void  loadData()
        
        {
            try
            {
                _servicesList = File.Exists(_filePath) ? JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(_filePath)) ?? new List<T>() : new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos: {ex.Message}");
                _servicesList = new List<T>();
            }
        }

        public async  Task<bool> SaveData()
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(_servicesList, Newtonsoft.Json.Formatting.Indented);
                await File.WriteAllTextAsync(FilePath, jsonContent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar datos: {ex.Message}");
                return false;
            }
        }
    }
}


