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
        public NBrightInfo Info;
        public NBrightInfo DataRecord;
        public NBrightInfo DataLangRecord;
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

        public int ParentItemId
        {
            get
            {
                return DataRecord.ParentItemId;
            }
            set
            {
                DataRecord.ParentItemId = value;
            }
        }


        public String PageName
        {
            get
            {
                if (Exists)
                {
                    return Info.GetXmlProperty("genxml/lang/genxml/textbox/pagename");
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
                    return Info.GetXmlProperty("genxml/lang/genxml/textbox/pagetitle");
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
                    return Info.GetXmlProperty("genxml/lang/genxml/textbox/tagwords");
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
                    return Info.GetXmlProperty("genxml/lang/genxml/textbox/pagedescription");
                }
                return "";
            }
        }


        public int TabId
        {
            get
            {
                return DataRecord.ItemID;
            }
        }

        public void Save()
        {
            var objCtrl = new NBrightDataController();
            objCtrl.Update(DataRecord);
            objCtrl.Update(DataLangRecord);            
        }

        public void Update(NBrightInfo info)
        {
            var localfields = info.GetXmlProperty("genxml/hidden/localizedfields").Split(',');

            foreach (var f in localfields)
            {
                DataLangRecord.SetXmlProperty(f, info.GetXmlProperty(f));
                DataRecord.RemoveXmlNode(f);
            }
            var fields = info.GetXmlProperty("genxml/hidden/fields").Split(',');

            foreach (var f in fields)
            {
                DataRecord.SetXmlProperty(f, info.GetXmlProperty(f));
                DataLangRecord.RemoveXmlNode(f);                
            }
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
                var l = objCtrl.GetList(_portalId, -1, "PLLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString(""));
                if (l.Count > 0)
                {
                    DataLangRecord = (NBrightInfo)l[0].Clone();
                    DataLangRecord.ItemID = -1;
                    DataLangRecord.Lang = _lang;
                    DataLangRecord.ValidateXmlFormat();
                    objCtrl.Update(DataLangRecord);
                }
            }
            
            if (errorcount > 0) objCtrl.Update(DataRecord); // update if we find a error

            // fix langauge records
            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                var l = objCtrl.GetList(_portalId, -1, "PLLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'");
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
                    var l2 = objCtrl.GetList(_portalId, -1, "PLLANG", " and NB1.ParentItemId = " + Info.ItemID.ToString("") + " and NB1.Lang = '" + lang + "'", "order by Modifieddate desc");
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
            _portalId = PortalSettings.Current.PortalId;
            var objTabCtrl = new DotNetNuke.Entities.Tabs.TabController();
            TabInfo = objTabCtrl.GetTab(tabId, _portalId, true);

            Exists = false;
            if (tabId == -1) tabId = AddNew(); // add new record if -1 is used as id.
            var objCtrl = new NBrightDataController();
            if (_lang == "") _lang = Utils.GetCurrentCulture();
            var nbi = objCtrl.GetByGuidKey(_portalId,-1,"PL",tabId.ToString(""));
            if (nbi != null)
            {
                Exists = true;
                _portalId = Info.PortalId;
                DataRecord = objCtrl.GetData(nbi.ItemID);
                DataRecord.SetXmlProperty("genxml/textbox/pagename", TabInfo.TabName);
                DataRecord.SetXmlProperty("genxml/textbox/pagetitle", TabInfo.Title);
                DataRecord.SetXmlProperty("genxml/textbox/tagwords", TabInfo.KeyWords);
                DataRecord.SetXmlProperty("genxml/textbox/pagedescription", TabInfo.Description);
                Info = objCtrl.Get(nbi.ItemID, _lang);
                if (DataLangRecord == null) // rebuild langauge if we have a missing lang record
                {
                    Validate();
                    DataLangRecord = objCtrl.GetData(nbi.ItemID, _lang);
                }
                Info = (NBrightInfo)DataRecord.Clone();
                Info.AddSingleNode("lang","","genxml");
                Info.AddXmlNode("<lang>" + DataLangRecord.XMLData + "</lang>","lang","genxml");
            }
        }

        private int AddNew()
        {
            var nbi = new NBrightInfo(true);
            nbi.PortalId = _portalId;
            nbi.TypeCode = "PL";
            nbi.ModuleId = -1;
            nbi.ItemID = -1;
            nbi.GUIDKey = TabInfo.TabID.ToString("");
            var objCtrl = new NBrightDataController();
            var itemId = objCtrl.Update(nbi);

            foreach (var lang in DnnUtils.GetCultureCodeList(_portalId))
            {
                nbi = new NBrightInfo(true);
                nbi.PortalId = _portalId;
                nbi.TypeCode = "PLLANG";
                nbi.ModuleId = -1;
                nbi.ItemID = -1;
                nbi.Lang = lang;
                nbi.ParentItemId = itemId;
                objCtrl.Update(nbi);
            }

            return itemId;
        }


        #endregion
    }
}
