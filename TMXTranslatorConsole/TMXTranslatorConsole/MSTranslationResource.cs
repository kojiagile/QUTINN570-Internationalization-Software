using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXTranslatorConsole
{
    enum EnumMSTransResponseTag
    {
        ArrayOfTranslateArrayResponse,
        TranslateArrayResponse,
        From,
        OriginalTextSentenceLengths,
        TranslatedText,
        TranslatedTextSentenceLengths,
    }

    /// <summary>
    /// Online Translation using Microsoft Translator web service.
    /// </summary>
    public class MSTranslationResource
    {
        /// <summary>
        /// time out value (millisecond)
        /// </summary>
        public static readonly int INT_TIMEOUT_MILLISEC = 60000;
        public static readonly string STR_FILE_NAME_CLIENT_INFO = "info/clientinfo.txt";

        public static string STR_HTTP_REQ_AUTH = @"grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}";

        public static readonly string STR_URI_TRANSLATOR_URI = @"http://api.microsofttranslator.com/v2/Http.svc/TranslateArray";
        public static readonly string STR_URI_XML_NAMESPACE = @"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2";
        public static readonly string STR_URI_AUTH_SCOPE = @"http://api.microsofttranslator.com";
        public static readonly string STR_URI_SERIALIZATION_ARRAY = @"http://schemas.microsoft.com/2003/10/Serialization/Arrays";
        public static readonly string STR_URI_DATAMARKET = @"https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";


        /// <summary>
        /// Fixed header value. A space after the value is required, thus do not remove.
        /// </summary>
        public static readonly string STR_HTTP_HEADER_TAG_BEARER = "Bearer ";

        public static readonly string STR_BODY_ROOT_START = "<TranslateArrayRequest>";
        public static readonly string STR_BODY_ROOT_END = "</TranslateArrayRequest>";
        public static readonly string STR_BODY_APPID = "<AppId />";
        public static string STR_BODY_LANG_FROM = "<From>{0}</From>";
        public static readonly string STR_BODY_TEXT_START = "<Texts>";
        public static readonly string STR_BODY_TEXT_END = "</Texts>";
        public static string STR_BODY_TEXT_CONTENT =
                "<string xmlns=\"" + STR_URI_SERIALIZATION_ARRAY + "\">{0}</string>";
        public static string STR_BODY_LANG_TO = "<To>{0}</To>";

        public static readonly string STR_HTTP_REQ_CONTENT_TYPE_XML = "text/xml";
        public static readonly string STR_HTTP_REQ_CONTENT_TYPE_APP_URL_ENCODED = "application/x-www-form-urlencoded";
        public static readonly string STR_HTTP_METHOD_GET = "GET";
        public static readonly string STR_HTTP_METHOD_POST = "POST";
        public static readonly string STR_HTTP_HEADER_ELEM_AUTH = "Authorization";

        public static string STR_HTTP_ERROR_MSG = "Http Status Code = {0}, Error Message = {1}";


    }
}
