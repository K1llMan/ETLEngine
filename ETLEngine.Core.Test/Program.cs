using System;
using System.Collections.Generic;
using System.Data;

using ELTEngineCore.Database;
using ELTEngineCore.Graph;

namespace ELTEngineCore.Test
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
                { "dbTempTable", "false" },
                { "dbTable", "test_table2" }
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
            ELTEngine engine = new ELTEngine();
            engine.RunGraph("testGraph.xml");
            */
        }
    }
}
