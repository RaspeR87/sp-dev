using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xnet.SP.Test.Utilities;

namespace Xnet.SP.Test.WebControls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:WikiNavigation runat=server></{0}:WikiNavigation>")]
    public class WikiNavigation : TreeView
    {
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("Drevo")]
        [Localizable(true)]
        public string ListName { get; set; }

        public SPList list;

        protected override void OnSelectedNodeChanged(EventArgs e)
        {
            Page.Response.Redirect(this.SelectedValue);
        }

        protected override void OnPagePreLoad(object sender, EventArgs e)
        {
            base.OnPagePreLoad(sender, e);

            this.DataBindings.Add(new TreeNodeBinding()
            {
                DataMember = "System.Data.DataRowView",
                TextField = "Text",
                ValueField = "Url"
            });

            List<KlasifikacijaItem> _postavke = new List<KlasifikacijaItem>();

            string cUrl = SPContext.Current.Web.Url;

            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                try
                {
                    using (var site = new SPSite(cUrl))
                    {
                        using (var web = site.OpenWeb())
                        {
                            list = web.FindListByName(ListName ?? "Drevo");
                            if (list != null)
                            {
                                SPQuery query = new SPQuery()
                                {
                                    ViewXml = "<View><Query><Where><Eq><FieldRef Name='Z' /><Value Type='Boolean'>0</Value></Eq></Where></Query></View>"
                                };

                                var lic = list.GetItems(query);

                                foreach (SPListItem li in lic)
                                {
                                    var postavka = new KlasifikacijaItem()
                                    {
                                        ID = li.ID,
                                        Text = li["Title"].ToString()
                                    };

                                    try
                                    {
                                        postavka.Url = new SPFieldUrlValue(li["Dokument"].ToString()).Url;
                                    }
                                    catch { }

                                    if (li["Nadrejeni"] != null)
                                    {
                                        var lID = new SPFieldLookupValue(li["Nadrejeni"].ToString()).LookupId;
                                        postavka.ParentID = lID;
                                    }

                                    _postavke.Add(postavka);
                                }

                                var sortedDataSet = _postavke.OrderBy(c => c.ParentID).ThenBy(c => c.Text).ToList().ToDataSet();

                                this.DataSource = new HierarchicalDataSet(sortedDataSet, "ID", "ParentID");
                                this.DataBind();

                                this.CollapseAll();

                                // expandanje specifičnega noda
                                if (Page.Request.Cookies["XnetWikiNavigationExpNodes"] != null)
                                {
                                    
                                    try
                                    {
                                        var currExpanded = Page.Request.Cookies["XnetWikiNavigationExpNodes"].Value.ToString();
                                        var currExpNodes = currExpanded.Split('|');

                                        if (currExpNodes.Length > 0)
                                        {
                                            var allNodes = this.Nodes.Cast<TreeNode>().SelectMany(GetNodeBranch).ToList();

                                            foreach (var currExpNode in currExpNodes)
                                            {
                                                try
                                                {
                                                    int currIndx = Int32.Parse(currExpNode);
                                                    allNodes[currIndx].Expand();
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ToLog(ex, "Error WikiNavigation");
                }
            });
        }

        private IEnumerable<TreeNode> GetNodeBranch(TreeNode node)
        {
            yield return node;

            foreach (TreeNode child in node.ChildNodes)
                foreach (var childChild in GetNodeBranch(child))
                    yield return childChild;
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);

            writer.Write("<style>");
            writer.Write("   .ms-core-sideNavBox-removeLeftMargin td { white-space: normal !important; }");
            writer.Write("</style>");

            writer.Write(@"
            <script type='text/javascript'>
	            var base_TreeView_ToggleNode = TreeView_ToggleNode;
	            
                TreeView_ToggleNode = function(data, index, node, lineType, children) {
                    var currExpanded = readCookie('XnetWikiNavigationExpNodes');
                    var currExpNodes = [];

                    if (currExpanded) {
                        currExpNodes = currExpanded.split('|');
                    }

                    var indexStr = index.toString();

                    var currElIndx = currExpNodes.indexOf(indexStr);                  

                    if (children.style.display == 'none') {
                        // je expanded
                        if (currElIndx == -1) {
                            currExpNodes.push(indexStr);
                        }
                    } else {
                        // je collapsed
                        if (currElIndx >= 0) {
                            currExpNodes.splice(currElIndx, 1);
                        }
                    }

                    console.log(currExpNodes);
                    createCookie('XnetWikiNavigationExpNodes', currExpNodes.join('|'));

	                base_TreeView_ToggleNode(data, index, node, lineType, children);
	            }
            </script>");

            if (list != null)
                writer.Write("<div style='margin:0 5px 10px 5px;'><a href=\"" + list.DefaultNewFormUrl + "?source=" + HttpUtility.UrlEncode(HttpContext.Current.Request.Url.ToString()) + "\">Dodaj dokument</a></div>");
        }
    }
}
