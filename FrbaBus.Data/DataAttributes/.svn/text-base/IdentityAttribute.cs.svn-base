using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrbaBus.Data.DataAttributes
{
    /// <summary>
    /// Indica que esta propiedad corresponde a un campo autoincremental en la base
    /// de datos (es excluida en los Inserts, pero se actualiza con el valor proporcionado
    /// por la base de datos)
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class IdentityAttribute : Attribute
    {
        public bool ActualizarEnInsert = true;
    }
}
