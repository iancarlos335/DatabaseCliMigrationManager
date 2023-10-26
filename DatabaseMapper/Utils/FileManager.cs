using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMapper.Utils
{
    public class FileManager
    {
        public void CreateDirectory(string path)
        {
            ClearDirectory(path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void ClearDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);

                for (int i = 0; i < files.Length; i++)
                {
                    File.Delete(files[i]);
                }
            }
        }

        public void CreateTextFile(string path, string filename, string text)
        {
            string filePath = Path.Combine(path, filename);

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, text);
            }
        }

        public void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        public string JavascriptGetTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss");
        }
    }
}
