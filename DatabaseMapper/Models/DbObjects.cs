using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMapper.Models
{
    public class Table
    {
        public String tableName { get; set; }

        public List<Column> columns { get; set; }

        public DateTime modify_date { get; set; }

        public Table() { }
        public Table(String tableName, List<Column> columns, DateTime modify_date)
        {
            this.tableName = tableName;
            this.columns = columns;
            this.modify_date = modify_date;
        }
    }

    public class Column
    {
        public String name { get; set; }
        public String type { get; set; }
        public int length { get; set; }
        public String nullable { get; set; }
        public int is_identity { get; set; }
        public String is_clustered {  get; set; }
    }

}
