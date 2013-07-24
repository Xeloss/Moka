using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrbaBus.Data.DataAttributes
{
    /// <summary>
    /// Indica el o los campos que son clave primaria en la base de datos
    /// (Esta propiedad es obligatoria para Update y Delete)
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class PrimaryKeyAttribute : Attribute
    { }
}
