// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using DotNetNuke.Entities.Portals;
using NBrightPL.common;
using Nevoweb.DNN.NBrightPL.Base;

namespace Nevoweb.DNN.NBrightPL
{

    public partial class BreadCrumb : SkinObjectBase
    {

        // private members

        protected Literal lBreadCrumb;

        #region "Public Members"

        public string Separator { get; set; }
        public string CssClass { get; set; }
        public string RootLevel { get; set; }
        public Boolean HtmlList { get; set; }
        public Boolean HideWithNoBreadCrumb { get; set; }


        #endregion

        protected override void OnLoad(EventArgs e)
        {

            if (!Page.IsPostBack)
            {

                var objCtrl = new NBrightDataController();


                // public attributes
                var strSeparator = "";
                if (!String.IsNullOrEmpty(Separator))
                {
                    if (Separator.Contains("src="))
                    {
                        Separator = Separator.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                    }
                    strSeparator = Separator;
                }
                else
                {
                    if (!HtmlList) strSeparator = "&nbsp;<img alt=\"*\" src=\"" + Globals.ApplicationPath + "/images/breadcrumb.gif\">&nbsp;";
                }

                var strCssClass = "";
                if (!string.IsNullOrEmpty(CssClass)) strCssClass = CssClass;

                int intRootLevel = 0;
                if (Utils.IsNumeric(RootLevel)) intRootLevel = int.Parse(RootLevel);

                var strBreadCrumbs = "";

                // process bread crumbs
                int intTab = 0;

                if (!(HideWithNoBreadCrumb && (PortalSettings.ActiveTab.BreadCrumbs.Count == 1)))
                {
                    for (intTab = intRootLevel; intTab <= PortalSettings.ActiveTab.BreadCrumbs.Count - 1; intTab++)
                    {
                        if (intTab != intRootLevel) strBreadCrumbs += strSeparator;

                        var objTab = (TabInfo) PortalSettings.ActiveTab.BreadCrumbs[intTab];
                        var dataRecord = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "PL",
                            objTab.TabID.ToString(""));
                        var dataRecordLang = objCtrl.GetDataLang(dataRecord.ItemID, Utils.GetCurrentCulture());
                        var pagename = dataRecordLang.GetXmlProperty("genxml/textbox/pagename");

                        if (HtmlList)
                        {
                            strBreadCrumbs += "<ul class=\"" + strCssClass + "\">";
                            if (objTab.DisableLink)
                                strBreadCrumbs += "<li>" + pagename + "</li>";
                            else
                                strBreadCrumbs += "<li>" + "<a href=\"" + objTab.FullUrl + "\">" + pagename + "</a>" + "</li>";
                            strBreadCrumbs += "</ul>";
                        }
                        else
                        {
                            if (objTab.DisableLink)
                                strBreadCrumbs += "<span class=\"" + strCssClass + "\">" + pagename + "</span>";
                            else
                                strBreadCrumbs += "<a href=\"" + objTab.FullUrl + "\" class=\"" + strCssClass + "\">" + pagename + "</a>";
                        }
                    }
                }
                lBreadCrumb = new Literal();
                lBreadCrumb.Text = strBreadCrumbs;
                NBrightPLBreadCrumb.Controls.Add(lBreadCrumb);
            }

        }
    }
}
