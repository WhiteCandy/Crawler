using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelper;
using DatabaseCore.Attributes;
using DatabaseCore.Model;

namespace DatabaseCore
{
    public enum DDLType
    {
        CreateTable,
        ModifyTable,
        DropTable,
    }

    public static class DatabaseHelper
    {
        public static string ToDatabaseType(this string type)
        {
            var lowerType = type.ToLower();
            if (lowerType == "int32") return "int";
            if (lowerType == "int64") return "bigint";
            if (lowerType == "double") return "double";
            if (lowerType == "string") return "text";
            if (lowerType == "datetime") return "datetime";

            return type.ToLower();
        }

        public static string DatabaseDefaultValue(this string type)
        {
            var lowerType = type.ToLower();
            if (lowerType == "int32") return "int";
            if (lowerType == "int64") return "bigint";
            if (lowerType == "double") return "double";
            if (lowerType == "string") return "text";
            if (lowerType == "datetime") return "datetime";

            var typeInfo = Type.GetType(type);
            if (typeInfo != null && typeInfo.IsValueType)
                return Activator.CreateInstance(typeInfo).ToString();
            return "";
        }

        public static Table ToDatabaseScheme<T>()
        {
            var typeInfo = typeof (T);
            if (!typeInfo.HasAttribute<TableAttribute>())
                return null;

            var table = new Table { Name = typeInfo.Name };
            foreach (var member in typeInfo.GetFields())
            {
                table.ElementList.Add(new Element()
                {
                    Name = member.Name,
                    Type = member.FieldType.Name.ToDatabaseType(),
                    IsKey = member.HasAttribute<KeyAttribute>(),
                });
            }
            return table;
        }

        public static string GenerateDDL(this Table table, DDLType type)
        {
            switch (type)
            {
                case DDLType.CreateTable:
                    return table.CreateTableQuery();
                case DDLType.ModifyTable:
                    return table.ModifyTableQuery();
                case DDLType.DropTable:
                    return table.DropTableQuery();
            }

            return null;
        }

        private static string CreateTableQuery(this Table table)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append(string.Format("CREATE TABLE {0}", table.Name) + Environment.NewLine);
            queryBuilder.Append("(" + Environment.NewLine);
            foreach (var elem in table.ElementList)
            {
                queryBuilder.Append(
                    string.Format("\t{0} {1}{2}{3}",
                        elem.Name,
                        elem.Type,
                        ",",
                        Environment.NewLine));
            }
            queryBuilder.Append(string.Format("\tPRIMARY KEY ({0}){1}",
                string.Join(", ", table.ElementList.Where(e => e.IsKey).Select(e => e.Name)),
                Environment.NewLine));
            
            queryBuilder.Append(")" + Environment.NewLine);
            queryBuilder.Append(@"ENGINE=InnoDB CHARACTER SET utf8 COLLATE utf8_general_ci");

            return queryBuilder.ToString();
        }

        private static string ModifyTableQuery(this Table table)
        {
            return "";
        }

        private static string DropTableQuery(this Table table)
        {
            return string.Format("DROP TABLE {0}", table.Name);
        }

        private static string CheckTableExistQuery(this Table table)
        {
            return string.Format("SHOW TABLES LIKE \"{0}\"", table.Name);
        }
    }
}