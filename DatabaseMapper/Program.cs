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
            var fileManager = new FileManager();
            var connection = new DatabaseConnection().startConnection();

            Console.WriteLine("Hello, World!");
            connection.Open();

            var sourcePath = Directory.GetCurrentDirectory();
            var rootFolder = Path.Join(sourcePath, "scripts");

            if (!Directory.GetDirectories(sourcePath).Contains("scripts"))
                fileManager.CreateDirectory(rootFolder);

            List<Table> updatedTables = new TableBusiness().createAndUpdateTableMigrations(connection, rootFolder);
            List<Procedure> updatedProcedures = new ProcedureBusiness().createAndUpdateProceduresMigrations(connection, rootFolder);
            List<Function> updatedFunctions = new FunctionBusiness().createAndUpdateFunctionsMigrations(connection, rootFolder);
            List<Trigger> updatedTriggers = new TriggerBusiness().createAndUpdateTriggersMigrations(connection, rootFolder);
            List<View> updatedViews = new ViewBusiness().createAndUpdateViewsMigrations(connection, rootFolder);
            List<Synonym> updatedSynonyms = new SynonymBusiness().createAndUpdateSynonymsMigrations(connection, rootFolder);

            Console.WriteLine("Tabelas alteradas:");
            foreach (var table in updatedTables)
                Console.WriteLine(table.tableName);

            Console.WriteLine("Procedures alteradas:");
            foreach (var procedure in updatedFunctions)
                Console.WriteLine(procedure.name);

            Console.WriteLine("Funções alteradas:");
            foreach (var function in updatedFunctions)
                Console.WriteLine(function.name);

            Console.WriteLine("Triggers alterados:");
            foreach (var trigger in updatedTriggers)
                Console.WriteLine(trigger.name);

            Console.WriteLine("Views alteradas:");
            foreach (var view in updatedViews)
                Console.WriteLine(view.name);

            connection.Close();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
        }
    }
}

