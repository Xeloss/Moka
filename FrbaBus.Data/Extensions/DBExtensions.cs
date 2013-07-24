using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using FrbaBus.Data.DataAttributes;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FrbaBus.Data.Extensions
{
    internal static class DBExtensions
    {
        public static SqlConnection GetConnection(this ConnectionStringSettings settings)
        {
            return new SqlConnection(settings.ConnectionString);
        }

        public static object GetField(this DataRow row, string fieldName)
        {
            try
            {
                return row[fieldName];
            }
            catch
            {
                return null;
            }
        }

        public static object Parse(this Type PropertyType, object Value)
        {
            if (Value is DBNull)
                return null;

            if (PropertyType.IsAssignableFrom(typeof(char)) && Value is string)
                return ((string)Value)[0];

            if (PropertyType.IsAssignableFrom(typeof(double)))
                return Convert.ToDouble(Value);

            if (PropertyType.IsAssignableFrom(typeof(decimal)))
                return Convert.ToDecimal(Value);

            if (PropertyType.IsAssignableFrom(typeof(Single)))
                return Convert.ToSingle(Value);

            if (PropertyType.IsEnum)
                return Enum.ToObject(PropertyType, int.Parse(Value.ToString()));

            return Value;
        }

        public static void AddParameters(this SqlParameterCollection parameterCollection, IEnumerable<object> Parameters)
        {
            foreach (var value in Parameters)
            {
                var aNewParameter = new SqlParameter()
                {
                    Value = value,
                    DbType = value.GetDBType()
                };

                parameterCollection.Add(aNewParameter);
            }

        }

        public static bool IsMapped(this PropertyInfo propertyInfo)
        {
            return !propertyInfo.GetCustomAttributes(typeof(NotMappedAttribute), true).Any();
        }

        public static bool IsPrimaryKey(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).Any();
        }

        public static bool IsIdentity(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(IdentityAttribute), true).Any();
        }

        public static bool ShouldUpdate(this PropertyInfo propertyInfo)
        {
            var attr = propertyInfo.GetCustomAttributes(typeof(IdentityAttribute), true).FirstOrDefault();
            if (attr == null)
                return false;

            return ((IdentityAttribute)attr).ActualizarEnInsert;
        }

        public static string ToStringParameterList(this object[] parameters)
        {
            var sb = new StringBuilder("");

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (i > 0)
                    sb.Append(", ");

                sb.Append(parameter.AsSQLLiteral());
            }

            return sb.ToString();
        }

        public static string AsSQLLiteral(this object anObject)
        {
            if (anObject == null)
                return "NULL";
            else if (anObject is string || anObject is char)
                return string.Format("'{0}'", anObject);
            else if (anObject is DateTime)
                return string.Format("'{0}'",((DateTime)anObject).ToSQLDate());
            else if (anObject is bool)
                return ((bool)anObject ? 1 : 0).ToString();
            else
            {
                if (anObject is Enum)
                {
                    var underlyingType = Enum.GetUnderlyingType(anObject.GetType());
                    var value = Convert.ChangeType(anObject, underlyingType);
                    return value.ToString();
                }

                return anObject.ToString();
            }
        }

        private static DbType GetDBType(this object anObject)
        {
            if (anObject is string || anObject is char)
                return DbType.String;

            if (anObject is bool)
                return DbType.Boolean;

            if (anObject is Byte)
                return DbType.Byte;

            if (anObject is DateTime)
                return DbType.DateTime;

            if (anObject is decimal)
                return DbType.Decimal;

            if (anObject is double)
                return DbType.Double;

            if (anObject is int)
                return DbType.Int32;

            if (anObject is float)
                return DbType.Single;

            return DbType.Object;
        }
    }
}
