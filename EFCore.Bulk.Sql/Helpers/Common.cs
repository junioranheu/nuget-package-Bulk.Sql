using EFCore.Bulk.Sql.Enums;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EFCore.Bulk.Sql.Helpers
{
    public static class Common
    {
        public static DataTable ConvertListToDataTable<T>(List<T> linq, SqlBulkCopy? sqlBulk, bool isExceptionInPortuguese)
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

        public static void MapColumns(SqlBulkCopy? sqlBulk, DataTable dataTable, PropertyInfo[] props, List<PropertyInfo> listTypes, bool isExceptionInPortuguese)
        {
            try
            {
                foreach (PropertyInfo prop in props)
                {
                    Type? type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;

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

        public static void PopulateTable<T>(List<T> linq, DataTable dataTable, List<PropertyInfo> listTypes, bool isExceptionInPortuguese)
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

        public static bool IsForeignKey(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var isCollection = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
            var isClass = propertyType.IsClass && propertyType != typeof(string);
            var isByteArray = propertyType == typeof(byte[]);

            return (isCollection || isClass) && !isByteArray;
        }

        public static bool IsNotMapped(PropertyInfo property)
        {
            return Attribute.IsDefined(property, typeof(NotMappedAttribute));
        }

        public static bool IsVirtual(PropertyInfo property)
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

        public static bool IsAbstract(PropertyInfo property)
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

        public static bool IsList(PropertyInfo property)
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

        public static string GetEnumDesc(Enum enumVal)
        {
            MemberInfo[] memInfo = enumVal.GetType().GetMember(enumVal.ToString());
            DescriptionAttribute? attribute = memInfo[0].GetCustomAttribute<DescriptionAttribute>();

            return attribute!.Description;
        }

        public static string GetExceptionText(bool? isExceptionInPortuguese, ExceptionEnum br, ExceptionEnum en, string extra = "")
        {
            return $"{GetEnumDesc(isExceptionInPortuguese.GetValueOrDefault() ? br : en)}{(!string.IsNullOrEmpty(extra) ? $" {extra}" : string.Empty)}";
        }

        public static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var startUnderscores = Regex.Match(name, @"^_+");
            var output = startUnderscores + Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();

            return output;
        }
    }
}