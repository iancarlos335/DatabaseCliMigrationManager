using DatabaseMapper.Models;
using DatabaseMapper.Repositories;
using DatabaseMapper.Utils;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseMapper.Business
{
    public class ViewBusiness
    {
        public List<View> viewsMigrationScriptsImplementation(SqlConnection sqlConnection, List<View> views, string rootFolder)
        {
            var viewsPath = Path.Join(rootFolder, "views");
            var fileManager = new FileManager();

            if (views.Count == 0)
                fileManager.CreateDirectory(viewsPath);

            var script = new StringBuilder();

            try
            {
                foreach (View view in views)
                {
                    string[] contentArray = new MigrationsRepository().spHelpTextContent(sqlConnection, view.name);

                    for (int i = 0; i < contentArray.Length; i++)
                    {
                        script.AppendLine(contentArray[i]);
                    }

                    fileManager.CreateTextFile(viewsPath, $@"Create_View_{view.name}_{DateTime.UtcNow:yyyy-MM-dd_HH_mm_ss_fff}.sql", script.ToString());
                    script.Clear();
                }
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            return views;
        }

        public List<View> createAndUpdateViewsMigrations(SqlConnection sqlConnection, string rootFolder)
        {
            string viewsPath = Path.Join(rootFolder, "views");
            var file = new FileManager();

            string firstLine;

            var views = new List<View>();
            var notCreatedViews = new List<View>();

            var allViews = new MigrationsRepository().getAllViewsNamesAndModifyDates(sqlConnection);

            if (!Directory.GetDirectories(rootFolder).Contains(viewsPath))
                views = viewsMigrationScriptsImplementation(sqlConnection, views, rootFolder);
            else if (Directory.GetFiles(viewsPath).Length == 0)
                notCreatedViews = viewsMigrationScriptsImplementation(sqlConnection, views, rootFolder);
            else
            {
                var updatableViewNames = new List<string>();
                try
                {
                    foreach (View view in allViews)
                    {
                        foreach (string filePath in Directory.GetFiles(viewsPath))
                        {
                            if (filePath.Contains($@"Create_View_{view.name}"))
                            {
                                firstLine = File.ReadLines(Path.Combine(filePath)).First();
                                if (!firstLine.Contains(view.modify_date.ToString()))
                                {
                                    views.Add(view);
                                    file.DeleteFile(filePath);
                                }
                                updatableViewNames.Add(view.name);
                            }
                        }
                    }

                    if (updatableViewNames.Count != allViews.Count)
                    {
                        allViews.ForEach(v =>
                        {
                            if (!updatableViewNames.Contains(v.name))
                                notCreatedViews.Add(v);
                        });
                    }

                    if (views.Count > 0)
                        views = viewsMigrationScriptsImplementation(sqlConnection, views, rootFolder);

                    if (notCreatedViews.Count > 0)
                        views = viewsMigrationScriptsImplementation(sqlConnection, views, rootFolder);

                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message + "Deu erro");
                }
            }

            return views;
        }
    }
}
