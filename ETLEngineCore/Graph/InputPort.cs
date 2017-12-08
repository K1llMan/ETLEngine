using System.Collections.Generic;

namespace ELTEngineCore.Graph
{
    /// <summary>
    /// Входящий порт
    /// </summary>
    public class InputPort: Port
    {
        #region Поля

        protected OutputPort port;
        protected IEnumerator<KeyValuePair<string, object>> enumerator;

        private bool isFirstRead = true;

        #endregion Поля

        #region Свойства

        #endregion Свойства

        #region Основные функции

        public InputPort(OutputPort outPort)
        {
            port = outPort;
        }

        /// <summary>
        /// Чтение следующей записи
        /// </summary>
        public KeyValuePair<string, object> Read()
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
