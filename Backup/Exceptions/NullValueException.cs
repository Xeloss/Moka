using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using FrbaBus.Common.Extensions;

namespace FrbaBus.Data.Exceptions
{
    public class NullValueException : DataBaseException
    {
        public NullValueException(SqlException ex)
            : base(ex)
        {
            var fieldName = ex.Message.MatchFirst(@"(?<=\').+?(?=\',)");
            var tableName = ex.Message.MatchFirst(@"(?<=table\s\').+?(?=\';)");

            this.Field = fieldName;
            this.Table = tableName;
        }

        public string Field { get; set; }

        public string Table { get; set; }

        public override string Message
        {
            get
            {
                return string.Format("El campo {0} no se puede insertar nulo en la tabla {1}", this.Field, this.Table);
            }
        }
    }
}
