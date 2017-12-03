namespace ETLEngineCore.Graph
{
    /// <summary>
    /// Ребро
    /// </summary>
    public class Edge: GraphElement
    {
        #region Свойства

        /// <summary>
        /// Откуда
        /// </summary>
        public string From { get; private set; }

        /// <summary>
        /// Куда
        /// </summary>
        public string To { get; private set; }

        #endregion Свойства

        #region Основные функции

        public Edge(string from, string to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Трансформация данных
        /// </summary>
        public override void Execute()
        {
            Record r = InputPorts[0].Read();
            while (r != null)
            {
                OutputPorts[0].Write(r);
                r = InputPorts[0].Read();
            }
        }

        #endregion Основные функции
    }
}
