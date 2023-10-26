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

        public Table() { }
        public Table(String tableName, List<Column> columns)
        {
            this.tableName = tableName;
            this.columns = columns;
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
