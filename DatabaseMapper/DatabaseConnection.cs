using System.Data.SqlClient;

namespace DatabaseMapper
{
    public class DatabaseConnection
    {
        private static string CONNECTION_STRING = "Data Source=.;Initial Catalog=banco_lanchonete;Persist Security Info=False;User ID=comcorp;Password=comcorp;Connection Timeout=300;";

        public SqlConnection startConnection()
        {
            return new SqlConnection(CONNECTION_STRING);
        }
    }
}
