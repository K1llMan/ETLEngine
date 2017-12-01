using System.Collections.Generic;

namespace ETLEngineCore.Graph
{
    /// <summary>
    /// Входящий порт
    /// </summary>
    public class InputPort: Port
    {
        #region Поля

        protected OutputPort port;
        protected IEnumerator<Record> enumerator;

        private bool isFirstRead = true;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Получение метаданных для записей
        /// </summary>
        public MetaData MetaData
        {
            get { return port.MetaData; }
        }

        #endregion Свойства

        #region Основные функции

        public InputPort(OutputPort outPort)
        {
            port = outPort;
        }

        /// <summary>
        /// Чтение следующей записи
        /// </summary>
        public Record Read()
        {
            // При первом чтении формируем итератор
            if (isFirstRead)
            {
                enumerator = port.GetEnumerator();
                isFirstRead = false;
            }

            enumerator.MoveNext();
            return enumerator.Current;
        }

        /// <summary>
        /// Сброс положения чтения
        /// </summary>
        public void Reset()
        {
            isFirstRead = true;
        }

        /*
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            bool first = true;

            while (enumerator.MoveNext())
            {
                Record record = enumerator.Current;

                if (first)
                {
                    str.Append(string.Join("|", record.GetMapping()) + "\r\n");
                    first = false;
                }
                str.Append(record + "\r\n");
            }

            Reset();

            return str.ToString();
        }*/

        #endregion Основные функции
    }
}
