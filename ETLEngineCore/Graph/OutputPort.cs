using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ELTEngineCore.Graph
{
    public class OutputPort: Port, IEnumerable<KeyValuePair<string, string>>
    {
        #region Поля

        protected Dictionary<string, string> data;

        #endregion Поля

        #region Свойства

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Запись на порт
        /// </summary>
        public void Write(Dictionary<string, string> dataRow)
        {
            data = dataRow;
        }

        /// <summary>
        /// Очистка данных
        /// </summary>
        public void Clear()
        {
            if (data != null)
                data.Clear();
        }

        #endregion Основные функции

        #region IEnumerator

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerator
    }
}
