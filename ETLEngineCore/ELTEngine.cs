using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using ELTEngineCore.Configuration;
using ELTEngineCore.Database;
using ELTEngineCore.Graph;
using ELTEngineCore.Staging;

namespace ELTEngineCore
{
    /// <summary>
    /// Главный класс движка
    /// </summary>
    public class ELTEngine
    {
        #region Поля

        private ELTConfiguration config;

        private ELTDatabase db;

        private Dictionary<string, Type> importedTypes;
        private Dictionary<string, MetaData> metaList;

        private List<Stage> stages;


        #endregion Поля

        #region Вспомогательные функции

        private string Tabs(int n)
        {
            return new string('\t', n);
        }

        /// <summary>
        /// Добавляет в словарь все типы элементов графа
        /// </summary>
        private string AddAssemblyTypes(Assembly assembly)
        {
            List<Type> types = assembly.GetTypes().Where(t => typeof(GraphElement).IsAssignableFrom(t) && !importedTypes.ContainsKey(t.Name)).ToList();

            foreach (Type type in types)
                importedTypes.Add(type.Name, type);

            return $"\r\n{Tabs(1)}" + string.Format("{0} ({1}): \r\n{2}", assembly.GetName().Name, assembly.GetName().Version,
                types.Count == 0 ? $"{Tabs(2)}Пропущено" : string.Join("\r\n", types.Select(t => $"{Tabs(2)}{t.Name}")));
        }

