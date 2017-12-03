using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;

using Dapper;
using ETLEngineCore.Extensions;
using Npgsql;

namespace ETLEngineCore.Database
{
    public class ETLDatabase
    {
        #region Поля

        private IDbConnection connection;
        private IDbTransaction transaction;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Тип базы
        /// </summary>
        public DBType DatabaseType { get; private set; }

        #endregion Свойства

        #region Вспомогательные функции

        private string GetConnectionString(string parameters)
        {
            string[] properties = parameters.Split(';', StringSplitOptions.RemoveEmptyEntries);

            Uri uri = new Uri(properties[0]);

            string typeStr = uri.Scheme;

            if (!Enum.TryParse(typeof(DBType), typeStr, true, out var type))
                return string.Empty;

            DatabaseType = (DBType)type;

            Dictionary<string, string> paramDict = new Dictionary<string, string> {
                { "Host", uri.Host },
                { "Port", uri.Port.ToString() }
            };

            List<string> segments = uri.Segments.ToList();
            segments.Remove("/");

            if (segments.Count > 0 && !paramDict.ContainsKey("Database"))
                paramDict.Add("Database", segments.Last());

            foreach (string property in properties.Skip(1))
            {
                string key = property.GetMatches(@".+(?=\=)").First().Trim();
                string value = property.GetMatches(@"(?<=\=).+").First().Trim();

                if (string.IsNullOrEmpty(key) || paramDict.ContainsKey(key) )
                    continue;

                paramDict.Add(key, value);
            }

            return string.Join("; ", paramDict.Select(p => $"{p.Key}={p.Value}"));
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Соединения с базой (формат [dbType]://[serverName[:portNumber][/instanceName]][;property=value[;property=value]])
        /// postgresql://localhost:5432/db;User ID=sysdba;Password=masterkey;
        /// </summary>
        public void Connect(string connStr)
        {
            connection?.Close();
            connection?.Dispose();

            string connectionString = GetConnectionString(connStr);

            if (string.IsNullOrEmpty(connectionString))
                return;

            switch (DatabaseType)
            {
                case DBType.PostgreSql:
                    connection = new NpgsqlConnection(connectionString);
                    break;

                case DBType.SqlServer:
                    connection = new SqlConnection();
                    break;

                case DBType.Oracle:
                default:
                    return;
            }


            connection.Open();
        }

        /// <summary>
        /// Отключение
        /// </summary>
        public void Disconnect()
        {
            connection.Close();
        }

        /// <summary>
        /// Создание транзакции
        /// </summary>
        public void BeginTransaction()
        {
            transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Параметризированный запрос
        /// </summary>
        public int Execute(string query, object param = null)
        {
            return connection.Execute(query, param);
        }

        /// <summary>
        /// Запрос, возвращающий результат
        /// </summary>
        public IEnumerable<dynamic> Query(string query, object param = null)
        {
            return connection.Query<dynamic>(query, param);
        }

        /// <summary>
        /// Типы базы в типы .Net
        /// </summary>
        public Type FromDBType(string type)
        {
            if (type.IsMatch("integer|real|double|bigint|smallint|numeric"))
                return typeof(decimal);

            if (type.IsMatch("character|char|text"))
                return typeof(string);

            if (type.IsMatch("date|time|timestamp"))
                return typeof(DateTime);

            return typeof(object);
        }

        /// <summary>
        /// Типы базы в типы .Net
        /// </summary>
        public string ToDBType(Type type, int length = 0)
        {
            switch (type.Name)
            {
                case "Int16":
                    return "smallint";
                case "Int32":
                    return "int";
                case "Int64":
                    return "bigint";
                case "Decimal":
                    return "decimal";
                case "DateTime":
                    return "date";
                case "Single":
                    return "real";
                case "Double":
                    return "float";
                case "String":
                    return "varchar";
            }

            return "varchar";
        }

        /// <summary>
        /// Фиксация транзакции
        /// </summary>
        public void Commit()
        {
            transaction.Commit();
        }

        /// <summary>
        /// Откат транзакции
        /// </summary>
        public void Rollback()
        {
            transaction.Rollback();
        }

        #endregion Основные функции
    }
}
