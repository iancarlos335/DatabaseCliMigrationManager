using Dapper;
using DatabaseMapper.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMapper.Business
{
    public class MigrationsCreation
    {
        public List<Tables> getSysTables(SqlConnection sqlConnection)
        {
            try
            {
                sqlConnection.Open();
                string sqlGetAllTableNames = "SELECT NAME FROM SYS.TABLES";

                List<String> tableNames = sqlConnection.Query<String>(sqlGetAllTableNames).ToList();

                string sqlGetTableDescriptions = ""
                List<Column> tables = sqlConnection.Query<Column>(sqlGetAllTableNames).ToList();

                tables.ForEach(t =>
                {
                    string columnTables = $@"SELECT COLUMN_NAME as name,DATA_TYPE as type, CHARACTER_MAXIMUM_LENGTH as length, IS_NULLABLE as nullable, 
                                            (SELECT 1 FROM SYS.COLUMNS WHERE NAME = COLUMN_NAME AND IS_IDENTITY = 1) as is_identity 
                                            FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{{t.TableName}}'";
                });
                
                sqlConnection.Close();
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro de conexão com o banco.");
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
