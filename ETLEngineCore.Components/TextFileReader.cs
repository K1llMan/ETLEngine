using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ELTEngineCore.Graph;

namespace ELTEngineCore.Components
{
    public class TextFileReader : Node
    {
        #region Основные функции

        public TextFileReader(MetaData meta) : base(meta)
        {
            
        }

        public override void Execute()
        {
            /*
            if (!parameters.ContainsKey("searchPath"))
                return;
            
            Stream fs = null;
            StreamReader sr = null;

            string dirName = Path.GetDirectoryName(parameters["searchPath"]);
            if (string.IsNullOrEmpty(dirName))
                return;

            FileInfo[] files = new DirectoryInfo(dirName).GetFiles(parameters["searchPath"].Replace(dirName, string.Empty));

            foreach (FileInfo file in files)
                try
                {
                    fs = new FileStream(file.FullName, FileMode.Open);
                    sr = new StreamReader(fs, Encoding.GetEncoding(Convert.ToInt32(parameters["encoding"])));

                    string[] rows = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    foreach (string row in rows)
                    {
                        List<object> values = new List<object>();
                        int i = 0;
                        foreach (string field in row.Split(';'))
                        {
                            values.Add(field);
                            i++;
                        }

                        OutputPorts[0].Write(values);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (sr != null)
                        sr.Close();
                    if (fs != null)
                        fs.Close();
                }
            */
        }

        #endregion Основные функции
    }
}
