using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_DiscordRPC
{
    public class AppConfig
    {
        [JsonProperty("rpcThreadUpdateInterval")]
        public int rpcThreadUpdateInt { get; private set; }

        [JsonProperty("kbThreadDetectInterval")]
        public int kbThreadUpdateInt { get; private set; }

        [JsonProperty("startHidden")]
        public bool isHidden { get; private set; }

        public AppConfig(int rpcInt, int kbInt, bool hidden)
        {
            rpcThreadUpdateInt = rpcInt;
            kbThreadUpdateInt = kbInt;
            isHidden = hidden;
        }
    }
}
