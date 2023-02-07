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
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.Skins;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using DotNetNuke.Entities.Portals;
using NBrightPL.common;
using Nevoweb.DNN.NBrightPL.Base;
using System.Web.UI;
using DotNetNuke.Services.Localization;
using DotNetNuke.Data;
using DotNetNuke.Common.Utilities;

namespace Nevoweb.DNN.NBrightPL
{

    public partial class Meta : SkinObjectBase 
    {

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                NBrightInfo info = null;
                var pagename = "";
                var objCtrl = new NBrightDataController();

                var eid = Utils.RequestQueryStringParam(Request, "eid");

                NBrightInfo dataRecord;
                if (Utils.IsNumeric(eid))
                {
                    dataRecord = objCtrl.Get(Convert.ToInt32(eid));
                }
                else
                {
                    dataRecord = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "PL", PortalSettings.ActiveTab.TabID.ToString(""));
                }

                if (dataRecord != null)
                {

                    var dataRecordLang = objCtrl.GetDataLang(dataRecord.ItemID, Utils.GetCurrentCulture());
                    if (dataRecordLang != null)
                    {
                        DotNetNuke.Framework.CDefault tp = (DotNetNuke.Framework.CDefault)this.Page;

                        if (Utils.IsNumeric(eid))
                        {
                            info = objCtrl.Get(Convert.ToInt32(eid), Utils.GetCurrentCulture());

                            pagename = info.GetXmlProperty("genxml/lang/genxml/textbox/pagename");
                            if (pagename == "") pagename = info.GetXmlProperty("genxml/textbox/pagename");
                            if (pagename == "") pagename = info.GetXmlProperty("genxml/lang/genxml/textbox/title");
                            if (pagename == "") pagename = info.GetXmlProperty("genxml/textbox/title");

                            var pagetitle = info.GetXmlProperty("genxml/lang/genxml/textbox/pagetitle");
                            if (pagetitle == "") pagetitle = info.GetXmlProperty("genxml/textbox/pagetitle");
                            if (pagetitle == "") pagetitle = info.GetXmlProperty("genxml/lang/genxml/textbox/title");
                            if (pagetitle == "") pagetitle = info.GetXmlProperty("genxml/textbox/title");

                            var pagekeywords = info.GetXmlProperty("genxml/lang/genxml/textbox/pagekeywords");

                            var pagedescription = info.GetXmlProperty("genxml/lang/genxml/textbox/pagedescription");

                            if (pagetitle != "") tp.Title = pagetitle;
                            if (pagedescription != "") tp.Description = pagedescription;
                            if (pagekeywords != "") tp.KeyWords = pagekeywords;

                        }
                        else
                        {
                            if (dataRecordLang.GetXmlProperty("genxml/textbox/pagetitle") != "")
                                tp.Title = dataRecordLang.GetXmlProperty("genxml/textbox/pagetitle");
                            tp.KeyWords = dataRecordLang.GetXmlProperty("genxml/textbox/tagwords");
                            if (dataRecordLang.GetXmlProperty("genxml/textbox/pagedescription") != "")
                                tp.Description = dataRecordLang.GetXmlProperty("genxml/textbox/pagedescription");
                        }


                        var cachekey = "NBrightPL*hreflang*" + PortalSettings.Current.PortalId + "*" + Utils.GetCurrentCulture() + "*" + PortalSettings.ActiveTab.TabID; // use nodeTablist incase the DDRMenu has a selector.
                        var canonicalurl = "";
                        var hreflangtext = "";
                        var hreflangobj = Utils.GetCache(cachekey);
                        if (hreflangobj != null) hreflangtext = hreflangobj.ToString();
                        if (hreflangtext == "" || true)
                        {
                            hreflangtext = "";  // clear so we don't produce multiple hreflang with cache.
                            var objTabCtrl = new TabController();
                            var dnnTab = objTabCtrl.GetTab(PortalSettings.ActiveTab.TabID, PortalSettings.Current.PortalId);
                            var enabledlanguages = LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId);
                            var padic = CBO.FillDictionary<string, PortalAliasInfo>("HTTPAlias", DataProvider.Instance().GetPortalAliases());
                            foreach (var l in enabledlanguages)
                            {

                                var portalalias = PortalSettings.Current.DefaultPortalAlias;
                                foreach (var pa in padic)
                                {
                                    if (pa.Value.PortalID == PortalSettings.Current.PortalId)
                                    {
                                        if (l.Key == pa.Value.CultureCode)
                                        {
                                            portalalias = pa.Key;
                                        }
                                    }
                                }

                                var urldata = "";
                                if (Utils.IsNumeric(eid))
                                {
                                    var infourl = objCtrl.Get(Convert.ToInt32(eid), l.Key);
                                    urldata = infourl.GetXmlProperty("genxml/lang/genxml/url");
                                    if (urldata == "")
                                    {
                                        urldata = "https://" + portalalias + pagename;
                                    }
                                }
                                else
                                {
                                    var dataTabLang = objCtrl.GetDataLang(dataRecord.ItemID, l.Key);
                                    if (dataTabLang != null)
                                    {
                                        pagename = dataTabLang.GetXmlProperty("genxml/textbox/pageurl");
                                        urldata = "https://" + portalalias + pagename;
                                    }
                                }

                                hreflangtext += "<link rel='alternative' href='" + urldata + "' hreflang='" + l.Key.ToLower() + "' />";
                                if (Utils.GetCurrentCulture() == l.Key)
                                {
                                    canonicalurl = urldata;
                                }


                            }
                            Utils.SetCache(cachekey, hreflangtext);
                        }

                        tp.Header.Controls.Add(new LiteralControl(hreflangtext));

                        if (DataProvider.Instance().GetInstallVersion().Major < 9)
                        {
                            tp.CanonicalLinkUrl = canonicalurl;
                        }

                    }
                }
            }
            catch (Exception exc)
            {
                //ignore
            }

        }



    }

}
