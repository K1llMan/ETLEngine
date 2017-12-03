using System;
using System.Collections.Generic;

using ETLEngineCore.Database;
using ETLEngineCore.Graph;

namespace ETLEngineCore.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ETLDatabase db = new ETLDatabase();
            db.Connect("postgresql://localhost:5432/postgres;User ID=postgres;Password=dv;");
            db.BeginTransaction();

            RecordTable rt = new RecordTable
            {
                Database = db,
                DBTable = "test_table"
            };

            rt.Query();
            rt.Data.Rows[0]["Code"] = "9999";

            rt.Update();
            /*
            rt.CreateDBTable("test_table1", new MetaData(new List<MetaRecord> {
                new MetaRecord { From = string.Empty, To = "id", Type = typeof(int) },
                new MetaRecord { From = string.Empty, To = "name", Type = typeof(string) },
                new MetaRecord { From = string.Empty, To = "code", Type = typeof(string) }
            }));
            */

            /*
            ETLEngine engine = new ETLEngine();
            engine.RunGraph("testGraph.xml");
            */
        }
    }
}
