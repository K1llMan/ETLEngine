using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ETLEngineCore.Graph;

using Microsoft.CSharp;

namespace ETLEngineCore.Components
{
    public class CodeReaderWriter : Node
    {
        #region Основные функции

        public override void Execute()
        {
            try
            {
                if (!parameters.ContainsKey("code"))
                    return;
                /*
                string code = parameters["code"];
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerResults results = provider.CompileAssemblyFromSource(
                    new CompilerParameters(Assembly.GetExecutingAssembly().GetReferencedAssemblies().Select(a => a.Name + ".dll").ToArray()), code);

                if (results.Errors.HasErrors)
                {
                    return;
                }

                Type graphElementType = results.CompiledAssembly.GetTypes().First(t => typeof(GraphElement).IsAssignableFrom(t));

                // Код не содержал типа, унаследованного от элемента графа
                if (graphElementType == null)
                    return;

                // Создаём исполняемый элемент графа
                GraphElement element = (GraphElement)Activator.CreateInstance(graphElementType);

                // Перенаправляем порты
                foreach (KeyValuePair<int, InputPort> port in InputPorts)
                {
                    element.InputPorts[port.Key] = port.Value;
                }

                foreach (KeyValuePair<int, OutputPort> port in OutputPorts)
                    element.OutputPorts[port.Key] = port.Value;

                element.Execute();
                */
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion Основные функции
    }
}
