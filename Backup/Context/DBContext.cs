using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

using FrbaBus.Data.Extensions;
using FrbaBus.Common.Extensions;
using FrbaBus.Data.Exceptions;

namespace FrbaBus.Data.Context
{
    /// <summary>
    /// Permite conectar y operar con la base de datos
    /// </summary>
    public class DBContext
    {
        public ConnectionStringSettings ConectionString { get; set; }

        public string Schema { get; protected set; }

        /// <summary>
        /// Inicializa el objeto con el schema y el ConectionString Default del App.config
        /// </summary>
        public DBContext()
        {
            try
            {
                this.Schema = ConfigurationManager.AppSettings["Schema"];
                this.ConectionString = ConfigurationManager.ConnectionStrings["Default"];
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new Exception("No se encontro un ConectionString válido", ex);
            }
        }
        /// <summary>
        /// Inicializa el objeto con el schema indicado y el ConectionString 
        /// que en el App.config tenga el nombre especificado
        /// </summary>
        public DBContext(string ConnectionStringName, string SchemaName)
        {
            if (ConnectionStringName.IsNullOrEmpty())
                throw new ArgumentNullException("ConnectionStringName");

            if (SchemaName.IsNullOrEmpty())
                throw new ArgumentNullException("SchemaName");

            try
            {
                this.Schema = SchemaName;
                this.ConectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new Exception("No se encontro un ConectionString válido", ex);
            }
        }
        /// <summary>
        /// Crea un DBContext usando el connection string y el schema especificados
        /// </summary>
        public static DBContext CreateUsing(string AConnectionString, string AnSchema)
        {
            if (AConnectionString.IsNullOrEmpty())
                throw new ArgumentNullException("AConnectionString");

            if (AnSchema.IsNullOrEmpty())
                throw new ArgumentNullException("AnSchema");

            var connectionString = new ConnectionStringSettings("Custom", AConnectionString);

            return new DBContext()
            {
                ConectionString = connectionString,
                Schema = AnSchema
            };
        }

        /// <summary>
        /// Selecciona todos los elementos de una tabla de igual nombre que la clase
        /// proporcionada y devuelve los elementos en una coleccion de dicha clase.
        /// </summary>
        /// <typeparam name="T">Clase con la que se mapearan los resultados de la consulta</typeparam>
        public IEnumerable<T> SelectAll<T>() where T : class, new()
        {
            var type = typeof(T);

            return this.SelectAll<T>(type.Name);
        }
        public IEnumerable<T> SelectAll<T>(string FromTable) where T : class, new()
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = string.Format("Select * from [{0}].[{1}]", this.Schema, FromTable);
            aCommand.CommandType = System.Data.CommandType.Text;

            return this.Retrieve<T>(aCommand);
        }

        /// <summary>
        /// Inserta un elemento de la clase proporcionada en la tabla de igual nombre
        /// que dicha clase.
        /// Si la clase define un campo como Identity este se actualizara en el objeto
        /// con el valor proporcionado por la base.
        /// </summary>
        public void Insert<T>(T aNewEntity)
        {
            var tableName = typeof(T).Name;
            this.Insert(aNewEntity, tableName);
        }
        public void Insert<T>(T aNewEntity, string IntoTable)
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = this.BuildInsertStatement(aNewEntity, IntoTable);
            aCommand.CommandType = CommandType.Text;

            var insertedId = this.ExecuteScalar(aCommand) as Nullable<int>;

