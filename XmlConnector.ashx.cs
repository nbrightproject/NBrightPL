using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NBrightPL.common;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace Nevoweb.DNN.NBrightPL
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class XmlConnector : IHttpHandler
    {
        private String _lang = "";

        public void ProcessRequest(HttpContext context)
        {
            #region "Initialize"
            
            var strOut = "";

            var paramCmd = Utils.RequestQueryStringParam(context, "cmd");
            var itemId = Utils.RequestQueryStringParam(context, "itemid");
            var lang = Utils.RequestQueryStringParam(context, "lang");
            var language = Utils.RequestQueryStringParam(context, "language");

            #region "setup language"

            // because we are using a webservice the system current thread culture might not be set correctly,
            //  so use the lang/lanaguge param to set it.
            if (lang == "") lang = language;
            if (!string.IsNullOrEmpty(lang)) _lang = lang;

            #endregion

            #endregion

            #region "Do processing of command"

            var objCtrl = new NBrightDataController();

            strOut = "ERROR!! - No Security rights for current user!";
            switch (paramCmd)
            {
                case "test":
                    strOut = "<root>" + UserController.GetCurrentUserInfo().Username + "</root>";
                    break;
                case "getformdata":
                    strOut = GetTabData(context, "viewbody.html");
                    break;
                case "getbasedata":
                    strOut = GetTabData(context, "viewbodybase.html");
                    break;
                case "saveformdata":
                    if (CheckRights()) strOut = SaveTabData(context);
                    break;
            }

            #endregion

            #region "return results"

            //send back xml as plain text
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(strOut);
            context.Response.End();

            #endregion

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        #region "Methods"


        private String GetTabData(HttpContext context, String templateName)
        {
            try
            {
                var strOut = "";
                //get uploaded params
                var ajaxInfo = GetAjaxFields(context);
                var tabid = ajaxInfo.GetXmlProperty("genxml/hidden/itemid");
                var selectlang = ajaxInfo.GetXmlProperty("genxml/hidden/selectlang");
                var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
                var baselang = ajaxInfo.GetXmlProperty("genxml/hidden/baselang");

                if (Utils.IsNumeric(tabid))
                {
                    if (templateName == "viewbodybase.html") selectlang = baselang;
                    if (selectlang == "") selectlang = lang; // use current culture 

                    // get template
                    var bodyTempl = GetTemplateData(templateName, lang);

                    //get data
                    var tabData = new TabData(tabid, selectlang);
                    strOut = GenXmlFunctions.RenderRepeater(tabData.Info, bodyTempl);                    
                }

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            
        }

        private String SaveTabData(HttpContext context)
        {
            try
            {
                //get uploaded params
                var ajaxInfo = GetAjaxFields(context);
                var tabid = ajaxInfo.GetXmlProperty("genxml/hidden/tabid");
                var selectlang = ajaxInfo.GetXmlProperty("genxml/hidden/selectlang");

                if (Utils.IsNumeric(tabid))
                {
                    var lang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
                    if (selectlang == "") selectlang = lang;
                    var tabData = new TabData(tabid, selectlang);

                    //save data
                    if (tabData.Exists)
                    {
                        var nodList2 = ajaxInfo.XMLDoc.SelectNodes("genxml/*");
                        if (nodList2 != null)
                        {
                            foreach (XmlNode nod1 in nodList2)
                            {
                                var nodList = ajaxInfo.XMLDoc.SelectNodes("genxml/" + nod1.Name.ToLower() + "/*");
                                if (nodList != null)
                                {
                                    foreach (XmlNode nod in nodList)
                                    {
                                        if (nod.Attributes != null && nod.Attributes["update"] != null)
                                        {
                                            if (nod1.Name.ToLower() == "checkboxlist")
                                            {
                                                if (nod.Attributes["update"].InnerText.ToLower() == "save")
                                                {
                                                    tabData.DataRecord.RemoveXmlNode("genxml/checkboxlist/" + nod.Name.ToLower());
                                                    tabData.DataRecord.AddXmlNode(nod.OuterXml, nod.Name.ToLower(), "genxml/checkboxlist");
                                                }
                                                if (nod.Attributes["update"].InnerText.ToLower() == "lang")
                                                {
                                                    tabData.DataLangRecord.RemoveXmlNode("genxml/checkboxlist/" + nod.Name.ToLower());
                                                    tabData.DataLangRecord.AddXmlNode(nod.OuterXml, nod.Name.ToLower(), "genxml/checkboxlist");
                                                }                                                                                                
                                            }
                                            else
                                            {
                                                if (nod.Attributes["update"].InnerText.ToLower() == "save")
                                                {
                                                    tabData.DataRecord.SetXmlProperty("genxml/" + nod1.Name.ToLower() + "/" + nod.Name.ToLower(),nod.InnerText);
                                                }
                                                if (nod.Attributes["update"].InnerText.ToLower() == "lang")
                                                {
                                                    tabData.DataLangRecord.SetXmlProperty("genxml/" + nod1.Name.ToLower() + "/" + nod.Name.ToLower(),nod.InnerText);
                                                }                                                
                                            }
                                        }
                                    }

                                }
                            }                            
                        }
                        tabData.Save();                        
                    }
                }

                return "";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        #endregion


        #region "functions"

        private String GetTemplateData(String templatename,String lang)
        {
            var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/NBrightPL");
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, "Themes\\config", "");
            var templ = templCtrl.GetTemplateData(templatename, lang);
            templ = Utils.ReplaceUrlTokens(templ);
            return templ;
        }

        private NBrightInfo GetAjaxFields(HttpContext context)
        {
            var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
            var xmlData = GenXmlFunctions.GetGenXmlByAjax(strIn, "");
            var objInfo = new NBrightInfo();

            objInfo.ItemID = -1;
            objInfo.TypeCode = "AJAXDATA";
            objInfo.XMLData = xmlData;
            var dic =  objInfo.ToDictionary();
            // set langauge if we have it passed.
            if (dic.ContainsKey("lang") && dic["lang"] != "") _lang = dic["lang"];

            // set the context  culturecode, so any DNN functions use the correct culture (entryurl tag token)
            if (_lang != "" && _lang != System.Threading.Thread.CurrentThread.CurrentCulture.ToString()) System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(_lang);
            return objInfo;
        }

        private Boolean CheckRights()
        {
            if (UserController.GetCurrentUserInfo().IsInRole("Manager") || UserController.GetCurrentUserInfo().IsInRole("Editor") || UserController.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }


        #endregion
    }
}