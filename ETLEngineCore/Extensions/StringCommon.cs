using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ETLEngineCore.Extensions
{
    /// <summary>
    /// Свой тип, чтобы не привязывать дополнительно класс регулярных выражений
    /// </summary>
    public enum RegExOptions
    {
        None = 0,
        IgnoreCase = 1,
        Multiline = 2,
        ExplicitCapture = 4,
        Singleline = 16,
        IgnorePatternWhitespace = 32,
        RightToLeft = 64,
        ECMAScript = 256,
        CultureInvariant = 512,
    }

    /// <summary>
    /// Предоставляет набор методов расширения для работы со строками.
    /// </summary>
    public static class StringCommon
    {
        #region Вычисление выражения из строки

        #region Вспомогательные функции
        /// <summary>
        /// Возвращает приоритет операций
        /// </summary>
        private static int GetOperPriority(string oper)
        {
            switch (oper)
            {
                case "^":
                    return 2;
                case "*":
                case "/":
                    return 1;
                case "+":
                case "-":
                    return 0;
            }

            return -1;
        }

        private static string regexNum = @"-?[\d]+(,[\d]+)?";
        private static string regexBracket = @"\(|\)";
        private static string regexOpers = @"\+|\-|\*|/|\^";

        /// <summary>
        /// Преобразование выражения в обратную польскую запись с разбиением на элементы
        /// </summary>
        private static string[] GetInvertPolishRecord(string expr)
        {
            // Если в строке есть буквы, значит ее невозможно вычислить
            if (expr.RemoveNumbers().IsMatch(@"[\w]+"))
                return new string[] { };

            string brakeExpr = string.Format(@"{0}|{1}|{2}", regexNum, regexBracket, regexOpers);
            string[] tokens = expr.GetMatches(brakeExpr);

            if (tokens.Length == 0)
                return new string[] { };

            Stack<string> stack = new Stack<string>();
            string outStr = string.Empty;
            foreach (string token in tokens)
            {
                // Число добавляем в выходную строку
                if (token.IsMatch(regexNum))
                {
                    outStr += " " + token;
                    continue;
                }

                // Операторы добавляем в стек
                if (token.IsMatch(regexOpers))
                {
                    // Пока приоритет операции на вершине стека больше, либо равен текущей операции
                    while (stack.Count != 0 && GetOperPriority(token) <= GetOperPriority(stack.Peek()))
                        outStr += " " + stack.Pop();
                    stack.Push(token);
                    continue;
                }

                // Скобки
                if (token.IsMatch(regexBracket))
                    switch (token)
                    {
                        case "(":
                            stack.Push(token);
                            break;
                        case ")":
                            // Выталкиваем элементы до открывающейся скобки
                            while (stack.Count != 0 && stack.Peek() != "(")
                                outStr += " " + stack.Pop();
                            // Ошибка в скобочной структуре
                            if (stack.Count == 0)
                                throw new Exception("Ошибка в скобочной структуре.");
                            // Выталкиваем открывающуюся скобку
                            stack.Pop();
                            break;
                    }
            }

            // Выталкиваем все элементы стека
            while (stack.Count != 0)
                outStr += " " + stack.Pop();

            return string.IsNullOrEmpty(outStr) 
                ? new string[] { } 
                : outStr.Trim().Split(' ');
        }

        #endregion Вспомогательные функции

        /// <summary>
        /// Вычисление математического выражения, заданного строкой
        /// </summary>
        public static string Eval(this string expr)
        {
            string[] tokens = GetInvertPolishRecord(expr);
            if (tokens.Length == 0)
                return expr;

            Stack<decimal> stack = new Stack<decimal>();
            foreach (string token in tokens)
            {
                if (token.IsMatch(regexNum))
                    stack.Push(Convert.ToDecimal(token));
                else
                {
                    decimal a = stack.Pop();
                    decimal b = stack.Pop();

                    switch (token)
                    {
                        case "+":
                            stack.Push(a + b);
                            break;
                        case "-":
                            stack.Push(b - a);
                            break;
                        case "*":
                            stack.Push(a * b);
                            break;
                        case "/":
                            stack.Push(b / a);
                            break;
                        case "^":
                            a = Convert.ToDecimal(Math.Pow(Convert.ToDouble(b), Convert.ToDouble(a)));
                            stack.Push(a);
                            break;
                    }
                }
            }

            // На вершине стека должно оставаться только вычисленное значение, иначе ошибка в выражении
            return stack.Count == 1 ? stack.Pop().ToString() : expr;
        }

        /// <summary>
        /// Вычисление возможных математических выражения в строке, содержащей буквы
        /// </summary>
        public static string EvalEx(this string expr)
        {
            string prevExpr;

            // Пытаемся последовательно вычислить все скобочки и свернуть выражение
            do
            {
                prevExpr = expr;
                string[] brackets = expr.GetMatches(@"\([^\)\(]+\)");

                expr = brackets.Aggregate(expr, (current, bracket) => current.Replace(bracket, bracket.Eval()));
            } while (expr != prevExpr);

            // Пытаемся вычислить выражение полностью
            return expr.Eval();
        }

        #endregion Вычисление выражения из строки

        #region Работа с регулярными выражениями

        /// <summary>
        /// Замена в строке через регулярное выражение
        /// </summary>
        /// <param name="regExpr">Регулярное выражение</param>
        /// <param name="str">Строка</param>
        /// <param name="replStr">Строка-замена</param>
        /// <param name="options">Опции регулярного выражения</param>
        /// <returns></returns>
        public static string ReplaceRegex(this string str, string regExpr, string replStr, RegExOptions options)
        {
            return str == null
                ? string.Empty
                : Regex.Replace(str, regExpr, replStr, (RegexOptions)options);
        }

        /// <summary>
        /// Замена в строке через регулярное выражение
        /// </summary>
        /// <param name="regExpr">Регулярное выражение</param>
        /// <param name="str">Строка</param>
        /// <param name="replStr">Строка-замена</param>
        /// <returns></returns>
        public static string ReplaceRegex(this string str, string regExpr, string replStr)
        {
            return ReplaceRegex(str, regExpr, replStr, RegExOptions.None);
        }

        /// <summary>
        /// Удаляет буквы и знаки препинания в строке
        /// </summary>
        /// <param name="str">Строка</param>
        public static string RemoveLetters(this string str)
        {
            return ReplaceRegex(str, @"[^\d]", "");
        }

        /// <summary>
        /// Удаляет все вхождения указанной подстроки в строке
        /// </summary>
        /// <param name="str">Строка</param>
        /// <param name="subStr">Подстрока</param>
        public static string RemoveSubStr(this string str, string subStr)
        {
            return ReplaceRegex(str, subStr, "");
        }

        /// <summary>
        /// Заменяет все вхождения указанной подстроки в строке
        /// </summary>
        /// <param name="str">Строка</param>
        /// <param name="subStr">Подстрока</param>
        /// <param name="newSubStr">Новая подстрока</param>
        public static string ReplaceSubStr(this string str, string subStr, string newSubStr)
        {
            return ReplaceRegex(str, subStr, newSubStr);
        }

        /// <summary>
        /// Удаляет цифры в начале и конце строки
        /// </summary>
        /// <param name="str">Строка</param>
        public static string TrimNumbers(this string str)
        {
            return ReplaceRegex(str, @"^[\d]*|[\d]*$", "");
        }

        /// <summary>
        /// Удаляет цифры в строке
        /// </summary>
        /// <param name="str">Строка</param>
        public static string RemoveNumbers(this string str)
        {
            return ReplaceRegex(str, @"[\d]", "");
        }

        /// <summary>
        /// Удаляет буквы и знаки препинания в начале и конце строки
        /// </summary>
        /// <param name="str">Строка</param>
        public static string TrimLetters(this string str)
        {
            string exp = @"-?[\d](.*)(?<![^\d])";
            if (Regex.IsMatch(str, exp))
                return Regex.Match(str, exp).Captures[0].Value;
            return string.Empty;
        }

        /// <summary>
        /// Удаляет лишние пробелы и переносы в строке
        /// </summary>
        /// <param name="str">Строка</param>
        public static string RemoveSpaces(this string str)
        {
            return ReplaceRegex(str, @"\s+", "");
        }

        /// <summary>
        /// Заменяет лишние пробелы и переносы в строке на указанную строку
        /// </summary>
        public static string ReplaceSpaces(this string str, string replacementStr)
        {
            return ReplaceRegex(str, @"\s+", replacementStr);
        }

        /// <summary>
        /// Проверяет соответствие регулярному выражению
        /// </summary>
        public static bool IsMatch(this string str, string pattern, RegExOptions options)
        {
            return Regex.IsMatch(str, pattern, (RegexOptions)options);
        }

        /// <summary>
        /// Проверяет соответствие регулярному выражению
        /// </summary>
        public static bool IsMatch(this string str, string pattern)
        {
            return IsMatch(str, pattern, RegExOptions.IgnoreCase);
        }

        /// <summary>
        /// Возвращает совпадения для регулярного выражения
        /// </summary>
        public static string [] GetMatches(this string str, string pattern, RegExOptions options)
        {
            if (str.IsMatch(pattern, options))
                return Regex.Matches(str, pattern, (RegexOptions)options).Cast<Match>().Select(m => m.Value).ToArray();

            return new string[] { };
        }

        /// <summary>
        /// Возвращает совпадения для регулярного выражения
        /// </summary>
        public static string[] GetMatches(this string str, string pattern)
        {
            return str.GetMatches(pattern, RegExOptions.IgnoreCase);
        }

        #endregion Работа с регулярными выражениями
    }
}
