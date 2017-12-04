using System;
using System.Collections.Generic;

using ETLEngineCore.Database;

namespace ETLEngineCore.Graph
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
        public ETLDatabase Database
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

        #endregion Свойства

        #region Основные функции

        public Node(MetaData meta)
        {
            table = new RecordTable();
            table.CreateTable(meta);

            MetaData = meta;
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void Init(Dictionary<string, string> inputParameters)
        {
            parameters = inputParameters;

            if (Convert.ToBoolean(inputParameters["dbTempTable"]))
                table.CreateDBTable($"temp{ID}");

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
