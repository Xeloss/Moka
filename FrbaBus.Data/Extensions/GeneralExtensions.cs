using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FrbaBus.Data.Extensions
{
    internal static class GeneralExtensions
    {
        /// <summary>
        /// Evalua si un objeto es null
        /// </summary>
        public static bool Exists(this object anObject)
        {
            return anObject != null;
        }

        /// <summary>
        /// Evalua si una colección tiene elementos
        /// </summary>
        public static bool IsEmpty<T>(this IEnumerable<T> aCollection)
        {
            return !aCollection.Any();
        }

        /// <summary>
        /// Evalua si una cadena es nula o esta vacia.
        /// Equivalente a usar String.IsNullOrEmpty()
        /// </summary>
        public static bool IsNullOrEmpty(this string aString)
        {
            return string.IsNullOrEmpty(aString);
        }

        /// <summary>
        /// Evalua la expresion regular contra la cadena y devuelve la primer ocurrencia
        /// de la misma. De lo contrario retorna null.
        /// </summary>
        public static string MatchFirst(this string aString, string RegExPattern)
        {
            return aString.MatchFirst(RegExPattern, RegexOptions.None);
        }

        /// <summary>
        /// Evalua la expresion regular usando las opciones indicadas
        /// contra la cadena y devuelve la primer ocurrencia
        /// de la misma. De lo contrario retorna null.
        /// </summary>
        public static string MatchFirst(this string aString, string RegExPattern, RegexOptions options)
        {
            var match = Regex.Match(aString, RegExPattern, options);

            if (match.Success)
                return match.Value;

            return null;
        }

        /// <summary>
        /// Evalua la expresion regular contra la cadena y devuelve todas las ocurrencias
        /// de la misma.
        /// </summary>
        public static IEnumerable<string> MatchAll(this string aString, string RegExPattern)
        {
            return aString.MatchAll(RegExPattern, RegexOptions.None);
        }

        /// <summary>
        /// Evalua la expresion regular usando las opciones indicadas
        /// contra la cadena y devuelve todas las ocurrencia
        /// de la misma.
        /// </summary>
        public static IEnumerable<string> MatchAll(this string aString, string RegExPattern, RegexOptions options)
        {
            var matches = Regex.Matches(aString, RegExPattern, options);
            var result = new List<string>();

            foreach (Match match in matches)
                result.Add(match.Value);

            return result;
        }

        public static string ToSQLDate(this DateTime aDateTime)
        {
            var a = aDateTime.ToString("yyyyMMdd HH:mm:ss.mmm");
            return a;
        }
    }
}
