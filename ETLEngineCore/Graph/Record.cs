using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ETLEngineCore.Graph
{
    /// <summary>
    /// Запись
    /// </summary>
    public class Record: IEnumerable<object>
    {
        #region Поля

        private List<object> data;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Получение метаданных для записей
        /// </summary>
        public object this[string field]
        {
            get
            {
                int i = Parent.Fields.IndexOf(field);
                return i == -1 ? null : data[i];
            }
        }

        /// <summary>
        /// Список, содержащий запись
        /// </summary>
        public RecordList Parent { get; private set; }

        #endregion Свойства

        #region Основные функции

        internal Record(RecordList list, List<object> inputData)
        {
            Parent = list;
            data = inputData;
        }

        /// <summary>
        /// Преобразование данных записи по правилам метаданных
        /// </summary>
        public List<object> Reformat(MetaData metaData)
        {
            // Без метаданных возвращаем текущую запись
            if (metaData == null || metaData.Equals(Parent.MetaData))
                return data;

            List<object> values = metaData.Select(r => {
                    return this[r.From];

                    #warning Здесь должно быть преобразование типов
                    /*
                    object value;
                    try
                    {
                        // Преобразование типов вызывает тормоза
                        value = this[r.From].GetType() == r.Type
                            ? this[r.From]
                            : Convert.ChangeType(this[r.From], r.Type);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    return value;*/
                })
                .ToList();

            return values;
        }

        /// <summary>
        /// Возвращает список полей в записи
        /// </summary>
        public string[] GetMapping()
        {
            return Parent.MetaData.Select(r => r.To).ToArray();
        }

        public override string ToString()
        {
            return string.Join("|", data);
        }

        #endregion Основные функции

        #region IEnumerable

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)data).GetEnumerator();
        }

        #endregion IEnumerable
    }
}
