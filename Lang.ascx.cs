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
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security;

namespace Nevoweb.DNN.NBrightPL
{

    public partial class Lang : SkinObjectBase 
    {

        #region Private Members

        private const string MyFileName = "Language.ascx";
        private string _SelectedItemTemplate;
        private string _alternateTemplate;
        private string _commonFooterTemplate;
        private string _commonHeaderTemplate;
        private string _footerTemplate;
        private string _headerTemplate;
        private string _itemTemplate;
        private string _localResourceFile;
        private LanguageTokenReplace _localTokenReplace;
        private string _separatorTemplate;
        private bool _showMenu = true;

        #endregion

        #region Public Properties

        public string AlternateTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_alternateTemplate))
                {
                    _alternateTemplate = Localization.GetString("AlternateTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _alternateTemplate;
            }
            set
            {
                _alternateTemplate = value;
            }
        }

        public string CommonFooterTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_commonFooterTemplate))
                {
                    _commonFooterTemplate = Localization.GetString("CommonFooterTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _commonFooterTemplate;
            }
            set
            {
                _commonFooterTemplate = value;
            }
        }

        public string CommonHeaderTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_commonHeaderTemplate))
                {
                    _commonHeaderTemplate = Localization.GetString("CommonHeaderTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _commonHeaderTemplate;
            }
            set
            {
                _commonHeaderTemplate = value;
            }
        }

        public string CssClass { get; set; }

        public string FooterTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_footerTemplate))
                {
                    _footerTemplate = Localization.GetString("FooterTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _footerTemplate;
            }
            set
            {
                _footerTemplate = value;
            }
        }

        public string HeaderTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_headerTemplate))
                {
                    _headerTemplate = Localization.GetString("HeaderTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _headerTemplate;
            }
            set
            {
                _headerTemplate = value;
            }
        }

        public string ItemTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_itemTemplate))
                {
                    _itemTemplate = Localization.GetString("ItemTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _itemTemplate;
            }
            set
            {
                _itemTemplate = value;
            }
        }

        public string SelectedItemTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_SelectedItemTemplate))
                {
                    _SelectedItemTemplate = Localization.GetString("SelectedItemTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _SelectedItemTemplate;
            }
            set
            {
                _SelectedItemTemplate = value;
            }
        }

        public string SeparatorTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_separatorTemplate))
                {
                    _separatorTemplate = Localization.GetString("SeparatorTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _separatorTemplate;
            }
            set
            {
                _separatorTemplate = value;
            }
        }

        public bool ShowLinks { get; set; }

        public bool ShowMenu
        {
            get
            {
                if ((_showMenu == false) && (ShowLinks == false))
                {
                    //this is to make sure that at least one type of selector will be visible if multiple languages are enabled
                    _showMenu = true;
                }
                return _showMenu;
            }
            set
            {
                _showMenu = value;
            }
        }

        public bool UseCurrentCultureForTemplate { get; set; }

        #endregion

        #region Protected Properties

        protected string CurrentCulture
        {
            get
            {
                return CultureInfo.CurrentCulture.ToString();
            }
        }

        protected string TemplateCulture
        {
            get
            {
                return (UseCurrentCultureForTemplate) ? CurrentCulture : "en-US";
            }
        }


        protected string LocalResourceFile
        {
            get
            {
                if (string.IsNullOrEmpty(_localResourceFile))
                {
                    _localResourceFile = Localization.GetResourceFile(this, MyFileName);
                }
                return _localResourceFile;
            }
        }

        protected LanguageTokenReplace LocalTokenReplace
        {
            get
            {
                if (_localTokenReplace == null)
                {
                    _localTokenReplace = new LanguageTokenReplace { resourceFile = LocalResourceFile };
                }
                return _localTokenReplace;
            }
        }

        #endregion

        #region Private Methods

        private string parseTemplate(string template, string locale)
        {
            string strReturnValue = template;
            try
            {
                if (!string.IsNullOrEmpty(locale))
                {
                    //for non data items use locale
                    LocalTokenReplace.Language = locale;
                }
                else
                {
                    //for non data items use page culture
                    LocalTokenReplace.Language = CurrentCulture;
                }

                //perform token replacements
                strReturnValue = LocalTokenReplace.ReplaceEnvironmentTokens(strReturnValue);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, Request.RawUrl);
            }
            return strReturnValue;
        }

        private void handleCommonTemplates()
        {
            if (string.IsNullOrEmpty(CommonHeaderTemplate))
            {
                litCommonHeaderTemplate.Visible = false;
            }
            else
            {
                litCommonHeaderTemplate.Text = parseTemplate(CommonHeaderTemplate, CurrentCulture);
            }
            if (string.IsNullOrEmpty(CommonFooterTemplate))
            {
                litCommonFooterTemplate.Visible = false;
            }
            else
            {
                litCommonFooterTemplate.Text = parseTemplate(CommonFooterTemplate, CurrentCulture);
            }
        }

        private bool LocaleIsAvailable(Locale locale)
        {
            var tab = PortalSettings.ActiveTab;
            if (tab.DefaultLanguageTab != null)
            {
                tab = tab.DefaultLanguageTab;
            }

            var localizedTab = TabController.Instance.GetTabByCulture(tab.TabID, tab.PortalID, locale);

            return localizedTab != null && !localizedTab.IsDeleted && TabPermissionController.CanViewPage(localizedTab);
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            selectCulture.SelectedIndexChanged += selectCulture_SelectedIndexChanged;
            rptLanguages.ItemDataBound += rptLanguages_ItemDataBound;

            try
            {
                var locales = new Dictionary<string, Locale>();
                IEnumerable<ListItem> cultureListItems = DotNetNuke.Services.Localization.Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, CurrentCulture, "", false);
                foreach (Locale loc in LocaleController.Instance.GetLocales(PortalSettings.PortalId).Values)
                {
                    string defaultRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", loc.Code), PortalSettings.PortalId, "Administrators");
                    if (!PortalSettings.ContentLocalizationEnabled ||
                        (LocaleIsAvailable(loc) &&
                            (PortalSecurity.IsInRoles(PortalSettings.AdministratorRoleName) || loc.IsPublished || PortalSecurity.IsInRoles(defaultRoles))))
                    {
                        locales.Add(loc.Code, loc);
                        foreach (var cultureItem in cultureListItems)
                        {
                            if (cultureItem.Value == loc.Code)
                            {
                                selectCulture.Items.Add(cultureItem);
                            }
                        }
                    }
                }
                if (ShowLinks)
                {
                    if (locales.Count > 1)
                    {
                        rptLanguages.DataSource = locales.Values;
                        rptLanguages.DataBind();
                    }
                    else
                    {
                        rptLanguages.Visible = false;
                    }
                }
                if (ShowMenu)
                {
                    if (!String.IsNullOrEmpty(CssClass))
                    {
                        selectCulture.CssClass = CssClass;
                    }
                    if (!IsPostBack)
                    {
                        //select the default item
                        if (CurrentCulture != null)
                        {
                            ListItem item = selectCulture.Items.FindByValue(CurrentCulture);
                            if (item != null)
                            {
                                selectCulture.SelectedIndex = -1;
                                item.Selected = true;
                            }
                        }
                    }
                    //only show language selector if more than one language
                    if (selectCulture.Items.Count <= 1)
                    {
                        selectCulture.Visible = false;
                    }
                }
                else
                {
                    selectCulture.Visible = false;
                }
                handleCommonTemplates();
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, Request.RawUrl);
            }
        }

        private void selectCulture_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Redirect to same page to update all controls for newly selected culture
            LocalTokenReplace.Language = selectCulture.SelectedItem.Value;
            //DNN-6170 ensure skin value is culture specific in case of  static localization
            DataCache.RemoveCache(string.Format(DataCache.PortalSettingsCacheKey, PortalSettings.PortalId, Null.NullString));

            // ---- START --------------------------------------------------
            // changed to deal with mutliple urls across languages.
            // --- Response.Redirect(LocalTokenReplace.ReplaceEnvironmentTokens("[URL]"));
            Response.Redirect(GetLangaugeUrl(selectCulture.SelectedItem.Value));
            // ---- END --------------------------------------------------


        }

        /// <summary>
        /// Binds data to repeater. a template is used to render the items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptLanguages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                var litTemplate = e.Item.FindControl("litItemTemplate") as Literal;
                if (litTemplate != null)
                {
                    //load proper template for this Item
                    string strTemplate = "";
                    switch (e.Item.ItemType)
                    {
                        case ListItemType.Item:
                            strTemplate = ItemTemplate;
                            break;
                        case ListItemType.AlternatingItem:
                            if (!string.IsNullOrEmpty(AlternateTemplate))
                            {
                                strTemplate = AlternateTemplate;
                            }
                            else
                            {
                                strTemplate = ItemTemplate;
                            }
                            break;
                        case ListItemType.Header:
                            strTemplate = HeaderTemplate;
                            break;
                        case ListItemType.Footer:
                            strTemplate = FooterTemplate;
                            break;
                        case ListItemType.Separator:
                            strTemplate = SeparatorTemplate;
                            break;
                    }

                    if (string.IsNullOrEmpty(strTemplate))
                    {
                        litTemplate.Visible = false;
                    }
                    else
                    {
                        if (e.Item.DataItem != null)
                        {
                            var locale = e.Item.DataItem as Locale;
                            if (locale != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
                            {
                                if (locale.Code == CurrentCulture && !string.IsNullOrEmpty(SelectedItemTemplate))
                                {
                                    strTemplate = SelectedItemTemplate;
                                }
                                // >----- START -----------------
                                // Langauge portalalias
                                strTemplate = strTemplate.Replace("[URL]", GetLangaugeUrl(locale.Code));
                                // >----- END -----------------

                                litTemplate.Text = parseTemplate(strTemplate, locale.Code);
                            }
                        }
                        else
                        {
                            //for non data items use page culture
                            litTemplate.Text = parseTemplate(strTemplate, CurrentCulture);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, Request.RawUrl);
            }
        }

        #endregion

        private string GetLangaugeUrl(string lang)
        {
            var objCtrl = new NBrightDataController();
            var dataRecord = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "PL", PortalSettings.ActiveTab.TabID.ToString(""));
            if (dataRecord != null)
            {

                var padic = CBO.FillDictionary<string, PortalAliasInfo>("HTTPAlias", DataProvider.Instance().GetPortalAliases());
                var portalalias = PortalSettings.Current.DefaultPortalAlias;
                foreach (var pa in padic)
                {
                    if (pa.Value.PortalID == PortalSettings.Current.PortalId)
                    {
                        if (lang == pa.Value.CultureCode)
                        {
                            portalalias = pa.Key;
                        }
                    }
                }
                var pagename = "";
                var dataTabLang = objCtrl.GetDataLang(dataRecord.ItemID, lang);
                if (dataTabLang != null)
                {
                    pagename = dataTabLang.GetXmlProperty("genxml/textbox/pageurl");
                }

                var newwebsiteurl = "//" + portalalias + pagename;

                return newwebsiteurl;
            }
            return PortalSettings.Current.DefaultPortalAlias;
        }

    }

}
