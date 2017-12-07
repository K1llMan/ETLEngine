using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ELTEngineCore.Extensions;

namespace ELTEngineCore
{
    /// <summary>
    /// Типы логгеров
    /// </summary>
    internal enum WriterType
    {
        /// <summary>
        /// Файловый
        /// </summary>
        File,

        /// <summary>
        /// Консольный
        /// </summary>
        Console
    }


    /// <summary>
    /// Класс с настройками записи
    /// </summary>
    internal class Writer
    {
        private StreamWriter sw;

        private WriterType type;
        private string layout = "\r\n{0}: {1} {2}: {3}";
        private string timeFormat = string.Empty;
        private int categoryCount = 2;
        private TraceMessageKind[] levels = { TraceMessageKind.All };

        // Повторяющие сообщения
        private string lastMessage;
        private int messageCount = 1;
        private TraceMessageKind lastMessageKind;

        #region Вспомогательные функции

        /// <summary>
        /// Устанавливает цвет консоли
        /// </summary>
        private void SetForegroundColor(TraceMessageKind traceMessageKind)
        {
            switch (traceMessageKind)
            {
                case TraceMessageKind.CriticalError:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;

                case TraceMessageKind.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case TraceMessageKind.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case TraceMessageKind.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }
        }

        /// <summary>
        /// Обновляет строку разметки параметрами
        /// </summary>
        private string UpdateLayout(string logLayout)
        {
            Dictionary<string,string> replaces = new Dictionary<string, string> {
                { "{newline}", "\r\n" },
                { "{category.*?}", "{0}" },
                { "{time.*?}", "{1}" },
                { "{kind}", "{2}" },
                { "{msg}", "{3}" }
            };

            foreach (string key in replaces.Keys)
            {
                // Для категорий может выводиться количество слов
                if (key.IsMatch("category"))
                {
                    string categoryStr = logLayout.GetMatches(key).FirstOrDefault();
                    if (!string.IsNullOrEmpty(categoryStr))
                    {
                        string catCount = categoryStr.GetMatches(@"(?<=\:)[\d]+").FirstOrDefault();
                        categoryCount = string.IsNullOrEmpty(catCount) ? categoryCount : Convert.ToInt32(catCount);
                    }
                }

                // Для времени можно задать маску
                if (key.IsMatch("time"))
                {
                    string timeStr = logLayout.GetMatches(key).FirstOrDefault();
                    if (!string.IsNullOrEmpty(timeStr))
                    {
                        string timeFormatStr = timeStr.GetMatches(@"(?<=\:).*(?=\})").FirstOrDefault();
                        timeFormat = timeFormatStr ?? timeFormat;
                    }
                }

                logLayout = logLayout.ReplaceRegex(key, replaces[key]);
            }

            return logLayout;
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public Writer(Dictionary<string, object> parameters)
        {
            type = (WriterType)parameters["Type"];
            switch (type)
            {
                case WriterType.File:
                    string dir = Path.GetDirectoryName(parameters["File"].ToString());
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    sw = new StreamWriter(parameters["File"].ToString(), true, Encoding.GetEncoding(1251));
                    break;
                case WriterType.Console:
                    sw = new StreamWriter(Console.OpenStandardOutput(), Encoding.GetEncoding(866));
                    Console.SetOut(sw);
                    break;
            }
            
            sw.AutoFlush = true;

            foreach (string key in parameters.Keys)
                switch (key)
                {
                    case "Levels":
                        levels = (TraceMessageKind[]) parameters[key];
                        break;
                    case "Pattern":
                        layout = UpdateLayout(parameters[key].ToString());
                        break;
                }
        }

        /// <summary>
        /// Запись в логгер
        /// </summary>
        public void Write(string message, TraceMessageKind traceMessageKind, string category)
        {
            // Если уровень логирование недопустимый, то пропускаем запись
            if (!levels.Contains(TraceMessageKind.All) && !levels.Contains(traceMessageKind))
                return;

            string[] words = category.Split('.');
            category = string.Join(".", words.Skip(words.Length - Math.Min(words.Length, categoryCount)));

            string formattedMessage = string.Format(layout, category, DateTime.Now.ToString(timeFormat), StringResources.GetLine(traceMessageKind), message);
            // Если сообщения совпадают, то инкрементируем счетчик
            if (formattedMessage == lastMessage && traceMessageKind == lastMessageKind)
            {
                messageCount++;
                return;
            }

            // Если счетчик не нулевой, то дописываем к предыдущему сообщению количество и выводим новое сообщение
            if (messageCount > 1)
            {
                if (type == WriterType.Console)
                    SetForegroundColor(lastMessageKind);

                sw.Write(string.Format(" ({0})", messageCount));
                Console.ResetColor();

                messageCount = 1;
            }

            lastMessage = formattedMessage;
            lastMessageKind = traceMessageKind;

            if (type == WriterType.Console)
                SetForegroundColor(traceMessageKind);

            sw.Write(formattedMessage);

            Console.ResetColor();
        }

        public void Close()
        {
            if (sw != null)
                sw.Close();
        }

        #endregion Основные функции
    }
}
