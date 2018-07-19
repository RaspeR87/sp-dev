using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.SharePoint.Utilities;
using System.Text;

namespace Xnet.SP.Test.Utilities
{
    /// <summary>
    /// Common Helper Class
    /// </summary>
    public static class Helper
    {
        public static string ConcatUrls(string rootUrl, string url)
        {
            try
            {
                rootUrl = rootUrl.Trim();

                if (!url.ToUpper().StartsWith("HTTP"))
                {
                    if ((!rootUrl.EndsWith("/")) && (!url.StartsWith("/")))
                        url = rootUrl + "/" + url;
                    else if ((rootUrl.EndsWith("/")) && (url.StartsWith("/")))
                        url = rootUrl.Substring(0, rootUrl.Length - 1) + url;
                    else
                        url = rootUrl + url;
                }
            }
            catch { }

            return url;
        }

        public static string AddQueryStringParameterToUrl(string url, string paramName, string paramValue)
        {
            string text = SPHttpUtility.UrlKeyValueEncode(paramName) + "=" + SPHttpUtility.UrlKeyValueEncode(paramValue);
            string[] array = url.Split('?');
            string str = array[0];
            string text2 = string.Empty;
            if (array.Length > 1)
            {
                text2 = array[1];
            }
            if (text2.Length == 0)
            {
                return url + "?" + text;
            }
            if (!text2.Contains(paramName))
            {
                return url + "&" + text;
            }
            string[] array2 = text2.Split('&');
            StringBuilder stringBuilder = new StringBuilder();
            string[] array3 = array2;
            foreach (string text3 in array3)
            {
                if (!text3.StartsWith(paramName + "=", StringComparison.OrdinalIgnoreCase) && text3.Length != 0)
                {
                    stringBuilder.Append(text3 + "&");
                }
            }
            stringBuilder.Append(text);
            return str + "?" + stringBuilder.ToString();
        }

        public static string EnsureEndsWithPathSeparator(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                if (url.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }

                return url + "/";
            }

            return "";
        }
    }
}
