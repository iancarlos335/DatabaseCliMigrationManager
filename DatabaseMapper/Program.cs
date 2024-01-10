using DatabaseMapper.Business;
using DatabaseMapper;
using DatabaseMapper.Models;
using DatabaseMapper.Utils;
// See https://aka.ms/new-console-template for more information

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var tableBusiness = new TableBusiness();
            var fileManager = new FileManager();
            var connection = new DatabaseConnection().startConnection();

            Console.WriteLine("Hello, World!");
            connection.Open();

            var sourcePath = Directory.GetCurrentDirectory();
            var rootFolder = Path.Join(sourcePath, "scripts");

            if (!Directory.GetDirectories(sourcePath).Contains("scripts"))
                fileManager.CreateDirectory(rootFolder);

            List<Table> updatedTables = tableBusiness.createAndUpdateTableMigrations(connection, rootFolder);

            Console.WriteLine("Tabelas alteradas:");
            foreach (var table in updatedTables)
            {
                Console.WriteLine(table.tableName);
            }

            connection.Close();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
        }
    }
}

