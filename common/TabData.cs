using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace NBrightPL.common
{
    public class TabData
    {
        public NBrightInfo DataRecord;
        public NBrightInfo DataLangRecord;
        public NBrightInfo Info;
        public TabInfo TabInfo;
        private String _lang = ""; // needed for webservice
        private int _portalId;

        /// <summary>
        /// Populate the class
        /// </summary>
        /// <param name="tabId">categoryId (use -1 to create new record)</param>
        /// <param name="lang"> </param>
        public TabData(String tabId,String lang)
        {
            _lang = lang;
            if (Utils.IsNumeric(tabId)) LoadData(Convert.ToInt32(tabId));
        }

        /// <summary>
        /// Populate the TabData in this class
        /// </summary>
        /// <param name="categoryId">categoryId (use -1 to create new record)</param>
        /// <param name="lang"> </param>
        public TabData(int tabId, String lang)
        {
            _lang = lang;
            LoadData(tabId);
        }

        #region "public functions/interface"

        /// <summary>
        /// Set to true if product exists
        /// </summary>
        public bool Exists { get; private set; }


        public String PageName
        {
            get
            {
                if (Exists)
                {
                    return DataLangRecord.GetXmlProperty("genxml/textbox/pagename");
                }
                return "";
            }
        }

        public String PageTitle
        {
            get
            {
                if (Exists)
                {
                    return DataLangRecord.GetXmlProperty("genxml/textbox/pagetitle");
                }
                return "";
            }
        }

        public String TagWords
        {
            get
            {
                if (Exists)
                {
                    return DataLangRecord.GetXmlProperty("genxml/textbox/tagwords");
                }
                return "";
            }
        }

        public String PageDescription
        {
            get
            {
                if (Exists)
                {
                    return DataLangRecord.GetXmlProperty("genxml/textbox/pagedescription");
                }
                return "";
            }
        }


        public int TabId
        {
            get
            {
                if (Utils.IsNumeric(DataRecord.GUIDKey)) return Convert.ToInt32(DataRecord.GUIDKey);
                return -1;
            }
        }

        public void Save()
        {
            var objCtrl = new NBrightDataController();
            objCtrl.Update(DataRecord);
            objCtrl.Update(DataLangRecord);
            var menucachekey = "NBrightPL*" + PortalSettings.Current.PortalId + "*" + _lang;
            Utils.RemoveCache(menucachekey);

        }


        public void ResetLanguage(String resetToLang)
        {
            if (resetToLang != DataLangRecord.Lang)
            {
                var resetToLangData = new TabData(DataRecord.ItemID, resetToLang);
                var objCtrl = new NBrightDataController();
                DataLangRecord.XMLData = resetToLangData.DataLangRecord.XMLData;
                objCtrl.Update(DataLangRecord);
            }
        }

        public int Validate()
        {
            var errorcount = 0;
            var objCtrl = new NBrightDataController();

            DataRecord.ValidateXmlFormat();

            if (DataLangRecord == null)
            {
                // we have no datalang record for this language, so get an existing one and save it.
                var l = objCtrl.GetList(_portalId, -1, "PLLANG", " and NB1.ParentItemId = " + DataRecord.ItemID.ToString(""));
                if (l.Count > 0)
                {
                    DataLangRecord = (NBrightInfo)l[0].Clone();
                    DataLangRecord.ItemID = -1;
                    DataLangRecord.Lang = _lang;
                    DataLangRecord.ValidateXmlFormat();
                    objCtrl.Update(DataLangRecord);
                }
            }
            
            // fix langauge records
            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                var l = objCtrl.GetList(_portalId, -1, "PLLANG", " and NB1.ParentItemId = " + DataRecord.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'");
                if (l.Count == 0 && DataLangRecord != null)
                {
                    var nbi = (NBrightInfo)DataLangRecord.Clone();
                    nbi.ItemID = -1;
                    nbi.Lang = lang;
                    objCtrl.Update(nbi);
                    errorcount += 1;
                }
                if (l.Count > 1)
                {
                    // we have more records than shoudl exists, remove any old ones.
                    var l2 = objCtrl.GetList(_portalId, -1, "PLLANG", " and NB1.ParentItemId = " + DataRecord.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'", "order by Modifieddate desc");
                    var lp = 1;
                    foreach (var i in l2)
                    {
                      if (lp >=2) objCtrl.Delete(i.ItemID);
                      lp += 1;
                    }
                }
            }

            return errorcount;
        }

        #endregion



        #region " private functions"

        private void LoadData(int tabId)
        {
            Exists = false;
            if (_lang != "")
            {
                _portalId = PortalSettings.Current.PortalId;
                var objTabCtrl = new DotNetNuke.Entities.Tabs.TabController();
                TabInfo = objTabCtrl.GetTab(tabId, _portalId, true);
                if (TabInfo != null)
                {
                    var objCtrl = new NBrightDataController();
                    DataRecord = objCtrl.GetByGuidKey(_portalId, -1, "PL", tabId.ToString(""));
                    if (DataRecord == null) DataRecord = AddNew(); // add new record.
                    if (_lang == "") _lang = Utils.GetCurrentCulture();
                    if (DataRecord != null)
                    {
                        Exists = true;
                        DataRecord.GUIDKey = TabInfo.TabID.ToString("");
                        DataLangRecord = objCtrl.GetDataLang(DataRecord.ItemID, _lang);
                        if (DataLangRecord == null) // rebuild langauge if we have a missing lang record
                        {
                            Validate();
                            DataLangRecord = objCtrl.GetDataLang(DataRecord.ItemID, _lang);
                            DataLangRecord.GUIDKey = TabInfo.TabID.ToString("");
                        }
                        Info = objCtrl.Get(DataRecord.ItemID, _lang);
                        Save();
                    }
                }                
            }
        }

        private NBrightInfo AddNew()
        {
            var objCtrl = new NBrightDataController();
            var nbi = new NBrightInfo(true);
            nbi.PortalId = _portalId;
            nbi.TypeCode = "PL";
            nbi.ModuleId = -1;
            nbi.ItemID = -1;
            nbi.GUIDKey = TabInfo.TabID.ToString("");
            var itemId = objCtrl.Update(nbi);
            nbi.ItemID = itemId;

            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                var nbi2 = new NBrightInfo(true);
                nbi2.PortalId = _portalId;
                nbi2.TypeCode = "PLLANG";
                nbi2.ModuleId = -1;
                nbi2.ItemID = -1;
                nbi2.Lang = lang;
                nbi2.ParentItemId = itemId;
                nbi2.GUIDKey = TabInfo.TabID.ToString("");
                nbi2.SetXmlProperty("genxml/textbox/pagename", TabInfo.TabName);
                nbi2.SetXmlProperty("genxml/textbox/pagetitle", TabInfo.Title);
                nbi2.SetXmlProperty("genxml/textbox/tagwords", TabInfo.KeyWords);
                nbi2.SetXmlProperty("genxml/textbox/pagedescription", TabInfo.Description);
                nbi2.ItemID = objCtrl.Update(nbi2);
            }

            return nbi;
        }


        #endregion
    }
}
