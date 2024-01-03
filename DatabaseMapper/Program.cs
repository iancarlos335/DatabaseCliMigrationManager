using DatabaseMapper.Business;
using DatabaseMapper;
using System.Data.SqlClient;
using DatabaseMapper.Models;
// See https://aka.ms/new-console-template for more information

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Hello, World!");
            SqlConnection connection = new DatabaseConnection().startConnection();
            connection.Open();
            MigrationsCreation migrations = new MigrationsCreation();

            List<Table> updatedTables = migrations.createAndUpdateMigrations(connection);

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

