using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PhlozLib
{
    public class TextLog
    {
        public static void Log(string filename, string message)
        {
            TextWriter logfile = File.AppendText(filename);
            logfile.WriteLine(message);
            logfile.Close();
        }
    }
}
