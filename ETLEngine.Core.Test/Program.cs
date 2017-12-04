using System;
using System.Collections.Generic;
using System.Data;

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

            MetaData meta = new MetaData(new List<MetaRecord> {
                new MetaRecord { From = string.Empty, To = "name", Type = typeof(string) },
                new MetaRecord { From = string.Empty, To = "code", Type = typeof(string) }
            });

            Node node = new Node(meta) {
                ID = "Node111",
                Database = db
            };

            node.Init(new Dictionary<string, string>{
                { "dbTempTable", "true" }
            });
            /*
            RecordTable rt = new RecordTable
            {
                Database = db,
                DBTable = "test_table2"
            };*/
            /*
            rt.Query();
            //rt.Data.Rows[0]["Code"] = "9999";

            rt.AddRow(new object[] {
                "Code", 1111,
                "Name", "olololo"
            });


            rt.Update();
            /*
            rt.CreateDBTable("test_table2", );*/
            

            /*
            ETLEngine engine = new ETLEngine();
            engine.RunGraph("testGraph.xml");
            */
        }
    }
}
