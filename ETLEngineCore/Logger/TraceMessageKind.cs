namespace ETLEngineCore
{

    /// <summary>
    /// Тип сообщения, записываемого в лог
    /// </summary>
    public enum TraceMessageKind
    {
        /// <summary>
        /// Критическая ошибка
        /// </summary>
        CriticalError = 0,

        /// <summary>
        /// Ошибка
        /// </summary>
        Error = 1,

        /// <summary>
        /// Некритическая ошибка
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Информационное сообщение
        /// </summary>
        Information = 3,

        /// <summary>
        /// Тип сообщения не отображается
        /// </summary>
        Debug = 4,

        /// <summary>
        /// Полный уровень отображения
        /// </summary>
        All = 5
    }

}
