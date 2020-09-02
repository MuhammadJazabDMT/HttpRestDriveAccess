using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpRestDriveAccess.Helpers
{
    public static class Logs
    {
        public static void LogHandler(string text)
        {
            string loggingingFolder = $"{Path.GetPathRoot(Environment.SystemDirectory)}Google Drive Logs";
            if (!Directory.Exists(loggingingFolder)) Directory.CreateDirectory(loggingingFolder);

            var path = Path.Combine(loggingingFolder, "logs.txt");

            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write);
            fs.Close();

            using (StreamWriter sr = new StreamWriter(path, true, Encoding.ASCII))
            {
                sr.Write($"{text}\n\n");
                sr.Close();
            }
        }
    }
}
