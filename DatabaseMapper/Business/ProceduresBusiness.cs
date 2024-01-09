using DatabaseMapper.Models;
using DatabaseMapper.Repositories;
using DatabaseMapper.Utils;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseMapper.Business
{
    public class ProceduresBusiness
    {
        public List<Procedure> proceduresMigrationScriptsImplementation(SqlConnection sqlConnection, List<Procedure> procedures, string rootFolder)
        {
            var proceduresPath = Path.Join(rootFolder, "procedures");
            var fileManager = new FileManager();

            if (procedures.Count == 0)
                fileManager.CreateDirectory(proceduresPath);

            var script = new StringBuilder();

            try
            {
                foreach (Procedure procedure in procedures)
                {
                    string[] contentArray = new MigrationsRepository().spHelpTextContent(sqlConnection, procedure.name);
                    
                    for (int i = 0; i < contentArray.Length; i++)
                    {
                        script.AppendLine(contentArray[i]);
                    }

                    fileManager.CreateTextFile(proceduresPath, $@"Create_Procedure_{procedure.name}_{DateTime.UtcNow:yyyy-MM-dd_HH_mm_ss_fff}.sql", script.ToString());
                    script.Clear();
                }
            } catch (SqlException ex) 
            {
                Console.Error.WriteLine(ex.Message);
            }

            return procedures;
        }

        public List<Procedure> createAndUpdateProceduresMigrations(SqlConnection sqlConnection, string rootFolder)
        {
            string proceduresPath = Path.Join(rootFolder, "procedures");

            var file = new FileManager();

            string firstLine;

            var procedures = new List<Procedure>();
            var notCreatedProcedures = new List<Procedure>();

            var allProcedures = new MigrationsRepository().getAllProceduresNamesAndModifyDates(sqlConnection);

            if (!Directory.GetDirectories(rootFolder).Contains(proceduresPath))
            {
                procedures = proceduresMigrationScriptsImplementation(sqlConnection, procedures, rootFolder);
            }
            else if (Directory.GetFiles(proceduresPath).Length == 0)
            {
                notCreatedProcedures = proceduresMigrationScriptsImplementation(sqlConnection, procedures, rootFolder);
            }
            else
            {
                var updatableProcedureNames = new List<string>();
                try
                {
                    foreach (Procedure procedure in allProcedures)
                    {
                        foreach (string filePath in Directory.GetFiles(proceduresPath))
                        {
                            if (filePath.Contains($@"Create_Procedure_{procedure.name}"))
                            {
                                firstLine = File.ReadLines(Path.Combine(filePath)).First();
                                if (!firstLine.Contains(procedure.modify_date.ToString()))
                                {
                                    procedures.Add(procedure);
                                    file.DeleteFile(filePath);
                                }
                                updatableProcedureNames.Add(procedure.name);
                            }
                        }
                    }

                    if (updatableProcedureNames.Count != allProcedures.Count)
                    {
                        allProcedures.ForEach(p =>
                        {
                            if (!updatableProcedureNames.Contains(p.name))
                            {
                                notCreatedProcedures.Add(p);
                            }

                        });
                    }

                    if (procedures.Count > 0)
                    {
                        procedures = proceduresMigrationScriptsImplementation(sqlConnection, procedures, rootFolder);
                    }
                    if (notCreatedProcedures.Count > 0)
                    {
                        procedures = proceduresMigrationScriptsImplementation(sqlConnection, notCreatedProcedures, rootFolder);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message + "Deu erro");
                }
            }

            return procedures;
        }
    }
}
