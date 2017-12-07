using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ELTEngineCore.Graph;

namespace ELTEngineCore.Components
{
    public class CodeReaderWriter : Node
    {
        #region Основные функции

        public CodeReaderWriter(MetaData meta) : base(meta)
        {
            
        }

        public override void Execute()
        {
            try
            {
                if (!parameters.ContainsKey("code"))
                    return;

                string code = parameters["code"];
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
                string assemblyName = Path.GetRandomFileName();
                MetadataReference[] references = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                    .Select(a => MetadataReference.CreateFromFile(Assembly.Load(a).Location))
                    .Union(new MetadataReference[] {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
                    })
                    .ToArray();

                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);

                    if (!result.Success)
                    {
                        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);
                        /*
                        foreach (Diagnostic diagnostic in failures)
                        {
                            Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        }*/

                        return;
                    }

                    ms.Seek(0, SeekOrigin.Begin);

                    Type graphElementType = Assembly.Load(ms.ToArray()).GetTypes().First(t => typeof(GraphElement).IsAssignableFrom(t));

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
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion Основные функции
    }
}
