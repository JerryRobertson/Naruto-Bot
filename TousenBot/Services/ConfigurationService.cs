using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TousenBot.Services
{
    public class ConfigurationService
    {
        public ConfigJson GetConfiguration()
        {
            var json = File.ReadAllText("config.json");
            return JsonConvert.DeserializeObject<ConfigJson>(json);

        }
    }
}
