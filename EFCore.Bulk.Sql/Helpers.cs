using EFCore.Bulk.Sql.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace Bulk
{
    public static class Helpers
    {
        private const int timeOutDefault = 180;

        #region BulkInsert
        /// <summary>
        /// LINQ to DataTable to Bulk • Defines programmatically if it is whether SQL Server or MySQL.
        /// </summary>
        /// <param name="linq">A list — commonly resulting from a LINQ query.</param>
        /// <param name="context">Application's context.</param>
        /// <param name="table">Aiming table.</param>
        /// <param name="timeOut">Bulk copy time out in seconds.</param>
        /// <param name="isExceptionInPortuguese">Exception's text language.</param>
        /// <param name="isDisableFKCheck">Controlling the database FKs checking.</param>
        public static async Task BulkInsert<T, TContext>(List<T> linq, TContext context, string table, int? timeOut = timeOutDefault, bool? isExceptionInPortuguese = false, bool? isDisableFKCheck = false) where TContext : DbContext
        {
            if (context is null)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ParamConexNaoPodeSerNulo, en: ExceptionEnum.ParamConexNaoPodeSerNulo_EN));
            }

            DbConnection con = context.Database.GetDbConnection();

            if (con is SqlConnection)
            {
                await BulkInsert(linq, con as SqlConnection, table, isExceptionInPortuguese.GetValueOrDefault(), isDisableFKCheck.GetValueOrDefault(), timeOut);
            }
            else if (con is System.Data.SqlClient.SqlConnection)
            {
                await BulkInsert(linq, con as System.Data.SqlClient.SqlConnection, table, isExceptionInPortuguese.GetValueOrDefault(), isDisableFKCheck.GetValueOrDefault(), timeOut);
            }
            else if (con is MySqlConnection)
            {
                await BulkInsert(linq, con as MySqlConnection, table, isExceptionInPortuguese.GetValueOrDefault(), isDisableFKCheck.GetValueOrDefault(), timeOut);
            }
            else
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ParamConexaoDeveSer, en: ExceptionEnum.ParamConexaoDeveSer_EN, extra: $"[{con.GetType()}]"));
            }
        }

        /// <summary>
        /// LINQ to DataTable to Bulk • SQL Server;
        /// </summary>
        /// <param name="linq">A list — commonly resulting from a LINQ query.</param>
        /// <param name="con">SQL Server connection (Microsoft.Data.SqlClient).</param>
        /// <param name="table">Aiming table.</param>
        /// <param name="isExceptionInPortuguese">Exception's text language.</param>
        /// <param name="isDisableFKCheck">Controlling the database FKs checking.</param>
        /// <param name="timeOut">Bulk copy time out in seconds.</param>
        public static async Task BulkInsert<T>(List<T> linq, SqlConnection? con, string table, bool? isExceptionInPortuguese = false, bool? isDisableFKCheck = false, int? timeOut = timeOutDefault)
        {
            if (con is null)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ParamConexNaoPodeSerNulo, en: ExceptionEnum.ParamConexNaoPodeSerNulo_EN));
            }

            SqlBulkCopy sqlBulk = new(con)
            {
                DestinationTableName = table
            };

            DataTable dataTable = ConvertListToDataTable(linq, sqlBulk, isExceptionInPortuguese.GetValueOrDefault());

            try
            {
                await con.OpenAsync();

                if (isDisableFKCheck.GetValueOrDefault())
                {
                    using SqlCommand disableAllConstraintsCmd = new($"ALTER TABLE {table} NOCHECK CONSTRAINT ALL", con);
                    disableAllConstraintsCmd.ExecuteNonQuery();
                }

                sqlBulk.BulkCopyTimeout = timeOut ?? timeOutDefault;
                sqlBulk.BatchSize = 5000;
                await sqlBulk.WriteToServerAsync(dataTable);

                if (isDisableFKCheck.GetValueOrDefault())
                {
                    using SqlCommand enableAllConstraintsCmd = new($"ALTER TABLE {table} CHECK CONSTRAINT ALL", con);
                    enableAllConstraintsCmd.ExecuteNonQuery();
                }

                await con.CloseAsync();
                dataTable.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ErroInternoSalvar, en: ExceptionEnum.ErroInternoSalvar_EN, extra: $"{ex.Message}."));
            }
        }

        /// <summary>
        /// LINQ to DataTable to Bulk • SQL Server;
        /// </summary>
        /// <param name="linq">A list — commonly resulting from a LINQ query.</param>
        /// <param name="con">SQL Server connection (System.Data.SqlClient).</param>
        /// <param name="table">Aiming table.</param>
        /// <param name="isExceptionInPortuguese">Exception's text language.</param>
        /// <param name="isDisableFKCheck">Controlling the database FKs checking.</param>
        /// <param name="timeOut">Bulk copy time out in seconds.</param>
        public static async Task BulkInsert<T>(List<T> linq, System.Data.SqlClient.SqlConnection? con, string table, bool? isExceptionInPortuguese = false, bool? isDisableFKCheck = false, int? timeOut = timeOutDefault)
        {
            if (con is null)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ParamConexNaoPodeSerNulo, en: ExceptionEnum.ParamConexNaoPodeSerNulo_EN));
            }

            try
            {
                Microsoft.Data.SqlClient.SqlConnection conMicrosoft = new(con.ConnectionString);
                await BulkInsert(linq, conMicrosoft, table, isExceptionInPortuguese.GetValueOrDefault(), isDisableFKCheck.GetValueOrDefault(), timeOut);
            }
            catch (Exception)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ErroInternoConverterSQLServerDeSystemParaMicrosoft, en: ExceptionEnum.ErroInternoConverterSQLServerDeSystemParaMicrosoft_EN));
            }
        }

        /// <summary>
        /// LINQ to DataTable to Bulk • MySQL;
        /// </summary>
        /// <param name="linq">A list — commonly resulting from a LINQ query.</param>
        /// <param name="con">MySQL connection.</param>
        /// <param name="table">Aiming table.</param>
        /// <param name="isExceptionInPortuguese">Exception's text language.</param>
        /// <param name="isDisableFKCheck">Controlling the database FKs checking.</param>
        /// <param name="timeOut">Bulk copy time out in seconds.</param>
        public static async Task BulkInsert<T>(List<T> linq, MySqlConnection? con, string table, bool? isExceptionInPortuguese = false, bool? isDisableFKCheck = false, int? timeOut = timeOutDefault)
        {
            if (con is null)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ParamConexNaoPodeSerNulo, en: ExceptionEnum.ParamConexNaoPodeSerNulo_EN));
            }

            MySqlBulkCopy sqlBulk = new(con)
            {
                DestinationTableName = table
            };

            DataTable dataTable = ConvertListToDataTable(linq, null, isExceptionInPortuguese.GetValueOrDefault());

            try
            {
                await con.OpenAsync();

                if (isDisableFKCheck.GetValueOrDefault())
                {
                    using MySqlCommand disableFkCmd = new("SET foreign_key_checks = 0", con);
                    disableFkCmd.ExecuteNonQuery();
                }

                sqlBulk.BulkCopyTimeout = timeOut ?? timeOutDefault;
                await sqlBulk.WriteToServerAsync(dataTable);

                if (isDisableFKCheck.GetValueOrDefault())
                {
                    using MySqlCommand enableFkCmd = new("SET foreign_key_checks = 1", con);
                    enableFkCmd.ExecuteNonQuery();
                }

                await con.CloseAsync();
                dataTable.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ErroInternoSalvar, en: ExceptionEnum.ErroInternoSalvar_EN, extra: $"{ex.Message}."));
            }
        }
        #endregion

        #region BulkDelete
        /// <summary>
        /// Bulk delete data from a table using a DbContext.
        /// </summary>
        /// <param name="dbContext">Application's context.</param>
        /// <param name="condition">LINQ's "where" condition.</param>
        public static async Task BulkDelete<T>(DbContext dbContext, Expression<Func<T, bool>> condition, bool? isExceptionInPortuguese = false) where T : class
        {
            try
            {
                await dbContext.Set<T>().Where(condition).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ErroInternoDeletar, en: ExceptionEnum.ErroInternoDeletar_EN, extra: $"{ex.Message}."));
            }
        }
        #endregion

        #region helpers
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
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ErroInternoConverterDados, en: ExceptionEnum.ErroInternoConverterDados_EN, extra: $"{ex.Message}."));
            }
        }

        private static void MapColumns(SqlBulkCopy? sqlBulk, DataTable dataTable, PropertyInfo[] props, List<PropertyInfo> listTypes, bool isExceptionInPortuguese)
        {
            try
            {
                foreach (PropertyInfo prop in props)
                {
                    Type? type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);

                    if (!IsForeignKey(prop) && !IsNotMapped(prop) && !IsVirtual(prop) && !IsAbstract(prop) && !IsList(prop))
                    {
                        sqlBulk?.ColumnMappings.Add(prop.Name, prop.Name);
                        dataTable.Columns.Add(prop.Name, type!);

                        listTypes.Add(prop);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ErroInternoMapearTabelaVirtual, en: ExceptionEnum.ErroInternoMapearTabelaVirtual_EN, extra: $"{ex.Message}."));
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
                        if (!IsForeignKey(listTypes[i]) && !IsNotMapped(listTypes[i]) && !IsVirtual(listTypes[i]) && !IsAbstract(listTypes[i]) && !IsList(listTypes[i]))
                        {
                            values[i] = listTypes[i].GetValue(item, null)!;
                        }
                    }

                    dataTable.Rows.Add(values);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(GetExceptionText(isExceptionInPortuguese, br: ExceptionEnum.ErroInternoAtribuirValoresTabelaVirtual, en: ExceptionEnum.ErroInternoAtribuirValoresTabelaVirtual_EN, extra: $"{ex.Message}."));
            }
        }

        private static bool IsForeignKey(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var isCollection = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
            var isClass = propertyType.IsClass && propertyType != typeof(string);
            var isByteArray = propertyType == typeof(byte[]);

            return (isCollection || isClass) && !isByteArray;
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

        private static bool IsList(PropertyInfo property)
        {
            try
            {
                Type propertyType = property.PropertyType;

                if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(List<>) || propertyType.GetGenericTypeDefinition() == typeof(IQueryable<>) || propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetEnumDesc(Enum enumVal)
        {
            MemberInfo[] memInfo = enumVal.GetType().GetMember(enumVal.ToString());
            DescriptionAttribute? attribute = CustomAttributeExtensions.GetCustomAttribute<DescriptionAttribute>(memInfo[0]);

            return attribute!.Description;
        }

        private static string GetExceptionText(bool? isExceptionInPortuguese, ExceptionEnum br, ExceptionEnum en, string extra = "")
        {
            return $"{GetEnumDesc(isExceptionInPortuguese.GetValueOrDefault() ? br : en)}{(!string.IsNullOrEmpty(extra) ? $" {extra}" : string.Empty)}";
        }
        #endregion
    }
}