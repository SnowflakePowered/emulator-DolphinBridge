using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace templatemaker
{
    internal class DolphinConfig
    {
        public string[] inputLines;
        public StringBuilder templateBuilder = new StringBuilder();
        public IDictionary<string, SnowflakeConfigKey> keys = new Dictionary<string, SnowflakeConfigKey>();


        public DolphinConfig(string[] input)
        {
            this.inputLines = input;
            foreach (string line in this.inputLines)
            {
                if (!line.StartsWith("["))
                {
                    this.ProcessLine(line);
                }
                else
                {
                    this.templateBuilder.AppendLine(line);
                }
            }
            File.WriteAllText("Template.template", this.templateBuilder.ToString());
            File.WriteAllText("configurations.keys.json", JsonConvert.SerializeObject(this.keys));
        }

        public void ProcessLine(string line)
        {
            var configLine = line.Replace(" ", "").Split('=');
            this.templateBuilder.AppendLine($"{configLine[0]} = {{{configLine[0]}}}");
            
            dynamic defaultValue;
            if (configLine[0].Contains('.'))
            {
                double d_value;
                if (Double.TryParse(configLine[1], out d_value))
                {
                    defaultValue = d_value;
                    this.keys.Add(configLine[0], new SnowflakeConfigKey("description", defaultValue));
                    return;
                }
            }
            int i_value;
            if (Int32.TryParse(configLine[1], out i_value))
            {
                defaultValue = i_value;
                this.keys.Add(configLine[0], new SnowflakeConfigKey("description", defaultValue));
                return;
            }
            bool b_value;
            if (Boolean.TryParse(configLine[1], out b_value))
            {
                defaultValue = b_value;
                this.keys.Add(configLine[0], new SnowflakeConfigKey("description", defaultValue));
                return;
            }
            
            this.keys.Add(configLine[0], new SnowflakeConfigKey("description", configLine[1]));
        }
    }
}
