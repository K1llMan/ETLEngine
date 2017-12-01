using System;
using ETLEngineCore;

namespace ETLEngineCore.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ETLEngine engine = new ETLEngine();
            engine.RunGraph("testGraph.xml");
        }
    }
}
