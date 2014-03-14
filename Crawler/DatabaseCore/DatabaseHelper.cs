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

        public static string CreateTableQuery(this Table table)
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

        public static string ModifyTableQuery(this Database database, Table classSchema, DataTable databaseSchema)
        {
            // validate field types
            #region Validate Schema

            foreach (var elem in classSchema.ElementList)
            {
                var classElemName = elem.Name;
                var classElemType = elem.Type;
                var classElemIsKey = elem.IsKey;
                var existField = false;
                foreach (DataRow row in databaseSchema.Rows)
                {
                    if (row["ColumnName"].ToString() != classElemName) continue;

                    var dbElemType = ((Type)row["DataType"]).Name.ToDatabaseType();
                    var dbElemIsKey = (bool)row["IsKey"];

                    if (dbElemType != classElemType)
                    {
                        const string errorMessage = "Element [{0}:{1}] type [{2}] and database element type [{3}] is not matched!";
                        var formattedMessage = string.Format(errorMessage,
                            classSchema.Name, classElemName, classElemType, dbElemType);
                        throw new Exception(formattedMessage);
                    }

                    if (classElemIsKey != dbElemIsKey)
                    {
                        const string errorMessage = "Element [{0}:{1}] key setting is not matched!";
                        var formattedMessage = string.Format(errorMessage,
                            classSchema.Name, classElemName, classElemType, dbElemType);
                        throw new Exception(formattedMessage);
                    }

                    existField = true;
                    break;
                }

                if (!existField && classElemIsKey)
                {
                    const string errorMessage = "Key setting of table [{0}] can't be change in here. Use tools like phpmyadmin to change key";
                    var formattedMessage = string.Format(errorMessage, classSchema.Name);
                    throw new Exception(formattedMessage);
                }
            }

            #endregion

            #region Sync Schema

            var queryBuilder = new StringBuilder();
            var lastColumnName = "";
            foreach (var elem in classSchema.ElementList)
            {
                var classElemName = elem.Name;
                var classElemType = elem.Type;
                var existField = false;
                foreach (DataRow row in databaseSchema.Rows)
                {
                    if (row["ColumnName"].ToString() != classElemName) continue;
                    existField = true;
                    break;
                }

                if (!existField)
                {
                    if (queryBuilder.Length > 0)
                        queryBuilder.AppendLine(",");
                    
                    var alterQuery = string.Format("\tADD COLUMN {0} {1}", classElemName, classElemType);
                    queryBuilder.Append(alterQuery);
                    if(lastColumnName.Length > 0)
                        queryBuilder.Append(string.Format(" AFTER {0}", lastColumnName));
                }
                lastColumnName = classElemName;
            }

            var alterTableQuery = "";
            if (queryBuilder.Length > 0)
            {
                alterTableQuery = string.Format("ALTER TABLE {0}", classSchema.Name)
                                  + Environment.NewLine
                                  + queryBuilder.ToString();
            }

            return alterTableQuery;

            #endregion
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