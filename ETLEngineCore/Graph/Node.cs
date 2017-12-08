using System;
using System.Collections.Generic;

using ELTEngineCore.Database;

namespace ELTEngineCore.Graph
{
    /// <summary>
    /// Узел
    /// </summary>
    public class Node: GraphElement
    {
        #region Поля

        // Данные
        protected RecordTable table;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// База данных, с которой осуществляется работа
        /// </summary>
        public ELTDatabase Database
        {
            get => table.Database;
            set => table.Database = value;
        }

        /// <summary>
        /// Имя таблицы в базе
        /// </summary>
        public string DBTable
        {
            get => table.DBTable;
        }

        /// <summary>
        /// Получение метаданных для записей
        /// </summary>
        public MetaData MetaData { get; }

        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Является ли таблица временной
        /// </summary>
        public bool IsTemporary { get; private set; }

        #endregion Свойства

        #region Основные функции

        public Node(MetaData meta)
        {
            table = new RecordTable();
            MetaData = meta;

            table.CreateTable(meta);
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void Init(Dictionary<string, string> inputParameters)
        {
            parameters = inputParameters;

            IsTemporary = inputParameters.ContainsKey("dbTempTable") && Convert.ToBoolean(inputParameters["dbTempTable"]);

            // Создаём временную таблицу или запрашиваем структуру существующей таблицы
            string tableName = IsTemporary ? $"temp{ID}" : string.Empty;
            if (inputParameters.ContainsKey("dbTable"))
                tableName = inputParameters["dbTable"];

            table.CreateDBTable(tableName);
            table.Query();

            OutputPorts[0] = new OutputPort();
        }

        /// <summary>
        /// Выполнение
        /// </summary>
        public override void Execute()
        {
        }

        /// <summary>
        /// Фиксация изменений
        /// </summary>
        public virtual void Commit()
        {
            table.Update();
        }

        /// <summary>
        /// Удаление
        /// </summary>
        public override void Dispose()
        {
            table.Clear();
        }

        #endregion Основные функции
    }
}
