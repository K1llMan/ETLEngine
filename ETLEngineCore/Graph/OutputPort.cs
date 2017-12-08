using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ELTEngineCore.Graph
{
    public class OutputPort: Port, IEnumerable<KeyValuePair<string, object>>
    {
        #region Поля

        protected Dictionary<string, object> data;

        #endregion Поля

        #region Свойства

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Запись на порт
        /// </summary>
        public void Write(Dictionary<string, object> dataRow)
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

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
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
