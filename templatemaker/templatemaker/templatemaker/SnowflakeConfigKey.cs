using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace templatemaker
{
    public class SnowflakeConfigKey
    {
        public string description { get; set; }
        public dynamic defaultValue { get; set; }

        public SnowflakeConfigKey(string description, dynamic defaultValue)
        {
            this.description = description;
            this.defaultValue = defaultValue;
        }
    }
}
