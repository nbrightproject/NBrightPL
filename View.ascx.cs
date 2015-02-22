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
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using DotNetNuke.Entities.Portals;

namespace Nevoweb.DNN.NBrightPL
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : Base.NBrightPLBase
    {

        private String _templH = "";
        private String _templD = "";
        private String _templF = "";

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);

            try
            {
                
                _templH = "viewheader.html";
                _templD = "viewbody.html";
                _templF = "viewfooter.html";                                        


                // Get Display Header
                var rpDataHTempl = GetTemplateData(_templH); ;
                rpDataH.ItemTemplate = new GenXmlTemplate(rpDataHTempl);

                // Get Display Body
                var rpDataTempl = GetTemplateData(_templD);
                rpData.ItemTemplate = new GenXmlTemplate(rpDataTempl);

                // Get Display Footer
                var rpDataFTempl = GetTemplateData(_templF);
                rpDataF.ItemTemplate = new GenXmlTemplate(rpDataFTempl); 
                

            }
            catch (Exception exc)
            {

                rpDataF.ItemTemplate = new GenXmlTemplate(exc.Message);
                // catch any error and allow processing to continue, output error as footer template.
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {

            base.DoDetail(rpDataH);

            #region "Data Repeater"

            if (_templD.Trim() != "") // if we don;t have a template, don't do anything
            {
                var tl = DnnUtils.GetPortalTabs(PortalId);
                var l = new List<NBrightInfo>();


                foreach (var t in tl)
                {
                    TabInfo tabinfo = t.Value;
                    var nbiT = new NBrightInfo(true);
                    nbiT.ItemID = tabinfo.TabID;
                    nbiT.GUIDKey = tabinfo.TabName;                    
                    l.Add(nbiT);                    
                }

                var nbi = ModCtrl.GetByGuidKey(PortalId, ModuleId, "NBRIGHTPL", "NBRIGHTPLTABDATA");
                if (nbi != null)
                {
                    l.Add(nbi);
                    rpData.DataSource = l;
                    rpData.DataBind();
                }
                else
                {
                    nbi = new NBrightInfo(true);
                    nbi.GUIDKey = "NBRIGHTPLTABDATA";
                    nbi.ModuleId = ModuleId;
                    nbi.PortalId = PortalId;
                    nbi.TypeCode = "NBRIGHTPL";
                    nbi.SetXmlProperty("genxml/testing","TEST");
                    ModCtrl.Update(nbi);
                }

            }

            #endregion


            // display footer
            base.DoDetail(rpDataF);

        }

        #endregion

        #region  "Events "

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

        }

        #endregion


        private String GetTemplateData(String templatename)
        {
            var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/NBrightPL");
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, "Themes\\config", "");
            var templ = templCtrl.GetTemplateData(templatename, Utils.GetCurrentCulture());
            //templ = Utils.ReplaceSettingTokens(templ, Settings );
            templ = Utils.ReplaceUrlTokens(templ);
            return templ;
        }

    }

}
