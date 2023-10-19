﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMapper.Models
{
    public class Tables
    {
        public string TableName { get; set; }

        public List<Column> Columns { get; set; }

    }

    public class Column
    {
        public string name { get; set; }
        public string type { get; set; }
        public string length { get; set; }
        public string nullable { get; set; }
        public int is_identity { get; set; }
    }

}
