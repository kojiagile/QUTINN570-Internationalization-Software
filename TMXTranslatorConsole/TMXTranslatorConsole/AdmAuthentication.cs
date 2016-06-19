using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Media;
using System.Runtime.Serialization.Json;

namespace TMXTranslatorConsole
{
    /// <summary>
    /// Authentication class for Microsoft Translator web service.
    /// This class gets access token.
    /// 
    /// See example code on the web page below:
    ///     http://msdn.microsoft.com/en-us/library/ff512421
    ///     
    /// </summary>
    class AdmAuthentication
    {
        private string clientId;
        private string cientSecret;
        private string request;

        public AdmAuthentication(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.cientSecret = clientSecret;
            //If clientid or client secret has special characters, encode before sending request
            this.request = string.Format(MSTranslationResource.STR_HTTP_REQ_AUTH,
                HttpUtility.UrlEncode(clientId),
                HttpUtility.UrlEncode(clientSecret),
                MSTranslationResource.STR_URI_AUTH_SCOPE);
        }

        public AdmAccessToken getAccessToken()
        {
            return httpPost(MSTranslationResource.STR_URI_DATAMARKET, this.request);
        }

        private AdmAccessToken httpPost(string DatamarketAccessUri, string requestDetails)
        {
            AdmAccessToken token = null;
            //Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
            webRequest.ContentType = MSTranslationResource.STR_HTTP_REQ_CONTENT_TYPE_APP_URL_ENCODED;
            webRequest.Method = MSTranslationResource.STR_HTTP_METHOD_POST;
            webRequest.Timeout = MSTranslationResource.INT_TIMEOUT_MILLISEC;
            byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
            webRequest.ContentLength = bytes.Length;
            using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                //Get deserialized object from JSON stream
                token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());

            }

            return token;
        }


    }
}
