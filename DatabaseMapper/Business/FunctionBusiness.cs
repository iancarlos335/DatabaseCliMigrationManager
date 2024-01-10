using DatabaseMapper.Models;
using DatabaseMapper.Repositories;
using DatabaseMapper.Utils;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseMapper.Business
{
    public class FunctionBusiness
    {
        public List<Function> functionsMigrationScriptsImplementation(SqlConnection sqlConnection, List<Function> functions, string rootFolder)
        {
            var functionsPath = Path.Join(rootFolder, "functions");
            var fileManager = new FileManager();

            if (functions.Count == 0)
                fileManager.CreateDirectory(functionsPath);

            var script = new StringBuilder();

            try
            {
                foreach (Function function in functions)
                {
                    string[] contentArray = new MigrationsRepository().spHelpTextContent(sqlConnection, function.name);

                    for (int i = 0; i < contentArray.Length; i++)
                    {
                        script.AppendLine(contentArray[i]);
                    }

                    fileManager.CreateTextFile(functionsPath, $@"Create_Function_{function.name}_{DateTime.UtcNow:yyyy-MM-dd_HH_mm_ss_fff}.sql", script.ToString());
                    script.Clear();
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            return functions;
        }

        public List<Function> createAndUpdateFunctionsMigrations(SqlConnection sqlConnection, string rootFolder)
        {
            string functionsPath = Path.Join(rootFolder, "functions");
            var file = new FileManager();

            string firstLine;

            var functions = new List<Function>();
            var notCreatedFunctions = new List<Function>();

            var allFunctions = new MigrationsRepository().getAllFunctionsNamesAndModifyDates(sqlConnection);

            if (!Directory.GetDirectories(rootFolder).Contains(functionsPath))
                functions = functionsMigrationScriptsImplementation(sqlConnection, functions, rootFolder);
            else if (Directory.GetFiles(functionsPath).Length == 0)
                notCreatedFunctions = functionsMigrationScriptsImplementation(sqlConnection, functions, rootFolder);
            else
            {
                var updatableFunctionNames = new List<string>();
                try
                {
                    foreach (Function function in allFunctions)
                    {
                        foreach (string filePath in Directory.GetFiles(functionsPath))
                        {
                            if (filePath.Contains($@"Create_Function_{function.name}"))
                            {
                                firstLine = File.ReadLines(Path.Combine(filePath)).First();
                                if (!firstLine.Contains(function.modify_date.ToString()))
                                {
                                    functions.Add(function);
                                    file.DeleteFile(filePath);
                                }
                                updatableFunctionNames.Add(function.name);
                            }
                        }
                    }

                    if (updatableFunctionNames.Count != allFunctions.Count)
                    {
                        allFunctions.ForEach(f =>
                        {
                            if (!updatableFunctionNames.Contains(f.name))
                                notCreatedFunctions.Add(f);

                        });
                    }

                    if (functions.Count > 0)
                        functions = functionsMigrationScriptsImplementation(sqlConnection, functions, rootFolder);

                    if (notCreatedFunctions.Count > 0)
                        functions = functionsMigrationScriptsImplementation(sqlConnection, functions, rootFolder);

                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message + "Deu erro");
                }
            }

            return functions;
        }
    }
}
