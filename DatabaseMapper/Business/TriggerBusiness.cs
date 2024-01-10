using DatabaseMapper.Models;
using DatabaseMapper.Repositories;
using DatabaseMapper.Utils;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseMapper.Business
{
    public class TriggerBusiness
    {
        public List<Trigger> triggersMigrationScriptsImplementation(SqlConnection sqlConnection, List<Trigger> triggers, string rootFolder)
        {
            var triggersPath = Path.Join(rootFolder, "triggers");
            var fileManager = new FileManager();

            if (triggers.Count == 0)
                fileManager.CreateDirectory(triggersPath);

            var script = new StringBuilder();

            try
            {
                foreach (Trigger trigger in triggers)
                {
                    string[] contentArray = new MigrationsRepository().spHelpTextContent(sqlConnection, trigger.name);

                    for (int i = 0; i < contentArray.Length; i++)
                    {
                        script.AppendLine(contentArray[i]);
                    }

                    fileManager.CreateTextFile(triggersPath, $@"Create_Procedure_{trigger.name}_{DateTime.UtcNow:yyyy-MM-dd_HH_mm_ss_fff}.sql", script.ToString());
                    script.Clear();
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return triggers;
        }

        public List<Trigger> createAndUpdateTriggersMigrations(SqlConnection sqlConnection, string rootFolder)
        {
            string triggersPath = Path.Join(rootFolder, "triggers");
            var file = new FileManager();

            string firstLine;

            var triggers = new List<Trigger>();
            var notCreatedTriggers = new List<Trigger>();

            var allTriggers = new MigrationsRepository().getAllTriggersNamesAndModifyDates(sqlConnection);

            if (!Directory.GetDirectories(rootFolder).Contains(triggersPath))
                triggers = triggersMigrationScriptsImplementation(sqlConnection, triggers, rootFolder);
            else if (Directory.GetFiles(triggersPath).Length == 0)
                notCreatedTriggers = triggersMigrationScriptsImplementation(sqlConnection, triggers, rootFolder);
            else
            {
                var updatableTriggerNames = new List<string>();

                try
                {
                    foreach (Trigger trigger in allTriggers)
                    {
                        foreach (string filePath in Directory.GetFiles(triggersPath))
                        {
                            if (filePath.Contains($@"Create_Trigger_{trigger.name}"))
                            {
                                firstLine = File.ReadLines(Path.Combine(filePath)).First();
                                if (!firstLine.Contains(trigger.modify_date.ToString()))
                                {
                                    triggers.Add(trigger);
                                    file.DeleteFile(filePath);
                                }
                                updatableTriggerNames.Add(trigger.name);
                            }
                        }
                    }

                    if (updatableTriggerNames.Count != allTriggers.Count)
                    {
                        allTriggers.ForEach(t =>
                        {
                            if (!updatableTriggerNames.Contains(t.name))
                                notCreatedTriggers.Add(t);
                        });
                    }

                    if (triggers.Count > 0)
                        triggers = triggersMigrationScriptsImplementation(sqlConnection, triggers, rootFolder);
                    if (notCreatedTriggers.Count > 0)
                        triggers = triggersMigrationScriptsImplementation(sqlConnection, notCreatedTriggers, rootFolder);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message + "Deu erro");
                }
            }

            return triggers;
        }
    }
}
