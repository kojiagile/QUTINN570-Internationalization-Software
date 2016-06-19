using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Collections;

namespace TMXTranslatorConsole
{
    /// <summary>
    /// Online Translation using Microsoft Translator web service.
    /// 
    /// See more detail of TranslateArray method(the app uses this method):
    /// (Use the TranslateArray method to retrieve translations for multiple source texts.)
    ///     http://msdn.microsoft.com/en-us/library/ff512422
    /// 
    /// See more detail of Translate method:
    /// (Translates a text string from one language to another.)
    ///     http://msdn.microsoft.com/en-us/library/ff512421
    /// 
    /// </summary>
    class MSTranslator
    {
        private AdmAccessToken admToken;
        private AdmAuthentication admAuth;

        public MSTranslator()
        {
            #region Tips about Access Token
            ///TIPS:
            /// You must obtain an access token to use the Microsoft Translator API. 
            /// The access token is passed with each API call and is used to authenticate you access to the Microsoft Translator API. 
            /// It provides a secure access to the Microsoft Translator API and allows the API to associate your application’s requests 
            /// to the Microsoft Translator service with your account on Azure Marketplace.
            /// 
            /// Detail of Access Token:
            ///     http://msdn.microsoft.com/en-us/library/hh454950.aspx
            ///     
            /// To obtain these, you need to register your information on the Microsoft web site below:
            ///
            /// Subscription registration
            ///     https://datamarket.azure.com/dataset/1899a118-d202-492c-aa16-ba21c33c06cb
            ///         Choose the top item($0.00/month) on the right and register your information.
            ///         
            /// Register your own application
            ///     https://datamarket.azure.com/developer/applications/
            ///         create new application on the web page to get clientID and client secret.
            ///         (it does not matter whether or not if you have created the application.)
            ///
            #endregion
            //first arg is cliendID. second arg is client secret.(it is like password)
            string[] info = this.getClientInfo();
            admAuth = new AdmAuthentication(info[0], info[1]);
        }

        private string[] getClientInfo()
        {
            string[] ret = { string.Empty, string.Empty };
            try
            {
                using (StreamReader sr = new StreamReader(MSTranslationResource.STR_FILE_NAME_CLIENT_INFO))
                {
                    ret[0] = sr.ReadLine();
                    ret[1] = sr.ReadLine();
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.StackTrace);
                throw e;
            }

            return ret;
        }

        public string[] translate(string[] testArray, string sourceLang, string targetLang)
        {
            if (testArray == null || testArray.Length == 0)
            {
                throw new ApplicationException("Online Translatoin Error. Source text is null or empty.");
            }
            if (string.IsNullOrEmpty(sourceLang) || string.IsNullOrEmpty(targetLang))
            {
                throw new ApplicationException("Online Translatoin Error. Argment sourceLang or targetLang is null or empty.");
            }

            string[] outText = null;
            try
            {
                //An access token is available for ten mins but this application gets the token every time 
                admToken = admAuth.getAccessToken();
                // Create a header with the access_token property of the returned token
                string headerValue = MSTranslationResource.STR_HTTP_HEADER_TAG_BEARER + admToken.access_token;

                //execute translation
                outText = this.requestTranslation(headerValue, testArray, sourceLang, targetLang);
            }
            catch (WebException webExp)
            {
                //Console.WriteLine(webExp.StackTrace);
                throw webExp;
            }
            catch (Exception exp)
            {
                //throw new ApplicationException(getErrorMessage(exp), exp);
                //Console.WriteLine(exp.Message + " : " + exp.StackTrace);
                throw exp;
            }

            return outText;
        }

        private string[] requestTranslation(string authToken, string[] testArray, string sourceLang, string targetLang)
        {
            string from = sourceLang;
            string to = targetLang;

            //string reqBody = string.Format(body, from, "text/plain", translateArraySourceTexts[0], translateArraySourceTexts[1], translateArraySourceTexts[2], to);
            string reqBody = this.getBodyText(testArray, sourceLang, targetLang);
            string uri = MSTranslationResource.STR_URI_TRANSLATOR_URI;

            // create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add(MSTranslationResource.STR_HTTP_HEADER_ELEM_AUTH, authToken);
            request.ContentType = MSTranslationResource.STR_HTTP_REQ_CONTENT_TYPE_XML;
            request.Method = MSTranslationResource.STR_HTTP_METHOD_POST;
            request.Timeout = MSTranslationResource.INT_TIMEOUT_MILLISEC;

            //is this code necessary?
            using (System.IO.Stream stream = request.GetRequestStream())
            {
                byte[] arrBytes = Encoding.UTF8.GetBytes(reqBody);
                stream.Write(arrBytes, 0, arrBytes.Length);
            }

            // Get the response
            WebResponse response = null;
            List<string> retList = new List<string>();
            try
            {
                response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        // Deserialize the response
                        string strResponse = reader.ReadToEnd();

                        XDocument doc = XDocument.Parse(@strResponse);
                        XNamespace ns = MSTranslationResource.STR_URI_XML_NAMESPACE;
                        foreach (XElement xe in doc.Descendants(ns + EnumMSTransResponseTag.TranslateArrayResponse.ToString()))
                        {
                            foreach (var node in xe.Elements(ns + EnumMSTransResponseTag.TranslatedText.ToString()))
                            {
                                retList.Add(node.Value);
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }

            return retList.ToArray();

        }


        private string getBodyText(string[] testArray, string sourceLang, string targetLang)
        {
            StringBuilder sb = null;
            try
            {
                StringBuilder sbTextElm = new StringBuilder();
                foreach (string str in testArray)
                {
                    sbTextElm.Append(string.Format(MSTranslationResource.STR_BODY_TEXT_CONTENT, str));
                }
                sb = new StringBuilder();
                sb.Append(MSTranslationResource.STR_BODY_ROOT_START);
                sb.Append(MSTranslationResource.STR_BODY_APPID);
                sb.Append(string.Format(MSTranslationResource.STR_BODY_LANG_FROM, sourceLang));
                sb.Append(MSTranslationResource.STR_BODY_TEXT_START);
                sb.Append(sbTextElm);
                sb.Append(MSTranslationResource.STR_BODY_TEXT_END);
                sb.Append(string.Format(MSTranslationResource.STR_BODY_LANG_TO, targetLang));
                sb.Append(MSTranslationResource.STR_BODY_ROOT_END);
            }
            catch (Exception exp)
            {
                //Console.WriteLine(exp.Message + " : " + exp.StackTrace);
                throw exp;
            }

            return sb.ToString();
        }

        private string getErrorMessage(Exception e)
        {
            WebException webExp = e as WebException;
            if (webExp == null)
            {
                return e.StackTrace;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.ToString());

            // Obtain detailed error information
            string strResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)webExp.Response)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                    {
                        strResponse = sr.ReadToEnd();
                    }
                }
            }
            sb.AppendLine(string.Format(MSTranslationResource.STR_HTTP_ERROR_MSG, webExp.Status, strResponse));

            return sb.ToString();
        }
    }
}
