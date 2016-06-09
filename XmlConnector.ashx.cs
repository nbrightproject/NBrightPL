using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore;
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
            var baselang = Utils.RequestQueryStringParam(context, "baselang");

            #region "setup language"

            // because we are using a webservice the system current thread culture might not be set correctly,
            //  so use the lang/lanaguge param to set it.
            if (lang == "") lang = language;
            if (!string.IsNullOrEmpty(lang)) _lang = lang;

            // default to current thread if we have no language.
            if (_lang == "") _lang = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(_lang);

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
                case "getsetting":
                    if (CheckRights()) strOut = GetSettings(context, "settings.html");
                    break;
                case "savesetting":
                    if (CheckRights()) strOut = SaveSettings(context);
                    break;
                case "translate":
                    if (CheckRights())
                    {
                        TranslateForm(context);
                        strOut = "reload";
                    }
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

        private String SaveSettings(HttpContext context)
        {
            try
            {
                //get uploaded params
                var ajaxInfo = GetAjaxFields(context);
                var tabid = ajaxInfo.GetXmlProperty("genxml/hidden/tabid");
                var selectlang = ajaxInfo.GetXmlProperty("genxml/hidden/selectlang");

                var objCtrl = new NBrightDataController();
                var dataRecord = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "SETTINGS", "NBrightPL");
                if (dataRecord == null)
                {
                    dataRecord = new NBrightInfo(true); // populate empty XML so we can update nodes.
                    dataRecord.GUIDKey = "NBrightPL";
                    dataRecord.PortalId = PortalSettings.Current.PortalId;
                    dataRecord.TypeCode = "SETTINGS";
                    dataRecord.Lang = "";
                }
                dataRecord.ModuleId = -99; // always use -99, so we don't get picked up as data by NBrightMod.

                var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                dataRecord.UpdateAjax(strIn);
                objCtrl.Update(dataRecord);

                return "";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        private String GetSettings(HttpContext context, String templateName)
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

                    // get template
                    var objCtrl = new NBrightDataController();
                    var bodyTempl = GetTemplateData(templateName, lang);
                    var dataRecord = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "SETTINGS", "NBrightPL");
                    strOut = GenXmlFunctions.RenderRepeater(dataRecord, bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

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
                        var strIn = HttpUtility.UrlDecode(Utils.RequestParam(context, "inputxml"));
                        tabData.DataRecord.UpdateAjax(strIn);
                        tabData.DataLangRecord.UpdateAjax(strIn);
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

        private void TranslateForm(HttpContext context)
        {
            try
            {
                var objCtrl = new NBrightDataController();
                var settings = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "SETTINGS", "NBrightPL");
                var clientId = settings.GetXmlProperty("genxml/textbox/bingclientid");
                var clientSecret = settings.GetXmlProperty("genxml/textbox/bingclientsecret");

                var headerValue = Utils.GetTranslatorHeaderValue(clientId, clientSecret);

                //get uploaded params
                var ajaxInfo = GetAjaxFields(context);
                var tabid = ajaxInfo.GetXmlProperty("genxml/hidden/tabid");
                var selectlang = ajaxInfo.GetXmlProperty("genxml/hidden/lang");
                var baselang = ajaxInfo.GetXmlProperty("genxml/hidden/baselangtrans");
                if (selectlang == "") selectlang = Utils.GetCurrentCulture();
                if (baselang == "") baselang = Utils.GetCurrentCulture();

                if (Utils.IsNumeric(tabid) && (baselang != selectlang))
                {
                    var baseData = new TabData(tabid, baselang);
                    var tabData = new TabData(tabid, selectlang);
                    //save data
                    if (tabData.Exists && baseData.Exists)
                    {
                        baselang = baselang.Substring(0, 2);
                        selectlang = selectlang.Substring(0, 2);

                        var nodList = baseData.DataLangRecord.XMLDoc.SelectNodes("genxml/textbox/*");
                        if (nodList != null)
                        {
                            foreach (XmlNode nod in nodList)
                            {
                                var newText = Utils.GetTranslatedText(headerValue, nod.InnerText, baselang, selectlang);
                                tabData.DataLangRecord.SetXmlProperty("genxml/textbox/" + nod.Name, newText);
                            }
                            tabData.Save();                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ignore
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