            if (insertedId.Exists())
                this.UpdateIdentityFieldOf(aNewEntity, insertedId.Value);
        }

        /// <summary>
        /// Actualiza un elemento de la clase proporcionada en la tabla de igual nombre
        /// que dicha clase. La clase debe tener por lo menos una propiedad con el 
        /// atributo PrimaryKey
        /// </summary>
        public void Update<T>(T anEntity)
        {
            var tableName = typeof(T).Name;
            this.Update(anEntity, tableName);
        }
        public void Update<T>(T anEntity, string FromTable)
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = this.BuildUpdateStatement(anEntity, FromTable);
            aCommand.CommandType = CommandType.Text;

            this.ExecuteNonQuery(aCommand);
        }

        /// <summary>
        /// Borra un elemento de la clase proporcionada en la tabla de igual nombre
        /// que dicha clase. La clase debe tener por lo menos una propiedad con el 
        /// atributo PrimaryKey
        /// </summary>
        public void Delete<T>(T anEntity)
        {
            var type = typeof(T);
            this.Delete(anEntity, type.Name);
        }
        public void Delete<T>(T anEntity, string FromTable)
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = this.BuildDeleteStatement(anEntity, FromTable);
            aCommand.CommandType = CommandType.Text;

            this.ExecuteNonQuery(aCommand);
        }

        /// <summary>
        /// Ejecuta el stored procedure especificado con los parametros indicados 
        /// y devuelve la cantidad de registros afectados.
        /// </summary>
        public int ExecuteProcedure(string StoredProcedure, params object[] Parameters)
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = this.BuildStoreProcedureStatement(StoredProcedure, Parameters);
            aCommand.CommandType = CommandType.Text;

            return this.ExecuteNonQuery(aCommand);
        }

        /// <summary>
        /// Ejecuta el stored procedure especificado con los parametros indicados y devuelve
        /// el resultado como una coleccion de la clase especificada.
        /// </summary>
        public IEnumerable<T> ExecuteAndRetrieveAs<T>(string StoredProcedure, params object[] Parameters)
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = this.BuildStoreProcedureStatement(StoredProcedure, Parameters);
            aCommand.CommandType = CommandType.Text;

            return this.Retrieve<T>(aCommand);
        }

        /// <summary>
        /// Ejecuta la query especificada y devuelve el resultado en una coleccion
        /// de la clase especificada
        /// </summary>
        public IEnumerable<T> SelectAndRetrieveAs<T>(string SelectStatement)
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = SelectStatement;
            aCommand.CommandType = CommandType.Text;

            return this.Retrieve<T>(aCommand);
        }

        /// <summary>
        /// Ejecuta la query especificada y devuelve la cantidad de filas afectadas
        /// </summary>
        public int ExecuteStatement(string Statement)
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = Statement;
            aCommand.CommandType = CommandType.Text;

            return this.ExecuteNonQuery(aCommand);
        }

        /// <summary>
        /// Ejecuta la query especificada y devuelve el valor de la primer columna del primer registro
        /// (si existe, sino null)
        /// </summary>
        public object ExecuteScalar(string Statement)
        {
            var aCommand = new SqlCommand();
            aCommand.CommandText = Statement;
            aCommand.CommandType = CommandType.Text;

            return this.ExecuteScalar(aCommand);
        }

        /// <summary>
        /// Genera un contexto que permite el uso de transacciones
        /// </summary>
        public virtual TransactionalDBContext AsTransactional()
        {
            return new TransactionalDBContext() { ConectionString = this.ConectionString };
        }

        protected virtual int ExecuteNonQuery(SqlCommand aCommand)
        {
            try
            {
                var result = 0;

                using (var aConnection = this.ConectionString.GetConnection())
                {
                    aCommand.Connection = aConnection;

                    aConnection.Open();
                    result = aCommand.ExecuteNonQuery();
                    aConnection.Close();
                }

                return result;
            }
            catch (SqlException ex)
            {
                throw this.WrapExcetion(ex);
            }
        }
        protected virtual object ExecuteScalar(SqlCommand aCommand)
        {
            try
            {
                object result;

                using (var aConnection = this.ConectionString.GetConnection())
                {
                    aCommand.Connection = aConnection;

                    aConnection.Open();
                    result = aCommand.ExecuteScalar();
                    aConnection.Close();
                }

                return result;
            }
            catch (SqlException ex)
            {
                throw this.WrapExcetion(ex);
            }
        }
        protected virtual IEnumerable<T> Retrieve<T>(SqlCommand aCommand)
        {
            IEnumerable<T> result;

            using (var aConnection = this.ConectionString.GetConnection())
            {
                aCommand.Connection = aConnection;

                var dataSet = new DataSet("SomeTable");
                var dataAdapter = new SqlDataAdapter(aCommand);

                dataAdapter.TableMappings.Add("Table", "SomeTable");
                dataAdapter.SelectCommand = aCommand;

                aConnection.Open();
                dataAdapter.Fill(dataSet);
                aConnection.Close();

                var table = dataSet.Tables["SomeTable"].AsEnumerable();
                result = this.Materialize<T>(table);
            }

            return result;
        }
        protected IEnumerable<T> Materialize<T>(EnumerableRowCollection<DataRow> Table)
        {
            var result = new List<T>();

            var type = typeof(T);
            var objectFields = type.GetProperties()
                                   .Where(p => p.IsMapped());

            foreach (var row in Table)
            {
                var newObject = Activator.CreateInstance<T>();

                foreach (var objectField in objectFields)
                {
                    var value = row.GetField(objectField.Name);
                    if (!value.Exists())
                        continue;

                    var parsedValue = objectField.PropertyType.Parse(value);

                    objectField.SetValue(newObject, parsedValue, null);
                }

                result.Add(newObject);
            }

            return result;
        }
        protected DataBaseException WrapExcetion(SqlException ex)
        {
            switch (ex.Number)
            {
                case 515: return new NullValueException(ex);
                case 547: return new CheckConstraintException(ex);
                case 2627: return new UniqueConstraintException(ex);
                
                default: return new DataBaseException(ex);
            }
        }

        private string BuildStoreProcedureStatement(string StoreProceudreName, object[] Parameters)
        {
            return string.Format("EXEC [{0}].{1} {2}", this.Schema, StoreProceudreName, Parameters.ToStringParameterList());
        }
        private string BuildInsertStatement<T>(T entity, string forTable)
        {
            var insertFields = new StringBuilder("");
            var insertValues = new StringBuilder("");

            var type = typeof(T);
            var identityField = type.GetProperties()
                                    .Where(p => p.IsMapped() && p.IsIdentity())
                                    .FirstOrDefault();

            var output = string.Empty;
            if (identityField.Exists() && identityField.ShouldUpdate())
                output = string.Format("OUTPUT INSERTED.{0}", identityField.Name);

            var objectFields = type.GetProperties()
                                   .Where(p => p.IsMapped() && !p.IsIdentity())
                                   .ToArray();

            for (int i = 0; i < objectFields.Length; i++)
            {
                if (i > 0)
                {
                    insertFields.Append(',');
                    insertValues.Append(',');
                }

                var value = objectFields[i].GetValue(entity, null);

                insertFields.Append(objectFields[i].Name);
                insertValues.Append(value.AsSQLLiteral());
            }

            var statement = string.Format(@"INSERT INTO [{0}].[{1}]({2}) 
                                            {3}
                                            VALUES ({4})"
                                          , this.Schema, forTable, insertFields.ToString(), output, insertValues.ToString());
            return statement;
        }
        private string BuildDeleteStatement<T>(T entity, string fromTable)
        {
            var condition = this.BuildPredicate(entity);

            return string.Format("DELETE FROM [{0}].[{1}] WHERE {2}",this.Schema, fromTable, condition);
        }
        private string BuildUpdateStatement<T>(T entity, string fromTable)
        {
            var predicate = this.BuildPredicate(entity);
            var fields = this.BuildUpdateFields(entity);

            return string.Format("UPDATE [{0}].[{1}] SET {2} WHERE {3}", this.Schema, fromTable, fields, predicate);
        }
        private string BuildUpdateFields<T>(T anEntity)
        {
            var type = typeof(T);
            var objectFields = type.GetProperties()
                                   .Where(p => p.IsMapped() && !p.IsIdentity())
                                   .ToArray();

            var fields = new StringBuilder("");

            for (int i = 0; i < objectFields.Length; i++)
            {
                if (i > 0)
                    fields.Append(", ");

                var value = objectFields[i].GetValue(anEntity, null);
                fields.Append(string.Format("{0} = {1}", objectFields[i].Name, value.AsSQLLiteral()));
            }

            return fields.ToString();
        }
        private string BuildPredicate<T>(T anEntity)
        {
            var type = typeof(T);
            var objectFields = type.GetProperties()
                                   .Where(p => p.IsMapped() && p.IsPrimaryKey())
                                   .ToArray();

            if (objectFields.IsEmpty())
                throw new ClavePrimariaFaltanteException(type.Name);

            var conditions = new StringBuilder("");

            for (int i = 0; i < objectFields.Length; i++)
            {
                if (i > 0)
                    conditions.Append(" AND ");

                var value = objectFields[i].GetValue(anEntity, null);
                conditions.Append(string.Format("{0} = {1}", objectFields[i].Name, value.AsSQLLiteral()));
            }

            return conditions.ToString();
        }

        private void UpdateIdentityFieldOf<T>(T anEntity, int fieldValue)
        {
            var type = typeof(T);
            var identityField = type.GetProperties()
                                    .Where(p => p.IsMapped() && p.IsIdentity())
                                    .FirstOrDefault();
            if (identityField.Exists())
                identityField.SetValue(anEntity, fieldValue, null);
        }
    }

    /// <summary>
    /// Permite ejecutar consultas dentro de transacciones
    /// </summary>
    public class TransactionalDBContext : DBContext, IDisposable
    {
        SqlConnection currentConnection;
        SqlTransaction activeTransaction;
        int NestingLevel;

        public TransactionalDBContext()
            : base()
        {
            this.currentConnection = this.ConectionString.GetConnection();
        }
        public TransactionalDBContext(string ConnectionStringName, string SchemaName)
            : base(ConnectionStringName, SchemaName)
        {
            this.currentConnection = this.ConectionString.GetConnection();
        }
        public new static TransactionalDBContext CreateUsing(string AConnectionString, string AnSchema)
        {
            if (AConnectionString.IsNullOrEmpty())
                throw new ArgumentNullException("AConnectionString");

            var connectionString = new ConnectionStringSettings("Custom", AConnectionString);

            return new TransactionalDBContext()
            {
                ConectionString = connectionString,
                Schema = AnSchema
            };
        }

        /// <summary>
        /// Inicia y apila una nueva transaccion. Abre y mantiene una conexion con 
        /// la base de datos hasta que se ejecute el rollback o commit de todas las
        /// transacciones que se inicien o se llame al metodo dispose.
        /// </summary>
        public void BeginTransaction()
        {
            this.OpenConnection();

            if(!activeTransaction.Exists())
                this.activeTransaction = currentConnection.BeginTransaction();
        }

        /// <summary>
        /// Ejecuta el rollback de todas las transacciones iniciadas
        /// </summary>
        public void RollBackTransaction()
        {
            if (NestingLevel == 0)
            {
                if (this.activeTransaction.Exists())
                {
                    this.activeTransaction.Rollback();
                    this.activeTransaction.Dispose();
                    this.activeTransaction = null;
                }
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Ejecuta el commit de la transaccion iniciada
        /// </summary>
        public void CommitTransaction()
        {
            if (NestingLevel == 0)
            {
                if (this.activeTransaction.Exists())
                {
                    this.activeTransaction.Commit();
                    this.activeTransaction.Dispose();
                    this.activeTransaction = null;
                }

                this.CloseConnection();
            }
        }

        /// <summary>
        /// Hace rollback de todas las transacciones, cierra la conexion y hace
        /// un dispose de la misma
        /// </summary>
        public void Dispose()
        {
            if (NestingLevel == 0)
            {
                this.RollBackTransaction();
                this.CloseConnection();
                this.currentConnection.Dispose();
            }
            else
                NestingLevel--;
        }

        public override TransactionalDBContext AsTransactional()
        {
            this.NestingLevel++;
            return this;
        }

        protected override int ExecuteNonQuery(SqlCommand aCommand)
        {
            if (this.currentConnection.State != ConnectionState.Open)
                return base.ExecuteNonQuery(aCommand);

            try
            {
                aCommand.Connection = this.currentConnection;
                aCommand.Transaction = this.activeTransaction;

                return aCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw this.WrapExcetion(ex);
            }
        }
        protected override object ExecuteScalar(SqlCommand aCommand)
        {
            if (this.currentConnection.State != ConnectionState.Open)
                return base.ExecuteNonQuery(aCommand);

            try
            {
                aCommand.Connection = this.currentConnection;
                aCommand.Transaction = this.activeTransaction;

                return aCommand.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw this.WrapExcetion(ex);
            }
        }

        protected override IEnumerable<T> Retrieve<T>(SqlCommand aCommand)
        {
            if (this.currentConnection.State != ConnectionState.Open)
                return base.Retrieve<T>(aCommand);

            aCommand.Connection = this.currentConnection;
            aCommand.Transaction = this.activeTransaction;

            var dataSet = new DataSet("SomeTable");
            var dataAdapter = new SqlDataAdapter(aCommand);

            dataAdapter.TableMappings.Add("Table", "SomeTable");
            dataAdapter.SelectCommand = aCommand;

            dataAdapter.Fill(dataSet);

            var table = dataSet.Tables["SomeTable"].AsEnumerable();

            return this.Materialize<T>(table);
        }

        private void OpenConnection()
        {
            if (currentConnection.State == ConnectionState.Closed)
                currentConnection.Open();
        }
        private void CloseConnection()
        {
            if (currentConnection.State == ConnectionState.Open)
                currentConnection.Close();
        }
    }
}