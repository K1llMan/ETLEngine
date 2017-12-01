using System.Collections.Generic;

namespace ETLEngineCore.Graph
{
    /// <summary>
    /// Узел
    /// </summary>
    public class Node: GraphElement
    {
        #region Свойства

        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void Init(Dictionary<string, string> inputParameters)
        {
            parameters = inputParameters;

            OutputPorts[0] = new OutputPort();
        }

        public override void Execute()
        {
        }

        #endregion Основные функции
    }
}
