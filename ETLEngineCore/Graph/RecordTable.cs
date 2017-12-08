using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;

using ELTEngineCore.Database;

namespace ELTEngineCore.Graph
{
    public class RecordTable: IEnumerable<DataRow>
    {
        #region Поля

        // Данные из таблицы
        private DataTable table;

        // Имя таблицы в базе
        private string dbTableName;

        private Dictionary<string, Func<object>> getDefaultValue;

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
            private set => UpdateTableInfo(value);
        }

        /// <summary>
        /// База данных, с которой осуществляется работа
        /// </summary>
        public ELTDatabase Database { get; set; }

        /// <summary>
        /// Поля в таблице
        /// </summary>
        public string[] Fields
        {
            get => Data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
        }

        #endregion Свойства

        #region Вспомогательные функции

        /// <summary>
        /// Создание из строк таблицы объекты для базы
        /// </summary>
        private dynamic GetObjectFromDataRow(DataRow row)
        {
            var obj = new ExpandoObject();
            var objDict = (IDictionary<string, object>)obj;

            foreach (DataColumn column in row.Table.Columns)
                objDict.Add(column.ColumnName, row[column.ColumnName]);

            return obj;
        }

        /// <summary>
        /// Получение набора строк для выполнения операции в базе
        /// </summary>
        private dynamic[] GetRowsByState(DataRowState state)
        {
            return Data.Rows.Cast<DataRow>()
                .Where(r => r.RowState == state)
                .Select(r => GetObjectFromDataRow(r))
                .ToArray();
        }

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

                // Словарь функций для формирования значения по умолчанию у строк
                getDefaultValue = new Dictionary<string, Func<object>>();

                table = new DataTable();
                foreach (dynamic row in result)
                {
                    DataColumn dc = new DataColumn
                    {
                        ColumnName = row.column_name,
                        DataType = Database.FromDBType(row.data_type)
                    };

                    table.Columns.Add(dc);

                    // Функции для значений по умолчанию
                    object defaultValue = row.column_default;

                    Func<object> f;
                    if (row.column_name == "id")
                        f = () => Database.ExecuteScalar($"select {defaultValue}");
                    else
                        f = () => defaultValue;

                    getDefaultValue.Add(row.column_name, f);
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

        public void CreateTable(MetaData meta)
        {
            table = new DataTable();
            table.Columns.Clear();
            table.Columns.AddRange(meta.Select(m => new DataColumn
            {
                ColumnName = m.To,
                DataType = m.Type
            }).ToArray());
        }

        /// <summary>
        /// Создание таблицы
        /// </summary>
        public void CreateDBTable(string tableName)
        {
            if (Database == null || string.IsNullOrEmpty(tableName))
                return;

            tableName = tableName.ToLower();

            Database.Execute(string.Format(
                "create table if not exists {0} (" +
                " id serial primary key, {1})",
                tableName,
                string.Join(", ", table.Columns.Cast<DataColumn>().Select(
                    c => $"{c.ColumnName} {Database.ToDBType(c.DataType)}"
                ))
            ));

            DBTable = tableName;
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
                        row[field.Key] = field.Value == null ? DBNull.Value : field.Value;

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
            if (Database == null)
                return -1;

            try
            {
                // Удаление
                int deletedCount = Database.Execute($"delete from {DBTable} where id = @id",
                    GetRowsByState(DataRowState.Deleted));

                // Обновление
               int updatedCount = Database.Execute(string.Format("update {0} set {1} where id = @id",
                    DBTable,string.Join(", ", Data.Columns.Cast<DataColumn>().Select(c => $"{c.ColumnName} = @{c.ColumnName}"))),
                    GetRowsByState(DataRowState.Modified));

                // Вставка
                // Вставляем всё, кроме ID, он генерируется автоматически
                string[] columns = Data.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .ToArray();

                int insertedCount = Database.Execute(string.Format("insert into {0} ({1}) values ({2})",
                        DBTable, string.Join(", ", columns), string.Join(", ", columns.Select(c => "@" + c))),
                    GetRowsByState(DataRowState.Added));

                // Коммит
                Database.Commit();

                // Очистка таблицы перед следующей порцией данных
                table.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return -1;
        }

        #endregion Операции с базой

        public bool AddRow(object[] mapping)
        {
            try
            {
                DataRow row = Data.NewRow();

                foreach (DataColumn column in table.Columns)
                    row[column.ColumnName] = getDefaultValue[column.ColumnName]();

                for (int i = 0; i < mapping.Length; i += 2)
                {
                    string field = mapping[i].ToString();
                    if (table.Columns.Contains(field))
                        row[field] = mapping[i + 1];
                }

                Data.Rows.Add(row);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Очистка таблицы
        /// </summary>
        public void Clear()
        {
            table.Clear();
        }

        #endregion Основные функции

        #region IEnumerable

        public IEnumerator<DataRow> GetEnumerator()
        {
            return (IEnumerator<DataRow>)table.Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