        /// <summary>
        /// Загрузка типов элементов графа из плагинов
        /// </summary>
        private void LoadPlugins()
        {
            importedTypes = new Dictionary<string, Type>();

            StringBuilder typesInfo = new StringBuilder();
            typesInfo.Append(AddAssemblyTypes(Assembly.GetExecutingAssembly()));

            foreach (string path in config.PluginsPath.Split(';'))
            {
                // Если корень диска не найден в пути, то директория ищется относительно рабочей
                DirectoryInfo pluginsDir = new DirectoryInfo(
                    string.IsNullOrEmpty(Path.GetPathRoot(path)) 
                    ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + path
                    : path);

                typesInfo.Append($"\r\n\r\n{pluginsDir.FullName}:");

                foreach (var file in pluginsDir.GetFiles().Where(x => x.Name.ToLower().EndsWith(".dll")))
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFile(file.FullName);
                        typesInfo.Append(AddAssemblyTypes(assembly));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            Logger.WriteToTrace(string.Format("Из плагинов загружены следующие типы: {0}", typesInfo) , TraceMessageKind.Information);
        }

        #region Загрузка графа

        /// <summary>
        /// Создание объекта с загруженным типом
        /// </summary>
        public T GetInstance<T>(string type)
        {
            if (importedTypes.ContainsKey(type))
                return (T)Activator.CreateInstance(importedTypes[type]);
            return (T)(object)null;
        }

        /// <summary>
        /// Получение атрибута
        /// </summary>
        private string GetAttr(XmlNode node, string attr)
        {
            if (node.Attributes == null || node.Attributes[attr] == null)
                return string.Empty;

            return node.Attributes[attr].Value;
        }

        /// <summary>
        /// Заполнение наборов метаданных
        /// </summary>
        private void LoadMeta(XmlDocument doc)
        {
            XmlNodeList nodes = doc.SelectNodes("//MetaData");
            foreach (XmlNode node in nodes)
            {
                List<MetaRecord> list = node.SelectNodes("MetaRecord").Cast<XmlNode>().Select(n => new MetaRecord {
                    From = GetAttr(n, "from"),
                    To = GetAttr(n, "to"),
                    Type = Type.GetType(string.Format("System.{0}", GetAttr(n, "type")))
                }).ToList();

                metaList.Add(GetAttr(node, "id"), new MetaData(list));
            }
        }

        /// <summary>
        /// Загрузка узлов графа
        /// </summary>
        private List<Node> LoadStageNodes(XmlNode stageNode)
        {
            List<Node> graphNodes = new List<Node>();

            XmlNodeList nodes = stageNode.SelectNodes("Node");
            foreach (XmlNode node in nodes)
            {
                string id = GetAttr(node, "id");
                string typeName = GetAttr(node, "type");

                // Создание элемента и инициализация параметров
                Dictionary<string, string> parameters = node.Attributes.Cast<XmlAttribute>()
                    .Select(n => n.Name)
                    .Except(new string[] { "id", "type" })
                    .ToDictionary(s => s, s => GetAttr(node, s));

                foreach (XmlNode attrNode in node.SelectNodes("attr"))
                {
                    string attrName = GetAttr(attrNode, "name");
                    if (!parameters.ContainsKey(attrName))
                        parameters.Add(attrName, attrNode.InnerText.Trim());
                }

                Node element = (Node)GetInstance<object>(typeName);
                if (element == null)
                    continue;

                element.ID = id;
                element.Init(parameters);

                graphNodes.Add(element);
            }

            return graphNodes;
        }
        
        /// <summary>
        /// Загрузка рёбер графа и формирование связей между узлами
        /// </summary>
        private List<Edge> LoadStageEdges(XmlNode stageNode, List<Node> graphNodes)
        {
            List<Edge> graphEdges = new List<Edge>();

            XmlNodeList nodes = stageNode.SelectNodes("Edge");
            foreach (XmlNode node in nodes)
            {
                string fromStr = GetAttr(node, "from");
                string toStr = GetAttr(node, "to");
                string meta = GetAttr(node, "metadata");

                // Ребро, не соединяющее узлы
                if (string.IsNullOrEmpty(fromStr) || string.IsNullOrEmpty(toStr) ||
                    !metaList.ContainsKey(meta))
                    continue;

                string fromName = fromStr.Split(':')[0];
                int fromNum = Convert.ToInt32(fromStr.Split(':')[1]);

                string toName = toStr.Split(':')[0];
                int toNum = Convert.ToInt32(toStr.Split(':')[1]);

                // Узлов не существует
                if (graphNodes.All(n => n.ID != fromName) || graphNodes.All(n => n.ID != fromName))
                    continue;

                // Связываем входящие и исходящие порты
                Edge edge = new Edge(fromName, toName) { ID = GetAttr(node, "id") };

                Node fromNode = graphNodes.First(n => n.ID == fromName);
                if (!fromNode.OutputPorts.ContainsKey(fromNum))
                    fromNode.OutputPorts[fromNum] = new OutputPort();

                edge.InputPorts[0] = new InputPort(fromNode.OutputPorts[fromNum]);
                edge.OutputPorts[0] = new OutputPort();

                graphNodes.First(n => n.ID == toName).InputPorts[toNum] = new InputPort(edge.OutputPorts[0]);

                graphEdges.Add(edge);
            }

            return graphEdges;
        }

        #endregion Загрузка графа

        /// <summary>
        /// Загрузка и проверка графа
        /// </summary>
        private bool LoadGraph(string graphPath)
        {
            if (!File.Exists(graphPath))
                return false;

            XmlDocument doc = new XmlDocument();
            doc.Load(graphPath);

            LoadMeta(doc);

            // Добавние этапов
            foreach (XmlNode stageNode in doc.SelectNodes("//Stage").Cast<XmlNode>().OrderBy(n => Convert.ToInt32(GetAttr(n, "num"))))
            {
                string desc = GetAttr(stageNode, "desc");

                List<Node> nodes = LoadStageNodes(stageNode);
                List<Edge> edges = LoadStageEdges(stageNode, nodes);
                stages.Add(new Stage(desc, nodes, edges));
            }

            return true;
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public ELTEngine(ELTConfiguration configuration)
        {
            config = configuration;

            metaList = new Dictionary<string, MetaData>();
            stages = new List<Stage>();

            // Соединение с базой
            db = new ELTDatabase();
            db.Connect(config.ConnectionString);
            db.BeginTransaction();
        }

        /// <summary>
        /// Основная функция исполнения графа
        /// </summary>
        public void RunGraph(string graphPath)
        {
            Logger.Initialize(Path.GetFileNameWithoutExtension(graphPath), config.LogsPath, true);
            
            LoadPlugins();

            if (!LoadGraph(graphPath))
                return;

            int stageNum = 0;

            // Выполнение графа
            foreach (Stage stage in stages)
            {
                Logger.WriteToTrace(string.Format("Запуск этапа \"{0}\". Описание этапа: {1}.", stageNum, stage.Description), 
                    TraceMessageKind.Information);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                stage.Execute();

                sw.Stop();
                Logger.WriteToTrace(string.Format("Время обработки : {0} мс", sw.ElapsedMilliseconds));

                stageNum++;
            }

            Logger.CloseLogFile();
        }

        #endregion Основные функции
    }
}
