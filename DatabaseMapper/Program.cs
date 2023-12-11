using DatabaseMapper.Business;
using DatabaseMapper;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
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
            List<Table> tables = migrations.getSysTables(connection);

            migrations.createTablesMigrationScripts(tables);
            migrations.updateTablesMigrationScripts(tables);

            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro de conexão com o banco.");
            Console.WriteLine(ex.Message);
        }
    }
}

