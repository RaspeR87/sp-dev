using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration.Claims;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Publishing;
using System.Reflection;
using Microsoft.SharePoint.Portal;
using System.Globalization;

namespace Xnet.SP.Test.Utilities
{
    /// <summary>
    /// SharePoint Helper Class
    /// </summary>
    public static class SPHelper
    {
        public static int MaximumPageNameLength = MaximumPageNameLength = 128 - ".aspx".Length;
        public static int MaximumPageNameTokenLength = MaximumPageNameLength - 9;

        public static string ConvertToAbsoluteUrl(string siteCollectionRelativeUrl, SPSite site, bool unescape = true)
        {
            string text = null;
            string url = site.Url;
            url = Helper.EnsureEndsWithPathSeparator(url);
            Uri baseUri = new Uri(url);
            Uri uri = new Uri(baseUri, siteCollectionRelativeUrl);
            if (uri.IsAbsoluteUri)
            {
                text = uri.AbsoluteUri;
                if (unescape)
                {
                    text = Uri.UnescapeDataString(text);
                }
            }
            return text;
        }

        public static string ConvertToValidUrlFileName(string leafName, char charToReplaceInvalidChar)
        {
            if (string.IsNullOrEmpty(leafName))
            {
                throw new ArgumentNullException("leafName");
            }
            if (SPUrlUtility.IsLegalFileName(leafName))
            {
                return leafName;
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (charToReplaceInvalidChar != 0 && !SPUrlUtility.IsLegalCharInUrl(charToReplaceInvalidChar))
                charToReplaceInvalidChar = '\0';

            bool flag = charToReplaceInvalidChar != '\0';
            string text = leafName.Trim().Trim('.');
            char c = '\0';
            bool flag2 = false;
            char[] array = text.ToCharArray();
            foreach (char c2 in array)
            {
                if (!SPUrlUtility.IsLegalCharInUrl(c2) || (c2 == '.' && c == '.'))
                {
                    flag2 = true;
                }
                else
                {
                    if (flag2 && flag)
                    {
                        stringBuilder.Append(charToReplaceInvalidChar);
                        flag2 = false;
                    }
                    stringBuilder.Append(c2);
                    c = c2;
                }
            }
            return stringBuilder.ToString();
        }

        public static bool DoesFileExistInDocLibFolder(SPWeb parentWeb, string fileName, string folderUrl)
        {
            string strUrl = Helper.ConcatUrls(folderUrl, fileName);
            try
            {
                SPFile file = parentWeb.GetFile(strUrl);
                return file.Exists;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static SPRegionalSettings GetContextRegionalSettings(SPWeb contextWeb)
        {
            if (contextWeb.CurrentUser != null && contextWeb.CurrentUser.RegionalSettings != null)
            {
                return contextWeb.CurrentUser.RegionalSettings;
            }
            return contextWeb.RegionalSettings;
        }

        public static string GetUniquePageName(string nameToken, bool tryNakedToken, PublishingWeb pubWeb, bool doRemoveInvalidUrlChars, SPFolder targetFolder)
        {
            string empty = string.Empty;
            empty = ((targetFolder == null || targetFolder == pubWeb.PagesList.RootFolder) ? pubWeb.PagesList.RootFolder.ServerRelativeUrl : targetFolder.ServerRelativeUrl);
            if (nameToken.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                nameToken = nameToken.Remove(checked(nameToken.Length - ".aspx".Length));
            }
            nameToken = nameToken.Replace(' ', '-');
            nameToken = nameToken.Replace('.', '-');
            nameToken = nameToken.Replace('+', '-');
            if (doRemoveInvalidUrlChars)
            {
                nameToken = ConvertToValidUrlFileName(nameToken, '-');
            }
            string text = null;
            if (tryNakedToken)
            {
                if (nameToken.Length > MaximumPageNameLength)
                {
                    nameToken = nameToken.Substring(0, MaximumPageNameLength);
                }
                string text2 = nameToken + ".aspx";
                if (!DoesFileExistInDocLibFolder(pubWeb.Web, text2, empty))
                {
                    text = text2;
                }
            }
            if (text == null)
            {
                do
                {
                    if (nameToken.Length > MaximumPageNameTokenLength)
                    {
                        nameToken = nameToken.Substring(0, MaximumPageNameTokenLength);
                    }
                    DateTime dateTime = GetContextRegionalSettings(pubWeb.Web).TimeZone.UTCToLocalTime(DateTime.UtcNow);
                    text = nameToken + dateTime.ToString("MMdd-", CultureInfo.InvariantCulture) + (dateTime.Ticks % 9999).ToString(CultureInfo.InvariantCulture) + ".aspx";
                }
                while (DoesFileExistInDocLibFolder(pubWeb.Web, text, empty));
            }
            return text;
        }


        public static PublishingPage CreatePublishingPage(PublishingWeb CurrentPublishingWeb, string newPageName, PageLayout pageLayout, SPFolder folder, bool doCreateFriendlyUrl)
        {
            PublishingPage publishingPage = null;
            bool tryNakedToken = true;
            do
            {
                try
                {
                    if (doCreateFriendlyUrl)
                    {
                        newPageName = GetUniquePageName(newPageName, tryNakedToken, CurrentPublishingWeb, true, folder);
                    }
                    if (!newPageName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        newPageName += ".aspx";
                    }
                    publishingPage = ((folder != null) ? CurrentPublishingWeb.AddPublishingPage(newPageName, pageLayout, folder) : CurrentPublishingWeb.AddPublishingPage(newPageName, pageLayout));
                    if (publishingPage != null && doCreateFriendlyUrl && publishingPage.ListItem.ParentList.ForceCheckout)
                    {
                        publishingPage.CheckIn(string.Empty);
                        publishingPage.CheckOut();
                    }
                }
                catch (SPException ex)
                {
                    if (doCreateFriendlyUrl && (ex.ErrorCode == -2130575306 || ex.ErrorCode == -2130575257))
                    {
                        tryNakedToken = false;
                        goto end_IL_0082;
                    }
                    throw;
                    end_IL_0082:;
                }
            }
            while (doCreateFriendlyUrl && publishingPage == null);
            return publishingPage;
        }
    }
}
