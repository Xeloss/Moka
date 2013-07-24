using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using FrbaBus.Common.Extensions;

namespace FrbaBus.Data.Exceptions
{
    public class CheckConstraintException : DataBaseException
    {
        public CheckConstraintException(SqlException ex)
            : base(ex)
        {
            var field = ex.Message.MatchFirst(@"(?<=\').+?(?=\')");
            var matches = ex.Message.MatchAll("(?<=\\s\\\").+?(?=\\\"(\\,|\\.))")
                                    .ToArray();
            if (matches.Length == 3)
            {
                this.ConstraintName = matches[0];
                this.Table = matches[2];
            }

            if (field.Exists())
                this.Field = field;
        }

        public string Field { get; set; }

        public string Table { get; set; }

        public string ConstraintName { get; set; }

        public override string Message
        {
            get
            {
                return string.Format("Violación de la Check Constraint {0} en la tabla {1}. El campo en conflicto es {2}", this.ConstraintName, this.Table, this.Field);
            }
        }
    }
}
