using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using ETLEngineCore.Extensions;

namespace ETLEngineCore
{
    public static class Logger
    {
        #region Поля

        // По умолчанию для логгирования задаются все уровни
        private static TraceMessageKind[] levels = { TraceMessageKind.All };
        private static List<Writer> writers;

        private static string logsDir;

        // Событие записи в лог
        public class WriteEventArgs
        {
            public string Message { get; internal set; }
        }

        public delegate void WriteEventHandler(WriteEventArgs e);
        public static event WriteEventHandler WriteEvent;

        #endregion Поля

        #region Вспомогательные функции

        /// <summary>
        /// Запись сообщения в лог
        /// </summary>
        private static void WriteToTrace(string message, TraceMessageKind traceMessageKind, string category)
        {
            // Если уровень логирование недопустимый, то пропускаем запись
            if (!levels.Contains(TraceMessageKind.All) && !levels.Contains(traceMessageKind))
                return;

            foreach (Writer writer in writers)
                writer.Write(message, traceMessageKind, category);

            if (WriteEvent != null)
                WriteEvent(new WriteEventArgs { Message = message });
        }

        /// <summary>
        /// Инициализация логгеров по умолчанию
        /// </summary>
        private static void InitDefaultLoggers(string fileName, bool useConsole)
        {
            // В файл
            writers.Add(new Writer(new Dictionary<string, object> {
                { "Type", WriterType.File },
                { "File", logsDir + fileName + ".txt" },
            }));

            // В консоль
            if (useConsole)
                writers.Add(new Writer(new Dictionary<string, object> {
                    { "Type", WriterType.Console },
                }));
        }

        /// <summary>
        /// Преобразует текстовые поля конфига к нужному типу
        /// </summary>
        private static Dictionary<string, object> UpdateWriterDict(string fileName, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("Type"))
                return null;

            Dictionary<string, object> outDict = new Dictionary<string, object>();
            foreach (string key in dict.Keys)
                switch (key)
                {
                    case "Type":
                        outDict.Add(key, (WriterType)Enum.Parse(typeof(WriterType), dict[key].ToString()));
                        if ((WriterType)outDict[key] == WriterType.File && !dict.ContainsKey("File"))
                            return null;
                        break;
                    case "File":
                        outDict.Add(key, logsDir + dict[key].ToString().Replace("{filename}", fileName));
                        break;
                    case "Levels":
                        outDict.Add(key, dict[key].ToString().Split('|').Select(s => (TraceMessageKind)Enum.Parse(typeof(TraceMessageKind), s.Trim())).ToArray());
                        break;
                    default:
                        outDict.Add(key, dict[key]);
                        break;
                }

            return outDict;
        }

        private static void InitWriters(string[] loggerParams, string fileName, bool useConsole)
        {
            if (loggerParams == null)
            {
                InitDefaultLoggers(fileName, useConsole);
                return;
            }

            Func<string, string> getKey = s => s.GetMatches(@".+(?=\=)").First().Trim();
            Func<string, string> getValue = s => s.GetMatches(@"(?<=\=).+").First().Trim();

            try
            {
                string roolLevels = loggerParams.FirstOrDefault(s => s.IsMatch(@"RootLevels[\s]*="));
                if (!string.IsNullOrEmpty(roolLevels))
                    levels = getValue(roolLevels).Split('|')
                        .Select(s =>
                        {
                            TraceMessageKind k = (TraceMessageKind)Enum.Parse(typeof(TraceMessageKind), s.Trim());
                            return k != null ? k : TraceMessageKind.All;
                        })
                        .ToArray();

                string loggerStr = loggerParams.FirstOrDefault(s => s.IsMatch(@"Loggers[\s]*="));
                if (string.IsNullOrEmpty(loggerStr))
                {
                    InitDefaultLoggers(fileName, useConsole);
                    return;
                }
                string[] loggers = getValue(loggerStr).Split(',').Select(s => s.Trim()).ToArray();
                foreach (string logger in loggers)
                {
                    Dictionary<string, object> curLogParams = loggerParams
                        .Where(s => s.IsMatch(string.Format(@"{0}\..*=", logger)))
                        .Select(s => s.ReplaceRegex(logger + @"\.", string.Empty))
                        .ToDictionary(s => getKey(s), s => (object)getValue(s));

                    curLogParams = UpdateWriterDict(fileName, curLogParams);
                    if (curLogParams == null)
                        continue;

                    // Пропускаем логгирование консоли, если она отключена
                    if (!useConsole && (WriterType)curLogParams["Type"] == WriterType.Console)
                        continue;

                    writers.Add(new Writer(curLogParams));
                }
            }
            catch (Exception ex)
            {                
                throw new Exception("Ошибка при разборе конфигурации логгеров");
            }

        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Инициализация
        /// </summary>
        public static void Initialize(string fileName, bool useConsole)
        {
            string logsPath = string.Empty;
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            logsDir = string.Format("{0}\\{1}\\", baseDir,
                string.IsNullOrEmpty(logsPath) ? StringResources.GetLine("LogsPath") : logsPath);
            if (!Directory.Exists(logsDir))
                Directory.CreateDirectory(logsDir);

            writers = new List<Writer>();

            // Загружаем данные конфига логгеров
            string[] configData = null;
            string configPath = baseDir + "\\logger.cfg";
            if (File.Exists(configPath))
            {
                FileStream fs = new FileStream(configPath, FileMode.Open);
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(fs, Encoding.Default);
                    configData = sr.ReadToEnd()
                          .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.ReplaceRegex("##.+", string.Empty).Trim())
                          .Where(s => !string.IsNullOrEmpty(s))
                          .ToArray();
                }
                finally
                {
                    if (sr != null)
                        sr.Close();

                    fs.Close();
                }
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            InitWriters(configData, fileName, useConsole);

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;

            WriteToTrace("---------------------------------------------------------------------");
            WriteToTrace(string.Format(StringResources.GetLine("StartupMessage"), DateTime.Now));
        }

        /// <summary>
        /// Удаление объектов вывода логов
        /// </summary>
        public static void CloseLogFile()
        {
            WriteToTrace("Удаление логгеров.");
            // Закрываем все логгеры
            foreach (Writer writer in writers)
                writer.Close();
        }

        #endregion Основные функции

        #region Запись сообщений в лог

        /// <summary>
        /// Запись сообщения в лог
        /// </summary>
        public static void WriteToTrace(string message)
        {
            WriteToTrace(message, TraceMessageKind.Debug);
        }

        /// <summary>
        /// Запись сообщения в лог
        /// </summary>
        public static void WriteToTrace(string message, TraceMessageKind traceMessageKind)
        {
            WriteToTrace(message, traceMessageKind, Assembly.GetCallingAssembly().GetName().Name);
        }

        #endregion Запись сообщений в лог
    }
}
