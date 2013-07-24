using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using FrbaBus.Common.Extensions;

namespace FrbaBus.Data.Exceptions
{
    /// <summary>
    /// Representa excepciones ocurridas por claves duplicadas o
    /// Unique Constraints que no se cumplen.
    /// </summary>
    public class UniqueConstraintException : DataBaseException
    {
        public UniqueConstraintException(SqlException ex)
            : base(ex)
        {
            var value = ex.Message.MatchFirst(@"(?<=\s\().+?(?=\))");
            var matches = ex.Message.MatchAll(@"(?<=\s').+?(?=\')")
                                    .ToArray();

            if (matches.Length == 2)
            {
                this.ConstraintName = matches[0];
                this.Table = matches[1];
            }

            if (value.Exists())
                this.DuplicatedValue = value;
        }

        public string DuplicatedValue { get; set; }

        public string Table { get; set; }

        public string ConstraintName { get; set; }

        public override string Message
        {
            get
            {
                return string.Format("Violación de {0} en la tabla {1}. El valor duplicado es {2}", this.ConstraintName, this.Table, this.DuplicatedValue);
            }
        }
    }
}
