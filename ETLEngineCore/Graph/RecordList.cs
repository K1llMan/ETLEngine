using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ETLEngineCore.Graph
{
    public class RecordList: IList<Record>
    {
        #region Поля

        // Набор строк
        private List<Record> data;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Получение метаданных для записей
        /// </summary>
        public MetaData MetaData { get; private set; }

        /// <summary>
        /// Получение названий полей для записей
        /// </summary>
        public List<string> Fields { get; private set; }

        #endregion Свойства

        #region Основные функции

        public RecordList(MetaData metaData)
        {
            data = new List<Record>();
            MetaData = metaData;
            Fields = metaData.Select(f => f.To).ToList();
        }

        /// <summary>
        /// Создание новой строки в списке
        /// </summary>
        public Record Add(List<object> dataRow)
        {
            if (dataRow == null)
                return null;

            // Если данных больше или меньше, то они будут обрезаны или заменены пустотой
            int i = 0;
            List<object> outRowData = MetaData.Select(m => {
                if (i > dataRow.Count - 1)
                    return null;
                return dataRow[i++];

                #warning Здесь должно быть преобразование типов
                /*
                try
                {
                    return dataRow[i].GetType() == m.Type
                        ? dataRow[i]
                        : Convert.ChangeType(dataRow[i], m.Type);
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    i++;
                }*/
            })
            .ToList();
            
            Record row = new Record(this, outRowData);
            data.Add(row);
            return row;
        }

        #endregion Основные функции

        #region IList

        public IEnumerator<Record> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)data).GetEnumerator();
        }

        public void Add(Record item)
        {
            data.Add(item);
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(Record item)
        {
            return data.Contains(item);
        }

        public void CopyTo(Record[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public bool Remove(Record item)
        {
            return data.Remove(item);
        }

        public int Count
        {
            get { return data.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(Record item)
        {
            return data.IndexOf(item);
        }

        public void Insert(int index, Record item)
        {
            data.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            data.RemoveAt(index);
        }

        public Record this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        #endregion IList
    }
}
