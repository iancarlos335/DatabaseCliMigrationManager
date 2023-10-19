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
        Console.WriteLine("Hello, World!");
        SqlConnection connection = new DatabaseConnection().startConnection();

        List<Tables> tables = new MigrationsCreation().getSysTables(connection);

        tables.ForEach(t => Console.WriteLine(t.ToString()));
    }
}

