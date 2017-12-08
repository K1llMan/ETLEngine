using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ELTEngineCore.Configuration
{
    /// <summary>
    /// Конфигурация движка
    /// </summary>
    public class ELTConfiguration
    {
        #region Поля

        private Dictionary<string, string> config;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Строка подключения к базе
        /// </summary>
        public string ConnectionString
        {
            get => GetParam("connectionString");
        }

        /// <summary>
        /// Путь к логам
        /// </summary>
        public string LogsPath
        {
            get => GetParam("logsPath");
        }

        /// <summary>
        /// Путь к плагинам
        /// </summary>
        public string PluginsPath
        {
            get => GetParam("pluginsPath");
        }

        #endregion Свойства

        #region Вспомогательные функции

        private string GetParam(string paramName)
        {
            if (!config.ContainsKey(paramName))
                return string.Empty;

            return config[paramName];
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public ELTConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            config = new Dictionary<string, string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            foreach (XmlNode node in doc.SelectNodes("//add"))
            {
                string key = node.Attributes["key"]?.Value;
                string value = node.Attributes["value"]?.Value;

                if (!string.IsNullOrEmpty(key) && !config.ContainsKey(key))
                    config.Add(key, value);
            }
        }

        #endregion Основные функции
    }
}
