using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ETLEngineCore.Graph
{
    public class OutputPort: Port, IEnumerable<Record>
    {
        #region Поля

        protected RecordList data;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Получение метаданных для записей
        /// </summary>
        public MetaData MetaData
        {
            get { return data == null ? null : data.MetaData; }
            set
            {
                if (data != null)
                    data.Clear();

                data = new RecordList(value);
            }
        }

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Запись на порт
        /// </summary>
        public void Write(List<object> dataRow)
        {
            // Если метаданные у порта отсутствуют, то пытаемся сформировать по данным
            if (MetaData == null)
            {
                int i = -1;
                MetaData = new MetaData(dataRow.Select(f => {
                    i++;
                    return new MetaRecord {
                        From = "field" + i, To = "field" + i, Type = typeof(string)
                    };
                }).ToList());
            }

            data.Add(dataRow);
        }

        /// <summary>
        /// Запись на порт
        /// </summary>
        public void Write(Record dataRow)
        {
            // Если метаданные у порта отсутствуют, то берем от поступающей записи
            if (MetaData == null)
                MetaData = dataRow.Parent.MetaData;

            data.Add(dataRow.Reformat(MetaData));
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

        public IEnumerator<Record> GetEnumerator()
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
