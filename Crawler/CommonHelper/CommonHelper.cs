using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelper
{
    public static class AttributeHelper
    {
        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof (T), false).OfType<T>().FirstOrDefault();
        }

        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(typeof (T), false).OfType<T>().FirstOrDefault();
        }

        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return GetAttribute<T>(type) != null;
        }

        public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return GetAttribute<T>(member) != null;
        }
    }

    public static class DataTableHelper
    {
        public static void Load(this DataTable table, IDataReader reader, bool createColumns)
        {
            if (createColumns)
            {
                table.Columns.Clear();
                var schemaTable = reader.GetSchemaTable();
                foreach (DataRowView row in schemaTable.DefaultView)
                {
                    var columnName = (string)row["ColumnName"];
                    var type = (Type)row["DataType"];
                    table.Columns.Add(columnName, type);
                }
            }

            table.Load(reader);
        }
    }

}
