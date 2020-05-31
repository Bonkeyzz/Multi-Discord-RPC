using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_DiscordRPC
{
    public class dRPCApplication
    {
        [JsonProperty("state")]
        /// <summary>
        /// Bottom Text
        /// </summary>
        public string sState;
        [JsonProperty("details")]
        /// <summary>
        /// Top Text
        /// </summary>
        public string sDetails;

        [JsonProperty("large_img_key")]
        public string sLargeImgKey;

        [JsonProperty("small_img_key")]
        public string sSmallImgKey;

        [JsonProperty("large_img_text")]
        public string sLargeImgText;

        [JsonProperty("small_img_text")]
        public string sSmallImgText;

        [JsonProperty("proc_name")]
        public string sProcessName; /* No Extension */

        [JsonProperty("app_id")]
        public string sAppId;

        [JsonProperty("app_name")]
        public string sAppName;
        /// <summary>
        /// Class for storing defined discord Apps/RPC Data (JSON Friendly)
        /// </summary>
        /// <param name="mState">Bottom Text of RPresence</param>
        /// <param name="mDetails">Top Text of RPresence</param>
        /// <param name="mLargeImgKey">Primary image name</param>
        /// <param name="mSmallImgKey">Secondary image name</param>
        /// <param name="mLargeImgText">Text when hovering on primary image</param>
        /// <param name="mSmallImgText">Text when hovering on secondary image</param>
        /// <param name="mProcessName">Process name to look for (No Extension)</param>
        /// <param name="mAppId">Application ID</param>
        public dRPCApplication(string mDetails = null, string mState = null, string mLargeImgKey = null,
            string mSmallImgKey = null, string mLargeImgText = null, string mSmallImgText = null, string mProcessName = null, string mAppId = null, string mAppName = null)
        {
            sState = mState;
            sDetails = mDetails;
            sLargeImgKey = mLargeImgKey;
            sSmallImgKey = mSmallImgKey;
            sLargeImgText = mLargeImgText;
            sSmallImgText = mSmallImgText;

            sProcessName = mProcessName;
            sAppId = mAppId;

            sAppName = mAppName;
        }
    }
}
