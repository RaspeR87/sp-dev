using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xnet.SP.MJU.Contents.Utilities;

namespace Xnet.SP.MJU.Contents.FieldTypes
{
    public class FTFileUploaderControl : BaseFieldControl
    {
        protected FileUpload fuFileUploader;
        protected HiddenField hfUrl;
        protected HiddenField hfText;
        protected PlaceHolder phCurrentFiles;
        protected PlaceHolder phScripts;

        protected override string DefaultTemplateName
        {
            get
            {
                return "XnetSPMJU_FTFileUploader";
            }
        }

        public virtual string Url
        {
            get
            {
                EnsureChildControls();

                if (base.ControlMode == SPControlMode.Display)
                {
                    object itemFieldValue = ItemFieldValue;
                    if (itemFieldValue == null)
                    {
                        return string.Empty;
                    }
                    if (itemFieldValue is SPFieldUrlValue)
                    {
                        return ((SPFieldUrlValue)ItemFieldValue).Url;
                    }
                    return itemFieldValue.ToString();
                }
                return hfUrl.Value;
            }
            set
            {
                EnsureChildControls();

                hfUrl.Value = value;
            }
        }

        public virtual string Text
        {
            get
            {
                EnsureChildControls();

                if (base.ControlMode == SPControlMode.Display)
                {
                    object itemFieldValue = ItemFieldValue;
                    if (itemFieldValue == null)
                    {
                        return string.Empty;
                    }
                    if (itemFieldValue is SPFieldUrlValue)
                    {
                        return ((SPFieldUrlValue)ItemFieldValue).Description;
                    }
                    return itemFieldValue.ToString();
                }
                return hfText.Value;
            }
            set
            {
                EnsureChildControls();

                hfText.Value = value;
            }
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();

                if (!String.IsNullOrEmpty(Url) && !String.IsNullOrEmpty(Text))
                {
                    SPFieldUrlValue field = new SPFieldUrlValue();
                    field.Url = Url;
                    field.Description = Text;

                    return field;
                }
                else if (fuFileUploader.HasFile)
                {
                    if (!base.List.EnableAttachments)
                    {
                        base.IsValid = false;
                        base.ErrorMessage = "Na seznamu nimate omogočene možnosti dodajanja prilog.";

                        return null;
                    }

                    byte[] array = new byte[fuFileUploader.PostedFile.ContentLength];
                    fuFileUploader.PostedFile.InputStream.Read(array, 0, fuFileUploader.PostedFile.ContentLength);

                    string fileName = fuFileUploader.PostedFile.FileName;

                    base.ListItem.Attachments.Add(fileName, array);

                    if (base.ControlMode == SPControlMode.New)
                    {
                        base.Web.AllowUnsafeUpdates = true;
                        base.ListItem.SystemUpdate();
                        base.Web.AllowUnsafeUpdates = false;
                    }

                    string url = SPUrlUtility.CombineUrl(base.ListItem.Attachments.UrlPrefix, fileName);

                    var member = Context.Request.Files.GetType().GetMethod("BaseRemove",
                      BindingFlags.Instance | BindingFlags.NonPublic);
                    member.Invoke(Context.Request.Files, new[] { fuFileUploader.UniqueID });

                    SPFieldUrlValue field = new SPFieldUrlValue();
                    field.Url = url;
                    field.Description = fileName;

                    return field;
                }
                else
                    return null;
            }
            set
            {
                EnsureChildControls();

                if (value == null)
                {
                    return;
                }

                SPFieldUrlValue field = (SPFieldUrlValue)value;
                Url = field.Url;
                Text = field.Description;

                if (ControlMode == SPControlMode.Edit)
                {
                    fuFileUploader.Enabled = false;

                    var htmlGuid = Guid.NewGuid();
                    var htmlGuidStr = htmlGuid.ToString("N");

                    phCurrentFiles.Controls.Add(new LiteralControl("<table border='0' cellpadding='0' cellspacing='0' id='tFTFileUploaderTable_" + htmlGuidStr + "'><tbody>"));

                    var attachmentLi = base.Web.GetFile(Url);

                    phCurrentFiles.Controls.Add(new LiteralControl("<tr id=\"tFTFileUploaderItem_" + attachmentLi.UniqueId.ToString("B") + "\"><td class=\"ms-vb\"><span dir=\"ltr\"><a onmousedown=\"return VerifyHref(this, event, '1', 'SharePoint.OpenDocuments.3', ''); return false;\" onclick=\"DispDocItemExWithServerRedirect(this, event, 'FALSE', 'FALSE', 'FALSE', 'SharePoint.OpenDocuments.3', '1', ''); return false;\" href=\"" + Url + "\">" + Text + "</a></span></td><td class=\"ms-propertysheet\"><img alt=\"Izbriši\" src=\"/_layouts/15/images/rect.gif?rev=43\">&nbsp;"));

                    HyperLink hlRemove = new HyperLink()
                    {
                        Text = "&nbsp;Izbriši",
                        NavigateUrl = "javascript:void(0)"
                    };
                    hlRemove.Attributes.Add("onclick", "RemoveAttachmentFromServer('" + attachmentLi.UniqueId.ToString("B") + "', 1); RemoveAttachmentFromFTFileUploader_" + htmlGuidStr + "('tFTFileUploaderItem_" + attachmentLi.UniqueId.ToString("B") + "', '" + hfUrl.ClientID + "', '" + hfText.ClientID + "', '" + fuFileUploader.ClientID + "'); return false;");
                    hlRemove.Attributes.Add("aria-label", "Izbriši " + field.Description);

                    phCurrentFiles.Controls.Add(hlRemove);

                    phCurrentFiles.Controls.Add(new LiteralControl("</td></tr>"));

                    phCurrentFiles.Controls.Add(new LiteralControl("</tbody></table>"));

                    phScripts.Controls.Add(new LiteralControl("<script type=\"text/javascript\">function RemoveAttachmentFromFTFileUploader_" + htmlGuidStr + "(elGuid, hfUrl, hfText, fuFileUploader) { document.getElementById('tFTFileUploaderTable_" + htmlGuidStr + "').deleteRow((document.getElementById(elGuid)).rowIndex); document.getElementById(hfUrl).value=''; document.getElementById(hfText).value=''; document.getElementById(fuFileUploader).disabled = false; }</script>"));
                }
            }
        }

        protected override void CreateChildControls()
        {
            if (Field == null || ControlMode == SPControlMode.Display)
                return;

            base.CreateChildControls();

            fuFileUploader = (FileUpload)TemplateContainer.FindControl("fuFileUploader");
            hfUrl = (HiddenField)TemplateContainer.FindControl("hfUrl");
            hfText = (HiddenField)TemplateContainer.FindControl("hfText");
            phCurrentFiles = (PlaceHolder)TemplateContainer.FindControl("phCurrentFiles");
            phScripts = (PlaceHolder)TemplateContainer.FindControl("phScripts");

            FTFileUploader parent = this.Field as FTFileUploader;
            if (parent.HideOOTBAttachFilesMenu)
            {
                if (ControlMode == SPControlMode.New)
                    phScripts.Controls.Add(new LiteralControl("<script type=\"text/javascript\">document.getElementById('Ribbon.ListForm.Edit.Actions').style.display = 'none';</script>"));
                else if (ControlMode == SPControlMode.Edit)
                    phScripts.Controls.Add(new LiteralControl("<script type=\"text/javascript\">document.getElementById('Ribbon.ListForm.Edit.Actions.AttachFile-Large').style.display = 'none';</script>"));
            }
        }
    }
}
