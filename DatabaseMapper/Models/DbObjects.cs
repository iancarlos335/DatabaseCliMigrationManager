namespace DatabaseMapper.Models
{
    public class Table
    {
        public string tableName { get; set; }

        public List<Column> columns { get; set; }

        public DateTime modify_date { get; set; }

        public Table(string tableName, List<Column> columns, DateTime modify_date)
        {
            this.tableName = tableName;
            this.columns = columns;
            this.modify_date = modify_date;
        }
    }

    public class Column
    {
        public string name { get; set; }
        public string type { get; set; }
        public int length { get; set; }
        public string nullable { get; set; }
        public int is_identity { get; set; }
        public string is_clustered { get; set; }
    }

    public class Procedure
    {
        public string name { get; set; }
        public string modify_date { get; set; }
        public string content { get; set; }
    }
    ^public class Function
    {
        public string name { get; set; }
        public string modify_date { get; set; }
        public string content { get; set; }
    }

    public class Trigger
    {
        public string name { get; set; }
        public string modify_date { get; set; }
        public string content { get; set; }
    }

    public class View
    {
        public  string name { get; set; }
        public string modify_date { get; set; }
        public string content { get; set; }
    }

    public class Synomn
    {
        public string name { get; set; }
        public string modify_date { get; set;}
        public string content { get; set;}
    }
}
