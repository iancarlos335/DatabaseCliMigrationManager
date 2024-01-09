using DatabaseMapper.Models;
using DatabaseMapper.Repositories;
using DatabaseMapper.Utils;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseMapper.Business
{
    public class TablesBusiness
    {
        public List<Table> tablesMigrationScriptsImplementation(SqlConnection sqlConnection, List<Table> tables, string rootFolder)
        {
            var tablesPath = Path.Join(rootFolder, "tables");
            var fileManager = new FileManager();

            if (tables.Count == 0)
                fileManager.CreateDirectory(tablesPath);

            tables = new MigrationsRepository().getSysTablesToCreateMigrations(sqlConnection, tables);

            var script = new StringBuilder();

            try
            {
                foreach (Table table in tables)
                {
                    script.AppendLine($@"--//// Modified at {table.modify_date}////--");

                    foreach (Column column in table.columns)
                    {
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

                    fileManager.CreateTextFile(tablesPath, $@"Create_Table_{table.tableName}_{DateTime.UtcNow:yyyy-MM-dd_HH_mm_ss_fff}.sql", script.ToString());
                    script.Clear();
                }                
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.Message);                
            }
            return tables;
        }

        public List<Table> createAndUpdateTableMigrations(SqlConnection sqlConnection, string rootFolder)
        {
            string tablesPath = Path.Join(rootFolder, "tables");

            var fileManager = new FileManager();

            string firstLine;

            var tables = new List<Table>();
            var notCreatedTables = new List<Table>();
            var allTables = new MigrationsRepository().getAllTableNamesAndModifyDates(sqlConnection);

            if (!Directory.GetDirectories(rootFolder).Contains(tablesPath))
            {
                tables = tablesMigrationScriptsImplementation(sqlConnection, tables, rootFolder);
            }
            else if (Directory.GetFiles(tablesPath).Length == 0)
            {
                notCreatedTables = tablesMigrationScriptsImplementation(sqlConnection, tables, rootFolder);
            }
            else
            {
                var updatableTableNames = new List<string>();
                try
                {
                    foreach (Table table in allTables)
                    {
                        foreach (string filePath in Directory.GetFiles(tablesPath))
                        {
                            if (filePath.Contains($@"Create_Table_{table.tableName}"))
                            {
                                firstLine = File.ReadLines(Path.Combine(filePath)).First();
                                if (!firstLine.Contains(table.modify_date.ToString()))
                                {
                                    tables.Add(table);
                                    fileManager.DeleteFile(filePath);
                                }
                                updatableTableNames.Add(table.tableName);
                            }
                        }
                    }

                    if (updatableTableNames.Count != allTables.Count)
                    {
                        allTables.ForEach(t =>
                        {
                            if (!updatableTableNames.Contains(t.tableName))
                            {
                                notCreatedTables.Add(t);
                            }

                        });
                    }

                    if (tables.Count > 0)
                    {
                        tables = tablesMigrationScriptsImplementation(sqlConnection, tables, rootFolder);
                    }
                    if (notCreatedTables.Count > 0)
                    {
                        tables = tablesMigrationScriptsImplementation(sqlConnection, notCreatedTables, rootFolder);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message + "Deu erro");
                }
            }
            return tables;
        }
    }
}
