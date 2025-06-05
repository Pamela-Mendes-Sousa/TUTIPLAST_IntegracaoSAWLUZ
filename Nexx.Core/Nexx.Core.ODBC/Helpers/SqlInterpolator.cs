using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nexx.Core.ODBC.Helpers
{
    public static class SqlInterpolator
    {
        public static string Format(string query, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query não pode ser vazia.", nameof(query));

            return Regex.Replace(query, @"\{(\d+)\}", match =>
            {
                int index = int.Parse(match.Groups[1].Value);

                if (index >= args.Length)
                    throw new IndexOutOfRangeException($"Parâmetro de índice {{ {index} }} não fornecido.");

                return FormatValue(args[index]);
            });
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                null => "NULL",
                string s => $"'{EscapeString(s)}'",
                DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
                bool b => b ? "TRUE" : "FALSE",
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => $"'{EscapeString(value.ToString())}'"
            };
        }

        private static string EscapeString(string input)
        {
            return input.Replace("'", "''");
        }
    }
}
