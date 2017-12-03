using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ETLEngineCore
{
    public static class StringResources
    {
        private static Dictionary<string, Dictionary<string, string>> resources;

        /// <summary>
        /// Загружает строковые данные из ресурсной секции сборки
        /// </summary>
        private static void LoadFromXml(string fileName)
        {
            resources = new Dictionary<string, Dictionary<string, string>>();

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ETLEngineCore.Logger.StringResources.xml");

            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            foreach (XmlNode node in doc.SelectSingleNode("NewDataSet").ChildNodes)
            {
                if (!resources.ContainsKey(node.Name))
                    resources[node.Name] = new Dictionary<string, string>();

                string key = node.SelectSingleNode("Value").InnerText;
                string value = node.SelectSingleNode("String").InnerText;
                if (!resources[node.Name].ContainsKey(key))
                    resources[node.Name][key] = value;
            }
        }

        /// <summary>
        /// Получить значение строковой константы
        /// </summary>
        public static string GetLine<T>(T value)
       {
            if (resources == null)
                LoadFromXml("");

            string typeName = value.GetType().Name;
            if (!resources.ContainsKey(typeName))
                typeName = "Constants";

            if (!resources[typeName].ContainsKey(value.ToString()))
                return string.Empty;

            return resources[typeName][value.ToString()];
        }

    }
}
