using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
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
            // default to current thread if we have no language.
            if (_lang == "") _lang = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();

            #endregion

            #endregion

            #region "Do processing of command"

            var objCtrl = new NBrightPLController();

            strOut = "ERROR!! - No Security rights for current user!";
            switch (paramCmd)
            {
                case "test":
                    strOut = "<root>" + UserController.GetCurrentUserInfo().Username + "</root>";
                    break;
                case "getproductlist":
                    strOut = GetProductList(context);
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


        #region "Product Methods"

        private String GetProductList(HttpContext context)
        {
            try
            {

                var settings = GetAjaxFields(context);

                return GetProductListData(settings);

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }


        }

        private String GetProductGeneralData(HttpContext context)
        {
            try
            {
                //get uploaded params
                var settings = GetAjaxFields(context);
                if (!settings.ContainsKey("itemid")) settings.Add("itemid", "");
                var productitemid = settings["itemid"];
                
                // get template
                var themeFolder = StoreSettings.Current.ThemeFolder;
                if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
                var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);
                var bodyTempl = templCtrl.GetTemplateData("productadmingeneral.html", _lang, true, true, true, StoreSettings.Current.Settings());

                //get data
                var prodData = ProductUtils.GetProductData(productitemid, _lang);
                var strOut = GenXmlFunctions.RenderRepeater(prodData.Info, bodyTempl);

                return strOut;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            
        }
        
        #endregion


        #region "functions"

        private String GetProductListData(Dictionary<String, String> settings,bool paging = true)
        {
            var strOut = "";

            if (!settings.ContainsKey("header")) settings.Add("header", "");
            if (!settings.ContainsKey("body")) settings.Add("body", "");
            if (!settings.ContainsKey("footer")) settings.Add("footer", "");
            if (!settings.ContainsKey("filter")) settings.Add("filter", "");
            if (!settings.ContainsKey("orderby")) settings.Add("orderby", "");
            if (!settings.ContainsKey("returnlimit")) settings.Add("returnlimit", "0");
            if (!settings.ContainsKey("pagenumber")) settings.Add("pagenumber", "0");
            if (!settings.ContainsKey("pagesize")) settings.Add("pagesize", "0");
            if (!settings.ContainsKey("searchtext")) settings.Add("searchtext", "");
            if (!settings.ContainsKey("searchcategory")) settings.Add("searchcategory", "");
            if (!settings.ContainsKey("cascade")) settings.Add("cascade", "False");

            var header = settings["header"];
            var body = settings["body"];
            var footer = settings["footer"];
            var filter = settings["filter"];
            var orderby = settings["orderby"];
            var returnLimit = Convert.ToInt32(settings["returnlimit"]);    
            var pageNumber = Convert.ToInt32(settings["pagenumber"]);            
            var pageSize = Convert.ToInt32(settings["pagesize"]);
            var cascade = Convert.ToBoolean(settings["cascade"]);

            var searchText = settings["searchtext"];
            var searchCategory = settings["searchcategory"];

            if (searchText != "") filter += " and (NB3.[ProductName] like '%" + searchText + "%' or NB3.[ProductRef] like '%" + searchText + "%' or NB3.[Summary] like '%" + searchText + "%' ) ";

            if (Utils.IsNumeric(searchCategory))
            {
                if (orderby == "{bycategoryproduct}") orderby += searchCategory;
                var objQual = DataProvider.Instance().ObjectQualifier;
                var dbOwner = DataProvider.Instance().DatabaseOwner;
                if (!cascade)
                    filter += " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where typecode = 'CATXREF' and XrefItemId = " + searchCategory + ") ";
                else
                    filter += " and NB1.[ItemId] in (select parentitemid from " + dbOwner + "[" + objQual + "NBrightBuy] where (typecode = 'CATXREF' and XrefItemId = " + searchCategory + ") or (typecode = 'CATCASCADE' and XrefItemId = " + searchCategory + ")) ";
            }
            else
            {
                if (orderby == "{bycategoryproduct}") orderby = " order by NB3.productname ";                
            }

            var recordCount = 0;

            var themeFolder = StoreSettings.Current.ThemeFolder;
            if (settings.ContainsKey("themefolder")) themeFolder = settings["themefolder"];
            var templCtrl = NBrightBuyUtils.GetTemplateGetter(themeFolder);

            if (!settings.ContainsKey("portalid")) settings.Add("portalid", PortalSettings.Current.PortalId.ToString("")); // aways make sure we have portalid in settings

            var objCtrl = new NBrightBuyController();

            var headerTempl = templCtrl.GetTemplateData(header, _lang, true, true, true, StoreSettings.Current.Settings());
            var bodyTempl = templCtrl.GetTemplateData(body, _lang, true, true, true, StoreSettings.Current.Settings());
            var footerTempl = templCtrl.GetTemplateData(footer, _lang, true, true, true, StoreSettings.Current.Settings());

            // replace any settings tokens (This is used to place the form data into the SQL)
            headerTempl = Utils.ReplaceSettingTokens(headerTempl, settings);
            headerTempl = Utils.ReplaceUrlTokens(headerTempl);
            bodyTempl = Utils.ReplaceSettingTokens(bodyTempl, settings);
            bodyTempl = Utils.ReplaceUrlTokens(bodyTempl);
            footerTempl = Utils.ReplaceSettingTokens(footerTempl, settings);
            footerTempl = Utils.ReplaceUrlTokens(footerTempl);

            var obj = new NBrightInfo(true);
            strOut = GenXmlFunctions.RenderRepeater(obj, headerTempl);

            if (paging) // get record count for paging
            {
                if (pageNumber == 0) pageNumber = 1;
                if (pageSize == 0) pageSize = StoreSettings.Current.GetInt("pagesize");
                recordCount = objCtrl.GetListCount(PortalSettings.Current.PortalId, -1, "PRD", filter,"PRDLANG",_lang);
            }

            var objList = objCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "PRD", "PRDLANG", _lang, filter, orderby, StoreSettings.Current.DebugMode,"",returnLimit,pageNumber,pageSize,recordCount);
            strOut += GenXmlFunctions.RenderRepeater(objList, bodyTempl);

            strOut += GenXmlFunctions.RenderRepeater(obj, footerTempl);

            // add paging if needed
            if (paging)
            {
                var pg = new NBrightCore.controls.PagingCtrl();
                strOut += pg.RenderPager(recordCount, pageSize, pageNumber);
            }

            return strOut;
        }

        private Dictionary<String, String> GetAjaxFields(HttpContext context)
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
            return dic;
        }

        private Boolean CheckRights()
        {
            if (UserController.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) || UserController.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }


        #endregion
    }
}