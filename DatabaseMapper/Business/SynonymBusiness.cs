using DatabaseMapper.Models;
using DatabaseMapper.Repositories;
using DatabaseMapper.Utils;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseMapper.Business
{
    public class SynonymBusiness
    {
        public List<Synonym> synonymsMigrationScriptsImplementation(SqlConnection sqlConnection, List<Synonym> synonyms, string rootFolder)
        {
            var synonymsPath = Path.Join(rootFolder, "synonyms");
            var fileManager = new FileManager();

            if (synonyms.Count == 0)
                fileManager.CreateDirectory(synonymsPath);

            var script = new StringBuilder();

            try
            {
                foreach (Synonym synonym in synonyms)
                {
                    string[] contentArray = new MigrationsRepository().spHelpTextContent(sqlConnection, synonym.name);

                    for (int i = 0; i < contentArray.Length; i++)
                    {
                        script.AppendLine(contentArray[i]);
                    }

                    fileManager.CreateTextFile(synonymsPath, $@"Create_Synonym_{synonym.name}_{DateTime.UtcNow:yyyy-MM-dd_HH_mm_ss_fff}.sql", script.ToString());
                    script.Clear();
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            return synonyms;
        }

        public List<Synonym> createAndUpdateSynonymsMigrations(SqlConnection sqlConnection, string rootFolder)
        {
            string synonymsPath = Path.Join(rootFolder, "synonyms");
            var file = new FileManager();

            string firstLine;

            var synonyms = new List<Synonym>();
            var notCreatedSynonyms = new List<Synonym>();

            var allSynonyms = new MigrationsRepository().getAllSynonymsNamesAndModifyDates(sqlConnection);

            if (!Directory.GetDirectories(rootFolder).Contains(synonymsPath))
                synonyms = synonymsMigrationScriptsImplementation(sqlConnection, synonyms, rootFolder);
            else if (Directory.GetFiles(synonymsPath).Length == 0)
                notCreatedSynonyms = synonymsMigrationScriptsImplementation(sqlConnection, synonyms, rootFolder);
            else
            {
                var updatableSynonymNames = new List<string>();
                try
                {
                    foreach (Synonym synonym in allSynonyms)
                    {
                        foreach (string filePath in Directory.GetFiles(synonymsPath))
                        {
                            if (filePath.Contains($@"Create_Synonym_{synonym.name}"))
                            {
                                firstLine = File.ReadLines(Path.Combine(filePath)).First();
                                if (!firstLine.Contains(synonym.modify_date.ToString()))
                                {
                                    synonyms.Add(synonym);
                                    file.DeleteFile(filePath);
                                }
                                updatableSynonymNames.Add(synonym.name);
                            }
                        }
                    }

                    if (updatableSynonymNames.Count != allSynonyms.Count)
                    {
                        allSynonyms.ForEach(s =>
                        {
                            if (!updatableSynonymNames.Contains(s.name))
                                notCreatedSynonyms.Add(s);
                        });
                    }

                    if (synonyms.Count > 0)
                        synonyms = synonymsMigrationScriptsImplementation(sqlConnection, synonyms, rootFolder);

                    if (notCreatedSynonyms.Count > 0)
                        synonyms = synonymsMigrationScriptsImplementation(sqlConnection, synonyms, rootFolder);

                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message + "Deu erro");
                }
            }

            return synonyms;
        }
    }
}
