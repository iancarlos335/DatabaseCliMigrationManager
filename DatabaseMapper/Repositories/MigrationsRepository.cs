using Dapper;
using DatabaseMapper.Models;
using System.Data.SqlClient;

namespace DatabaseMapper.Repositories
{
    public class MigrationsRepository
    {
        public List<Table> getSysTablesToCreateMigrations(SqlConnection sqlConnection, List<Table> tables)
        {
            string sqlGetAllTableNames = "";
            var tablesName = new List<string>();
            var returnTables = new List<Table>();

            if (tables.Count == 0)
            {
                sqlGetAllTableNames = "SELECT NAME as tableName FROM SYS.TABLES";
                tablesName = sqlConnection.Query<string>(sqlGetAllTableNames).ToList();
            }
            else
                tables.ForEach(t => { tablesName.Add(t.tableName); });



            tablesName.ForEach(tableName =>
            {
                string sqlGetTableColumns = "SELECT isc.COLUMN_NAME as name, isc.DATA_TYPE as type, isc.CHARACTER_MAXIMUM_LENGTH as length," +
                    " isc.IS_NULLABLE as nullable, sc.is_identity, si.type_desc as is_clustered" +
                    " from SYS.TABLES st" +
                    " INNER JOIN INFORMATION_SCHEMA.COLUMNS isc ON st.NAME = isc.TABLE_NAME" +
                    " INNER JOIN SYS.COLUMNS sc ON st.OBJECT_ID = sc.OBJECT_ID" +
                    " INNER JOIN SYS.INDEXES si ON st.OBJECT_ID = si.OBJECT_ID" +
                    $@" WHERE isc.TABLE_NAME = '{tableName}' and sc.NAME = ISC.COLUMN_NAME";

                List<Column> tablesColumns = sqlConnection.Query<Column>(sqlGetTableColumns).ToList(); //TODO update that query to be more performable and fast.
                DateTime modifiedDate = sqlConnection.Query<DateTime>($@"SELECT MODIFY_DATE FROM SYS.TABLES WHERE NAME LIKE '{tableName}'").FirstOrDefault();

                returnTables.Add(new Table(tableName, tablesColumns, modifiedDate));
            });

            return returnTables;
        }

        public List<Table> getAllTableNamesAndModifyDates(SqlConnection sqlConnection)
        {
            return sqlConnection.Query<Table>("SELECT NAME as tableName, modify_date FROM SYS.TABLES").ToList();
        }

        public List<Procedure> getAllProceduresNamesAndModifyDates(SqlConnection sqlConnection)
        {
            return sqlConnection.Query<Procedure>("SELECT NAME, modify_date FROM SYS.PROCEDURES").ToList();
        }

        public List<Function> getAllFunctionsNamesAndModifyDates(SqlConnection sqlConnection)
        {
            return sqlConnection.Query<Function>("SELECT name, modify_date FROM SYS.OBJECTS WHERE TYPE IN ('FN', 'FS', 'FT', 'AF', 'IF', 'TF')").ToList();
        }

        public List<Trigger> getAllTriggersNamesAndModifyDates(SqlConnection sqlConnection)
        {
            return sqlConnection.Query<Trigger>("SELECT name, modify_date FROM SYS.TRIGGERS").ToList();
        }

        public List<View> getAllViewsNamesAndModifyDates(SqlConnection sqlConnection)
        {
            return sqlConnection.Query<View>("SELECT name, modify_date FROM SYS.VIEWS").ToList();
        }

        public List<Synomn> getAllSynomnsNamesAndModifyDates(SqlConnection sqlConnection)
        {
            return sqlConnection.Query<Synomn>("SELECT name, modify_date FROM SYS.SYNONYMS").ToList();
        }

        public string[] spHelpTextContent(SqlConnection sqlConnection, string databaseObject)
        {
            return sqlConnection.Query<string>($@"sp_helptext {databaseObject}").ToArray();
        }
    }
}
