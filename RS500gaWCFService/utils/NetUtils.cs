using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace RS500gaWCFService.utils
{
    public class NetUtils
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string WEB_URL_PARAM_FSTR = "&{0}=";
        public const string CONCAT_POST_PARAM_FSTR = "{0}={1}";
        private static int webCallsCount = 0;
        private static int successWebCallsCount = 0;


        public static string GetServiceResponse(string requestUrl)
        {
            string httpResponse = null;
            string funName = "GetServiceResponse(string requestUrl) - ";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Timeout = 15000;
            HttpWebResponse response = null;

            StreamReader reader = null;
            try
            {
                //response = (HttpWebResponse)request.GetResponseAsync();
                webCallsCount++;
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        successWebCallsCount++;
                        reader = new StreamReader(response.GetResponseStream());

                        httpResponse = Utils.SanitizeXmlString(reader.ReadToEnd());
                        reader.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                return Constants.RETVAL_MESSAGE_ERROR + ":" + funName + ex.Message;
            }

            return httpResponse;
        }

        public static string RemoveHtmlAndCDATATags(string CDATAText)
        {
            string cleanData = Regex.Replace(CDATAText, Constants.REGEX_CDATA_STR, string.Empty).Trim();
            cleanData = Regex.Replace(cleanData, Constants.REGEX_HREF_STR, string.Empty);
            return System.Net.WebUtility.HtmlDecode(cleanData);
        }

        public static string RemoveHtmlAndRegexTags(string CDATAText, string regexStr)
        {
            string cleanData = Regex.Replace(CDATAText, regexStr, string.Empty).Trim();
            cleanData = Regex.Replace(cleanData, Constants.REGEX_HREF_STR, string.Empty);
            return System.Net.WebUtility.HtmlDecode(cleanData);
        }

        public static string PostServiceResponse(string requestUrl, Dictionary<string, string> parameters)
        {
            string httpResponse = null;
            string funName = "PostServiceResponse(string requestUrl, Dictionary<string, string> parameters) - ";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            HttpWebResponse response = null;
            StreamReader reader = null;

            string postData = string.Empty;
            foreach (var par in parameters)
            {
                postData += string.Format(CONCAT_POST_PARAM_FSTR, par.Key, par.Value);
            }
            var data = Encoding.ASCII.GetBytes(postData);

            request.Timeout = 15000;
            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Accept = "application/xml";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                //using (response = (HttpWebResponse)request.GetResponse())
                //{
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    reader = new StreamReader(response.GetResponseStream());

                    httpResponse = reader.ReadToEnd();
                    reader.Close();
                }
                //}

            }
            catch (Exception ex)
            {
                log.Error(Constants.ERROR_LOADING, ex);
                return Constants.RETVAL_MESSAGE_ERROR + ":" + funName + ex.Message;
            }

            return httpResponse;
        }

    }
}