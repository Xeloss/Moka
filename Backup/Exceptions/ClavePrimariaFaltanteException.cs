using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace FrbaBus.Data.Exceptions
{
    public class ClavePrimariaFaltanteException : MissingPrimaryKeyException
    {
        private string message;

        public ClavePrimariaFaltanteException(string TypeName)
        {
            this.message = string.Format(@"La clase {0} no especifica una clave primaria.
                                           Para realizar esta operación, es necesario decorar el campo que represente dicha clave primaria con FrbaBus.Data.DataAttributes.PrimaryKey");
        }

        public override string Message
        {
            get
            {
                return message;
            }
        }
    }
}
