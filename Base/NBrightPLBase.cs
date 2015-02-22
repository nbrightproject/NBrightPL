using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightPL.Base
{
    public class NBrightPLBase : DotNetNuke.Entities.Modules.PortalModuleBase
	{
        public NBrightDNN.NBrightDataController ModCtrl;
		public bool DebugMode = false;
		public string ModuleKey = "";
		public string UploadFolder = "";
		public string SelUserId = "";
        public string ThemeFolder = "";

        public DotNetNuke.Framework.CDefault BasePage
        {
            get { return (DotNetNuke.Framework.CDefault) this.Page; }
        }

		protected override void OnInit(EventArgs e)
		{

            ModCtrl = new NBrightDataController();

		    base.OnInit(e);
           
          
        }


        #region "Display Methods"


        public void DoDetail(Repeater rp1, NBrightInfo obj)
        {
            var l = new List<object> { obj };
            rp1.DataSource = l;
            rp1.DataBind();
        }

        public void DoDetail(Repeater rp1)
        {
            var obj = new NBrightInfo(true);
            var l = new List<object> { obj };
            rp1.DataSource = l;
            rp1.DataBind();
        }

        #endregion



    }
}
