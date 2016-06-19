using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TMXTranslatorConsole
{
    /// <summary>
    /// Access Token data class for Microsoft Translator web service.
    /// 
    /// <see cref="http://msdn.microsoft.com/en-us/library/hh454950.aspx"/>
    /// </summary>
    [DataContract]
    class AdmAccessToken
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string scope { get; set; }
    }
}
