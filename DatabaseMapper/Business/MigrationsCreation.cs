using Dapper;
using DatabaseMapper.Models;
using DatabaseMapper.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DatabaseMapper.Business
{
    public class MigrationsCreation
    {
        public List<Table> getSysTables(SqlConnection sqlConnection)
        {
            string sqlGetAllTableNames = "SELECT NAME as tableName FROM SYS.TABLES";
            List<String> tablesName = sqlConnection.Query<String>(sqlGetAllTableNames).ToList();

            List<Table> tables = new List<Table>();

            tablesName.ForEach(tableName =>
            {
                string sqlGetTableColumns = "SELECT isc.COLUMN_NAME as name, isc.DATA_TYPE as type, isc.CHARACTER_MAXIMUM_LENGTH as length," +
                    " isc.IS_NULLABLE as nullable, sc.is_identity, si.type_desc as is_clustered" +
                    " from SYS.TABLES st" +
                    " INNER JOIN INFORMATION_SCHEMA.COLUMNS isc ON st.NAME = isc.TABLE_NAME" +
                    " INNER JOIN SYS.COLUMNS sc ON st.OBJECT_ID = sc.OBJECT_ID" +
                    " INNER JOIN SYS.INDEXES si ON st.OBJECT_ID = si.OBJECT_ID" +
                    $@" WHERE isc.TABLE_NAME = '{tableName}' and sc.NAME = ISC.COLUMN_NAME";

                List<Column> tablesColumns = sqlConnection.Query<Column>(sqlGetTableColumns).ToList();
                DateTime modifiedDate = sqlConnection.Query<DateTime>($@"SELECT MODIFY_DATE FROM SYS.TABLES WHERE NAME LIKE '{tableName}'").FirstOrDefault();

                tables.Add(new Table(tableName, tablesColumns, modifiedDate));
            });

            return tables;
        }


        public void createTablesMigrationScripts(List<Table> tables)
        {
            String rootFolder = Directory.GetCurrentDirectory();
            String tablesPath = Path.Combine(rootFolder, "tables");
            String finalFolderPath = Path.Join(tablesPath);

            FileManager file = new FileManager();
            file.CreateDirectory(tablesPath);

            StringBuilder script = new StringBuilder();

            try
            {
                foreach (Table table in tables)
                {
                    foreach (Column column in table.columns)
                    {
                        script.AppendLine($@"--//// Modified at {table.modify_date}////--");
                        if (column.is_identity.Equals(0))
                        {
                            string nullable = column.nullable.Equals("YES") ? "NULLABLE" : "";

                            script.AppendLine($@"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{table.tableName}' AND COLUMN_NAME = '{column.name}')");
                            script.AppendLine("BEGIN");
                            script.AppendLine($@"ALTER TABLE {table.tableName} ");
                            script.AppendLine($@"ADD {column.name} {column.type}({column.length}) {nullable}");
                            script.AppendLine("END");
                        }
                        else
                        {
                            string nullable = column.nullable.Equals("YES") ? "NULLABLE" : "";
                            string is_clustered = column.is_clustered.Equals("CLUSTERED") ? "CLUSTERED" : "";

                            script.AppendLine($@"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{table.tableName}' AND COLUMN_NAME = '{column.name}')");
                            script.AppendLine($@"BEGIN");
                            script.AppendLine($@"ALTER TABLE {table.tableName} ADD {column.name} {column.type} IDENTITY(1,1) {nullable}");
                            script.AppendLine($@"ALTER TABLE {table.tableName}");
                            script.AppendLine($@"ADD CONSTRAINT PK_{table.tableName}_{column.name} PRIMARY KEY {is_clustered} ({column.name})");
                            script.AppendLine($@"END");
                        }
                        script.AppendLine();
                    }

                    file.CreateTextFile(finalFolderPath, $@"Create_Table_{table.tableName}_{file.JavascriptGetTime()}.sql", script.ToString());
                    script.Clear();
                }
            } catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }            
        }

        public void updateTablesMigrationScripts(List<Table> tables)
        {

            String rootFolder = Directory.GetCurrentDirectory();
            String tablesPath = Path.Combine(rootFolder, "tables");
            String finalFolderPath = Path.Join(tablesPath);

            FileManager file = new FileManager();
            file.CreateDirectory(tablesPath);

            StringBuilder script = new StringBuilder();

            String firstLine;
            try
            {
                foreach (Table table in tables)
                {
                    foreach (string filePath in Directory.GetFiles(finalFolderPath))
                    {
                        if (filePath.Contains($@"Create_Table_{table.tableName}"))
                        {
                            firstLine = File.ReadLines(Path.Combine(filePath)).First();
                            if (firstLine.Contains(table.modify_date.ToString()))
                            {
                                foreach (Column column in table.columns)
                                {
                                    script.AppendLine($@"--//// Modified at {table.modify_date}////--");                                    
                                    if (column.is_identity.Equals(0))
                                    {
                                        string nullable = column.nullable.Equals("YES") ? "NULLABLE" : "";

                                        script.AppendLine($@"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{table.tableName}' AND COLUMN_NAME = '{column.name}')");
                                        script.AppendLine("BEGIN");
                                        script.AppendLine($@"ALTER TABLE {table.tableName} ");
                                        script.AppendLine($@"ADD {column.name} {column.type}({column.length}) {nullable}");
                                        script.AppendLine("END");
                                    }
                                    else
                                    {
                                        string nullable = column.nullable.Equals("YES") ? "NULLABLE" : "";
                                        string is_clustered = column.is_clustered.Equals("CLUSTERED") ? "CLUSTERED" : "";

                                        script.AppendLine($@"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{table.tableName}' AND COLUMN_NAME = '{column.name}')");
                                        script.AppendLine($@"BEGIN");
                                        script.AppendLine($@"ALTER TABLE {table.tableName} ADD {column.name} {column.type} IDENTITY(1,1) {nullable}");
                                        script.AppendLine($@"ALTER TABLE {table.tableName}");
                                        script.AppendLine($@"ADD CONSTRAINT PK_{table.tableName}_{column.name} PRIMARY KEY {is_clustered} ({column.name})");
                                        script.AppendLine($@"END");
                                    }
                                    script.AppendLine();
                                }

                                file.CreateTextFile(finalFolderPath, $@"Create_Table_{table.tableName}_{file.JavascriptGetTime()}.sql", script.ToString());
                                script.Clear();
                            }
                            else
                                continue;
                        }
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.Error.WriteLine(excpt.Message);
            };
        }
    }
}
