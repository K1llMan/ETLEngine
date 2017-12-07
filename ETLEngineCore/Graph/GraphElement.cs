using System;
using System.Collections.Generic;

namespace ELTEngineCore.Graph
{
    /// <summary>
    /// Родительский класс для всех элементов графа
    /// </summary>
    public abstract class GraphElement: IDisposable
    {
        #region Поля

        protected Dictionary<string, string> parameters;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Идентификатор
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Входящие порты
        /// </summary>
        public Dictionary<int, InputPort> InputPorts { get; private set; }

        /// <summary>
        /// Исходящие порты
        /// </summary>
        public Dictionary<int, OutputPort> OutputPorts { get; private set; }

        #endregion Свойства

        #region Основные функции

        protected GraphElement()
        {
            InputPorts = new Dictionary<int, InputPort>();
            OutputPorts = new Dictionary<int, OutputPort>();
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public virtual void Init(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Выполнение
        /// </summary>
        public virtual void Execute()
        {
        }

        /// <summary>
        /// Очистка
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion Основные функции
    }
}
