using System.Collections.Generic;
using System.Linq;

using ELTEngineCore.Graph;

namespace ELTEngineCore.Staging
{
    /// <summary>
    /// Этап
    /// </summary>
    public class Stage
    {
        #region Поля

        private List<Node> nodes;
        private List<Edge> edges;

        private List<GraphElement> queue;

        #endregion Поля

        #region Свойства

        public string Description { get; private set; }

        #endregion Свойства

        #region Вспомогательные функции

        /// <summary>
        /// Добавление элементов в очередь исполнения
        /// </summary>
        private void InsertInQueue(GraphElement element)
        {
            if (queue.Contains(element))
                return;

            if (element is Node)
            {
                // Выбираем входящие рёбра
                foreach (Edge edge in edges.Where(e => e.To == element.ID))
                    InsertInQueue(edge);

                // Вставляем сам элемент
                if (!queue.Contains(element))
                    queue.Add(element);

                // Выбираем исходящие рёбра
                foreach (Edge edge in edges.Where(e => e.From == element.ID))
                    InsertInQueue(edge);
            }

            if (element is Edge)
            {
                // Выбираем исходный узел
                InsertInQueue(nodes.First(n => n.ID == ((Edge)element).From));

                // Вставляем сам элемент
                if (!queue.Contains(element))
                    queue.Add(element);

                // Выбираем исходный узел
                InsertInQueue(nodes.First(n => n.ID == ((Edge)element).To));
            }
        }

        /// <summary>
        /// Формирование очереди исполнения
        /// </summary>
        private void FormGraphQueue()
        {
            queue = new List<GraphElement>();

            foreach (Node node in nodes)
                if (!queue.Contains(node))
                    InsertInQueue(node);
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public Stage(string desc, List<Node> graphNodes, List<Edge> graphEdges)
        {
            Description = desc;
            nodes = graphNodes;
            edges = graphEdges;

            FormGraphQueue();
        }

        /// <summary>
        /// Выполнение этапа
        /// </summary>
        public void Execute()
        {
            Clear();

            foreach (GraphElement element in queue)
            {
                element.Execute();
            }
        }

        /// <summary>
        /// Очистка данных перед выполнением
        /// </summary>
        public void Clear()
        {
            foreach (GraphElement element in queue)
            {
                foreach (InputPort port in element.InputPorts.Values)
                    port.Reset();

                foreach (OutputPort port in element.OutputPorts.Values)
                    port.Clear();
            }
        }

        #endregion Основные функции
    }
}
