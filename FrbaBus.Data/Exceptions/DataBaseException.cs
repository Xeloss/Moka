using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;

namespace FrbaBus.Data.Exceptions
{
    public class DataBaseException : DbException
    {
        public DataBaseException(SqlException ex)
            : base(ex.Message, ex)
        {
        }
    }
}
