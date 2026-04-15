using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalloriesBot
{
    static public class DBModels
    {
        public class User
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public long TgId{ get; set; }
        }
    }
}
