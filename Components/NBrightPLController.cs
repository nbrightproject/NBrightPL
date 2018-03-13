using System;
using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using NBrightCore.common;
using NBrightDNN;

namespace NBrightPL.Components
{

    public class NBrightPLController : IPortable
	{

        #region Optional Interfaces

        #region IPortable Members

        /// -----------------------------------------------------------------------------
		/// <summary>
		///   ExportModule implements the IPortable ExportModule Interface
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name = "moduleId">The Id of the module to be exported</param>
		/// <history>
		/// </history>
		/// -----------------------------------------------------------------------------
		public string ExportModule(int ModuleId)
		{
			var objModCtrl = new ModuleController();
			var xmlOut = "<root>";

			var objModInfo = objModCtrl.GetModule(ModuleId);

			if (objModInfo != null)
			{
				var portalId = objModInfo.PortalID;
                var objTabCtrl = new TabController();
                var objCtrl = new NBrightDataController();
			    var l = objCtrl.GetList(portalId, -1, "PL");
			    foreach (var nbi in l)
			    {
			        if (Utils.IsNumeric(nbi.GUIDKey))
			        {
                        var tabInfo = objTabCtrl.GetTab(Convert.ToInt32(nbi.GUIDKey), portalId, true);
                        if (tabInfo != null)
                        {
                            nbi.SetXmlProperty("genxml/exporttabid", tabInfo.TabID.ToString());
                            nbi.GUIDKey = EnCode(tabInfo.TabPath); // use breadcrumd to relink on import.
                            xmlOut += nbi.ToXmlItem();
                        }			            
			        }
			    }
                var l2 = objCtrl.GetList(portalId, -1, "PLLANG");
                foreach (var nbi in l2)
                {
                    if (Utils.IsNumeric(nbi.GUIDKey))
                    {
                        var tabInfo = objTabCtrl.GetTab(Convert.ToInt32(nbi.GUIDKey), portalId, true);
                        if (tabInfo != null)
                        {
                            nbi.SetXmlProperty("genxml/exporttabid", tabInfo.TabID.ToString());
                            nbi.GUIDKey = EnCode(tabInfo.TabPath); // use breadcrumd to relink on import.
                            xmlOut += nbi.ToXmlItem();
                        }
                    }
                }

			}
            xmlOut += "</root>";

			return xmlOut;
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		///   ImportModule implements the IPortable ImportModule Interface
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name = "moduleId">The ID of the Module being imported</param>
		/// <param name = "content">The Content being imported</param>
		/// <param name = "version">The Version of the Module Content being imported</param>
		/// <param name = "userId">The UserID of the User importing the Content</param>
		/// <history>
		/// </history>
		/// -----------------------------------------------------------------------------

		public void ImportModule(int moduleId, string content, string version, int userId)
		{
			var xmlDoc = new XmlDocument();
			var objModCtrl = new ModuleController();
            var objCtrl = new NBrightDataController();
            var objModInfo = objModCtrl.GetModule(moduleId);
			if (objModInfo != null)
			{
                var portalId = objModInfo.PortalID;

                // import All records
                xmlDoc.LoadXml(content);

                var xmlNodList = xmlDoc.SelectNodes("root/item");
			    if (xmlNodList != null)
			    {
                    // delete existing
                    var l = objCtrl.GetList(portalId, -1, "PL");
                    foreach (var nbi in l)
                    {
                        objCtrl.Delete(nbi.ItemID);
                    }
                    var l2 = objCtrl.GetList(portalId, -1, "PLLANG");
                    foreach (var nbi in l2)
                    {
                        objCtrl.Delete(nbi.ItemID);
                    }

                    foreach (XmlNode xmlNod1 in xmlNodList)
			        {
			            var nbi = new NBrightInfo();
                        nbi.FromXmlItem(xmlNod1.OuterXml);
			            nbi.ItemID = -1;
			            nbi.ParentItemId = -1;
			            nbi.PortalId = portalId;
                        objCtrl.Update(nbi);
			        }
			    }

			    // relink the tabid using breadcrumb
                var tl = DnnUtils.GetPortalTabs(portalId);
                foreach (var t in tl)
                {
                    var tabInfo = (TabInfo) t.Value;
                    var strFilter = " and NB1.GUIDKey = '" + EnCode(tabInfo.TabPath) + "' ";

                    var l = objCtrl.GetList(portalId, moduleId, "PL", strFilter);
                    foreach (var i in l)
                    {
                        i.GUIDKey = tabInfo.TabID.ToString("");
                        objCtrl.Update(i);
                        var l2 = objCtrl.GetList(portalId, moduleId, "PLLANG", strFilter);
                        foreach (var i2 in l2)
                        {
                            i2.GUIDKey = tabInfo.TabID.ToString("");
                            i2.ParentItemId = i.ItemID;
                            objCtrl.Update(i2);
                        }

                    }

                }

			}

		}


        private static string EnCode(string stringToEncode)
        {
            var strOut = "";
            for (var i = 0; i < stringToEncode.Length; i++)
            {
                strOut += ((int)stringToEncode[i]) + ".";
            }
            return strOut;
        }
        #endregion


        #endregion

    }

}
