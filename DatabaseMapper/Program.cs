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
            SqlConnection connection = new DatabaseConnection().startConnection();
            connection.Open();

            Console.WriteLine("Hello, World!");
            List<Table> tables = new MigrationsCreation().getSysTables(connection);

            new MigrationsCreation().createTablesMigrationScripts(tables);

            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro de conexão com o banco.");
            Console.WriteLine(ex.Message);
        }
    }
}

