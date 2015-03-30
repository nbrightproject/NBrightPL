using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Tabs;
using NBrightCore.providers;
using NBrightDNN;
using NBrightCore.common;


namespace NBrightPL.common
{

    public static class LocalUtils
    {

        public static String GetTreeTabLi(string tabcssclass = "")
        {
            var tabList = DotNetNuke.Entities.Tabs.TabController.GetTabsBySortOrder(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, Utils.GetCurrentCulture(), true);
            return GetTreeTabLi("", tabList, 0, 0, tabcssclass);
        }

        private static String GetTreeTabLi(String rtnList, List<TabInfo> tabList, int level, int parentid, string tabcssclass = "")
        {

            if (level > 30) return rtnList; // stop infinate loop
            rtnList += "<ul>";
            foreach (TabInfo tInfo in tabList)
            {
                var parenttestid = tInfo.ParentId;
                if (parenttestid < 0) parenttestid = 0;
                if (parentid == parenttestid)
                {
                    if (!tInfo.IsDeleted && tInfo.TabPermissions.Count > 2)
                    {
                        rtnList += "<li tabid='" + tInfo.TabID + "'";
                        if (tabcssclass != "") rtnList += " class='" + tabcssclass + "'";
                        rtnList += ">";
                        rtnList += tInfo.TabName;
                        rtnList += "</li>";
                        rtnList = GetTreeTabLi(rtnList, tabList, level + 1, tInfo.TabID, tabcssclass);
                    }
                }
            }
            rtnList += "</ul>";

            return rtnList;
        }

    }

}
