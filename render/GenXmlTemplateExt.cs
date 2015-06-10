using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Installer.Writers;
using DotNetNuke.Services.Localization;
using NBrightCore;
using NBrightCore.common;
using NBrightCore.providers;
using NBrightCore.render;
using DotNetNuke.Entities.Users;
using NBrightDNN;
using NBrightPL.common;
using Nevoweb.DNN.NBrightPL;

namespace NBrightPL.render
{
    public class GenXmlTemplateExt : GenXProvider
    {

        private string _rootname = "genxml";
        private string _databindColumn = "XMLData";
        private ConcurrentStack<Boolean> visibleStatus;

        #region "Override methods"
        // This section overrides the interface methods for the GenX provider.
        // It allows providers to create controls/Literals in the NBright template system.

        public override bool CreateGenControl(string ctrltype, Control container, XmlNode xmlNod, string rootname = "genxml", string databindColum = "XMLData", string cultureCode = "", Dictionary<string, string> settings = null, ConcurrentStack<Boolean> visibleStatusIn = null)
        {
            visibleStatus = visibleStatusIn;

            //remove namespace of token.
            // If the NBrigthCore template system is being used across mutliple modules in the portal (that use a provider interface for tokens),
            // then a namespace should be added to the front of the type attribute, this stops clashes in the tokening system. NOTE: the "tokennamespace" tag is now supported as well
            if (ctrltype.StartsWith("nbpl:")) ctrltype = ctrltype.Substring(5);

            _rootname = rootname;
            _databindColumn = databindColum;

            switch (ctrltype)
            {
                case "treetabli":
                    CreateTreeLi(container, xmlNod);
                    return true;
                case "cultureselect":
                    CreateEditCultureSelect(container, xmlNod);
                    return true;
                default:
                    return false;

            }

        }

        #region "GenXmlFunction Extension"

        public override string GetField(Control ctrl)
        {
            return "";
        }

        public override void SetField(Control ctrl, string newValue)
        {
        }

        public override string GetGenXml(List<Control> genCtrls, XmlDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml")
        {
                return "";
        }

        public override string GetGenXmlTextBox(List<Control> genCtrls, XmlDocument xmlDoc, string originalXml, string folderMapPath, string xmlRootName = "genxml")
        {
                return "";
        }

        public override object PopulateGenObject(List<Control> genCtrls, object obj)
        {
                return "";
        }


        public override TestOfData TestOfDataBinding(object sender, EventArgs e)
        {
            var rtn = new TestOfData();
            rtn.DataValue = null;
            return rtn;
        }

        public override void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
        }

        #endregion

        #endregion

        #region "Private support Methods"
        //These methods create the actual controls and databinding for the controls specified in the "CreateGenControl" method.

        #region "Literal tokens"

        private void CreateTreeLi(Control container, XmlNode xmlNod)
        {
            var cssclass = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["cssclass"] != null)) cssclass = xmlNod.Attributes["cssclass"].InnerText;

            var lc = new Literal();
            lc.Text = LocalUtils.GetTreeTabLi(cssclass);
            lc.DataBinding += LiteralDataBinding;
            container.Controls.Add(lc);
        }


        private void CreateEditCultureSelect(Control container, XmlNode xmlNod)
        {
            var cssclass = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["cssclass"] != null)) cssclass = xmlNod.Attributes["cssclass"].InnerText;
            var cssclassli = "";
            if (xmlNod.Attributes != null && (xmlNod.Attributes["cssclassli"] != null)) cssclassli = xmlNod.Attributes["cssclassli"].InnerText;

            var enabledlanguages = LocaleController.Instance.GetLocales(PortalSettings.Current.PortalId);
            var strOut = "<ul class='" + cssclass + "'>";
            foreach (var l in enabledlanguages)
            {
                strOut += "<li>";
                strOut += "<a href='javascript:void(0)' lang='" + l.Value.Code + "' class='" + cssclassli + "'><img src='/Images/Flags/" + l.Value.Code + ".gif' alt='" + l.Value.NativeName + "' /></a>";
                strOut += "</li>";
            }
            strOut += "</ul>";

            var lc = new Literal();
            lc.Text = strOut;
            lc.DataBinding += LiteralDataBinding;
            container.Controls.Add(lc);
        }

        private void LiteralDataBinding(object sender, EventArgs e)
        {
            try
            {
                var lc = (Literal)sender;
                lc.Visible = visibleStatus.Last();
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        #endregion


        #endregion
    }
}
