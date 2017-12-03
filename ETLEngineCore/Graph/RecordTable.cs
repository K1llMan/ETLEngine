using System;
using System.Data;
using System.Linq;

using ETLEngineCore.Database;

namespace ETLEngineCore.Graph
{
    public class RecordTable
    {
        #region Поля

        // Данные из таблицы
        private DataTable table;

        // Имя таблицы в базе
        private string dbTableName;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Данные
        /// </summary>
        public DataTable Data { get => table; }

        /// <summary>
        /// Имя таблицы в базе
        /// </summary>
        public string DBTable {
            get => dbTableName;
            set => UpdateTableInfo(value);
        }

        /// <summary>
        /// База данных, с которой осуществляется работа
        /// </summary>
        public ETLDatabase Database { get; set; }

        #endregion Свойства

        #region Вспомогательные функции

        /// <summary>
        /// Формирование структуры DataTable для хранения информации
        /// </summary>
        private void InitDataTable()
        {
            try
            {
                dynamic result = Database.Query(
                    "select column_name, data_type, column_default, is_nullable" +
                    " from information_schema.columns" +
                    $" where  table_name = '{DBTable}'");

                table = new DataTable();
                foreach (dynamic row in result)
                {
                    DataColumn dc = new DataColumn
                    {
                        ColumnName = row.column_name,
                        DataType = Database.FromDBType(row.data_type),
                        DefaultValue = row.column_default
                    };

                    table.Columns.Add(dc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Обновление данных о рабочей таблице
        /// </summary>
        private void UpdateTableInfo(string tableName)
        {
            if (Database == null)
                return;

            try
            {
                // Проверяем наличия таблицы в базе
                dynamic result = Database.Query(
                    "select exists (" +
                    " select 1" +
                    " from information_schema.tables" +
                    $" where table_name = '{tableName}')").First();

                // Если не существует, то создаём новую
                if (!result.exists)
                        return;

                dbTableName = tableName;

                InitDataTable();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        #endregion Вспомогательные функции

        #region Основные функции

        #region Операции с базой

        /// <summary>
        /// Создание таблицы
        /// </summary>
        public void CreateDBTable(string tableName, MetaData meta)
        {
            table = new DataTable();
            table.Columns.Clear();
            table.Columns.AddRange(meta.Select(m => new DataColumn {
                ColumnName = m.To,
                DataType = m.Type
            }).ToArray());

            Database.Execute(string.Format(
                "create table {0} ({1})",
                tableName,
                string.Join(", ", table.Columns.Cast<DataColumn>().Select(
                    c => $"{c.ColumnName} {Database.ToDBType(c.DataType)}"
                ))
            ));

            dbTableName = tableName;
        }

        /// <summary>
        /// Запрос данных
        /// </summary>
        public int Query()
        {
            if (Database == null)
                return -1;

            try
            {
                // Проверяем наличия таблицы в базе
                dynamic result = Database.Query(
                    $" select {string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}" +
                    $" from {DBTable}");

                foreach (dynamic dbRow in result)
                {
                    DataRow row = table.NewRow();
                    foreach (dynamic field in dbRow)
                        row[field.Key] = field.Value;

                    table.Rows.Add(row);
                }

                table.AcceptChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return table.Rows.Count;
        }

        /// <summary>
        /// Применение изменений к таблице
        /// </summary>
        /// <returns></returns>
        public int Update()
        {
            try
            {
                #warning необходимо преобразовать строки в объекты с заданными свойствами

                Database.Execute($"delete from {DBTable} where id = @id",
                    Data.Rows.Cast<DataRow>().Where(t => t.RowState == DataRowState.Deleted));

                Database.Execute(string.Format("update {0} set {1} where id = @id",
                    DBTable,string.Join(", ", Data.Columns.Cast<DataColumn>().Select(c => $"{c.ColumnName} = @{c.ColumnName}"))),
                    Data.Rows.Cast<DataRow>().Where(r => r.RowState == DataRowState.Modified));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return -1;
        }


        #endregion Операции с базой



        #endregion Основные функции
    }
}
