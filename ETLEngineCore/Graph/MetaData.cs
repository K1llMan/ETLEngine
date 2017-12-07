using System;
using System.Collections;
using System.Collections.Generic;

namespace ELTEngineCore.Graph
{
    /// <summary>
    /// Запись правила метаданных
    /// </summary>
    public struct MetaRecord
    {
        public string From;
        public string To;
        public Type Type;
    }

    /// <summary>
    /// Набор метаданных для преобразования записи
    /// </summary>
    public class MetaData: IEnumerable<MetaRecord>
    {
        #region Поля

        private List<MetaRecord> rules;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Количество полей
        /// </summary>
        public int Count
        {
            get { return rules.Count; }
        }

        public MetaRecord this[int index]
        {
            get { return rules[index]; }
        }

        #endregion Свойства

        #region Основные функции

        public MetaData(List<MetaRecord> workRules)
        {            
            rules = workRules;
        }

        #endregion Основные функции

        #region IEnumerable

        public IEnumerator<MetaRecord> GetEnumerator()
        {
            return rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)rules).GetEnumerator();
        }

        #endregion IEnumerable
    }
}
