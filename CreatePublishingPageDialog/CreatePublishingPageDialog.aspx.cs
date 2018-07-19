using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using System.Linq;
using Microsoft.SharePoint.Utilities;
using Xnet.SP.Test.Utilities;

namespace Xnet.SP.Test.Layouts.Xnet.SP.Test
{
    public partial class CreatePublishingPageDialog : Microsoft.SharePoint.Publishing.Internal.CodeBehind.CreatePublishingPageDialog15
    {
        private string folderUrl;
        private string originalRequestedName;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            folderUrl = SPRequestParameterUtility.GetValue<string>(base.Request, "RootFolder", SPRequestParameterSource.QueryStringValues);

            SPWeb web = SPContext.Current.Web;
            SPFieldChoice field = (SPFieldChoice)web.Fields.GetFieldByInternalName("NewsType");

            if (!Page.IsPostBack)
            {
                ddlNewsType.Items.Clear();
                field.Choices.Cast<string>().ToList().ForEach(x => ddlNewsType.Items.Add(x));
            }
            else
            {
                originalRequestedName = nameInput.Text;
                if (originalRequestedName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                {
                    originalRequestedName = originalRequestedName.Remove(checked(originalRequestedName.Length - ".aspx".Length));
                }

                nameInput.Text = nameInput.Text.Replace(' ', '-');
            }
        }

        protected new void CreateButton_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                SPLongOperation.Begin(delegate (SPLongOperation longOperation)
                {
                    SPWeb web = SPContext.Current.Web;
                    PublishingWeb currentPublishingWeb = PublishingWeb.GetPublishingWeb(web);
                    PublishingPage publishingPage = null;
                    SPFolder sPFolder = null;
                    if (!string.IsNullOrEmpty(folderUrl))
                    {
                        sPFolder = currentPublishingWeb.Web.GetFolder(folderUrl);
                        if (!sPFolder.Exists)
                        {
                            string url = Helper.ConcatUrls(folderUrl, nameInput.Text);
                            SPUtility.CreateParentFoldersForFile(currentPublishingWeb.PagesList, url, false);
                            sPFolder = currentPublishingWeb.Web.GetFolder(folderUrl);
                        }
                    }
                    PageLayout pageLayout = null;
                    string text = base.Request.QueryString.Get("PLUrl");
                    if (string.IsNullOrEmpty(text))
                    {
                        pageLayout = currentPublishingWeb.DefaultPageLayout;
                    }
                    else
                    {
                        try
                        {
                            pageLayout = new PageLayout(base.Web.GetListItem(text));
                        }
                        catch (Exception)
                        {
                            Logger.ToLog(new Exception(string.Format("Unable to create PageLayout from listitem of path : {0}", text)));
                            pageLayout = currentPublishingWeb.DefaultPageLayout;
                        }
                    }
                    publishingPage = SPHelper.CreatePublishingPage(currentPublishingWeb, nameInput.Text, pageLayout, sPFolder, false);
                    if (publishingPage != null && originalRequestedName != null)
                    {
                        publishingPage.Title = originalRequestedName;
                        publishingPage.ListItem["NewsType"] = ddlNewsType.SelectedValue;
                        publishingPage.Update();
                    }

                    string text2 = SPHttpUtility.UrlPathEncode(publishingPage.ListItem.File.ServerRelativeUrl, false);
                    string FinishUrl = SPHelper.DesignModeUrl(text2);

                    if (!string.IsNullOrEmpty(base.Request.QueryString.Get("IsDlg")))
                    {
                        if (base.Request.QueryString["shouldRedirectPage"] == "0")
                        {
                            string scriptLiteralToEncode = SPHelper.ConvertToAbsoluteUrl(FinishUrl, currentPublishingWeb.Web.Site, true);
                            longOperation.EndScript("window.frameElement.commitPopup('" + SPHttpUtility.EcmaScriptStringLiteralEncode(scriptLiteralToEncode) + "');");
                        }
                        else
                        {
                            longOperation.EndScript("window.frameElement.navigateParent('" + SPHttpUtility.EcmaScriptStringLiteralEncode(FinishUrl) + "');");
                        }
                    }
                    else
                    {
                        longOperation.End(FinishUrl);
                    }
                });
            }
        }
    }
}
