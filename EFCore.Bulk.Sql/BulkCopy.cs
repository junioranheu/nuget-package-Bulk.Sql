using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Bulk
{
    public static class BulkCopy
    {
        private const int timeOutDefault = 180;

        /// <summary>
        /// LINQ to DataTable to Bulk • Defines programmatically if it is whether SQL Server or MySQL.
        /// </summary>
        /// <param name="linq">A list — commonly resulting from a LINQ query.</param>
        /// <param name="context">Application's context.</param>
        /// <param name="table">Aiming table.</param>
        /// <param name="timeOut">Bulk copy time out in seconds.</param>
        public static async Task BulkInsert<T, TContext>(List<T> linq, TContext context, string table, int? timeOut = timeOutDefault, bool? isExceptionInPortuguese = false) where TContext : DbContext
        {
            if (context is null)
            {
                throw new Exception(isExceptionInPortuguese.GetValueOrDefault() ? "O parâmetro de conexão não deve ser nulo" : "The connection parameter must not be null");
            }

            DbConnection con = context.Database.GetDbConnection();

            if (con is SqlConnection)
            {
                await BulkInsert(linq, con as SqlConnection, table, timeOut);
            }
            else if (con is MySqlConnection)
            {
                await BulkInsert(linq, con as MySqlConnection, table, timeOut);
            }
            else
            {
                throw new Exception(isExceptionInPortuguese.GetValueOrDefault() ? $"O parâmetro de conexão deve ser do tipo 'Microsoft.Data.SqlClient.SqlConnection' ou 'MySqlConnection'. Tipo atual: {con.GetType()}" : $"The connection parameter must be a 'Microsoft.Data.SqlClient.SqlConnection' or 'MySqlConnection' type. Current type: {con.GetType()}");
            }
        }

        /// <summary>
        /// LINQ to DataTable to Bulk • SQL Server;
        /// </summary>
        /// <param name="linq">A list — commonly resulting from a LINQ query.</param>
        /// <param name="con">SqlConnection.</param>
        /// <param name="table">Aiming table.</param>
        /// <param name="timeOut">Bulk copy time out in seconds.</param>
        public static async Task BulkInsert<T>(List<T> linq, SqlConnection? con, string table, int? timeOut = timeOutDefault, bool? isExceptionInPortuguese = false)
        {
            if (con is null)
            {
                throw new Exception(isExceptionInPortuguese.GetValueOrDefault() ? "O parâmetro de conexão não deve ser nulo" : "The connection parameter must not be null");
            }

            SqlBulkCopy sqlBulk = new(con)
            {
                DestinationTableName = table
            };

            DataTable dataTable = ConvertListToDataTable(linq, sqlBulk, isExceptionInPortuguese.GetValueOrDefault());

            try
            {
                await con.OpenAsync();
                sqlBulk.BulkCopyTimeout = timeOut ?? timeOutDefault;
                sqlBulk.BatchSize = 5000;
                await sqlBulk.WriteToServerAsync(dataTable);

                await con.CloseAsync();
                dataTable.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception(isExceptionInPortuguese.GetValueOrDefault() ? $"Houve um erro interno ao salvar os dados. {ex.Message}" : $"There was an internal error while saving the data. {ex.Message}");
            }
        }

        /// <summary>
        /// LINQ to DataTable to Bulk • MySQL;
        /// </summary>
        /// <param name="linq">A list — commonly resulting from a LINQ query.</param>
        /// <param name="con">SqlConnection.</param>
        /// <param name="table">Aiming table.</param>
        /// <param name="timeOut">Bulk copy time out in seconds.</param>
        public static async Task BulkInsert<T>(List<T> linq, MySqlConnection? con, string table, int? timeOut = timeOutDefault, bool? isExceptionInPortuguese = false)
        {
            if (con is null)
            {
                throw new Exception(isExceptionInPortuguese.GetValueOrDefault() ? "O parâmetro de conexão não deve ser nulo" : "The connection parameter must not be null");
            }

            MySqlBulkCopy sqlBulk = new(con)
            {
                DestinationTableName = table
            };

            DataTable dataTable = ConvertListToDataTable(linq, null, isExceptionInPortuguese.GetValueOrDefault());

            try
            {
                await con.OpenAsync();
                sqlBulk.BulkCopyTimeout = timeOut ?? timeOutDefault;
                await sqlBulk.WriteToServerAsync(dataTable);

                await con.CloseAsync();
                dataTable.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception(isExceptionInPortuguese.GetValueOrDefault() ? $"Houve um erro interno ao salvar os dados. {ex.Message}" : $"There was an internal error while saving the data. {ex.Message}");
            }
        }

        #region helpers;
        private static DataTable ConvertListToDataTable<T>(List<T> linq, SqlBulkCopy? sqlBulk, bool isExceptionInPortuguese)
        {
            try
            {
                DataTable dataTable = new(typeof(T).Name);
                PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                List<PropertyInfo> listTypes = new();

                MapColumns(sqlBulk, dataTable, props, listTypes, isExceptionInPortuguese);
                PopulateTable(linq, dataTable, listTypes, isExceptionInPortuguese);

                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception(isExceptionInPortuguese ? $"Houve um erro interno ao converter os dados. {ex.Message}" : $"There was an internal error while converting the data. {ex.Message}");
            }
        }

        private static void MapColumns(SqlBulkCopy? sqlBulk, DataTable dataTable, PropertyInfo[] props, List<PropertyInfo> listTypes, bool isExceptionInPortuguese)
        {
            try
            {
                foreach (PropertyInfo prop in props)
                {
                    Type? type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);

                    if (!IsForeignKey(prop) && !IsNotMapped(prop) && !IsVirtual(prop) && !IsAbstract(prop))
                    {
                        sqlBulk?.ColumnMappings.Add(prop.Name, prop.Name);
                        dataTable.Columns.Add(prop.Name, type!);

                        listTypes.Add(prop);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(isExceptionInPortuguese ? $"Houve um erro interno ao mapear a tabela virtual de dados. {ex.Message}" : $"There was an internal error while mapping the DataTable. {ex.Message}");
            }
        }

        private static void PopulateTable<T>(List<T> linq, DataTable dataTable, List<PropertyInfo> listTypes, bool isExceptionInPortuguese)
        {
            try
            {
                foreach (T item in linq)
                {
                    var values = new object[dataTable.Columns.Count];

                    for (int i = 0; i < values.Length; i++)
                    {
                        if (!IsForeignKey(listTypes[i]) && !IsNotMapped(listTypes[i]))
                        {
                            values[i] = listTypes[i].GetValue(item, null)!;
                        }
                    }

                    dataTable.Rows.Add(values);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(isExceptionInPortuguese ? $"Houve um erro interno ao atribuir valores à tabela virtual de dados. {ex.Message}" : $"There was an internal error while assigning values ​​to the DataTable. {ex.Message}");
            }
        }

        private static bool IsForeignKey(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var isCollection = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
            var isClass = propertyType.IsClass && propertyType != typeof(string);

            return isCollection || isClass;
        }

        private static bool IsNotMapped(PropertyInfo property)
        {
            return Attribute.IsDefined(property, typeof(NotMappedAttribute));
        }

        private static bool IsVirtual(PropertyInfo property)
        {
            try
            {
                return property.GetGetMethod()?.IsVirtual == true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsAbstract(PropertyInfo property)
        {
            try
            {
                return property.GetGetMethod()?.IsAbstract == true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion;
    }
}