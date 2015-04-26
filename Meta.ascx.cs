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

namespace Nevoweb.DNN.NBrightPL
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Meta : SkinObjectBase 
    {

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                var objCtrl = new NBrightDataController();

                var dataRecord = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "PL", PortalSettings.ActiveTab.TabID.ToString(""));
                var dataRecordLang = objCtrl.GetDataLang(dataRecord.ItemID, Utils.GetCurrentCulture());

                var bPage = (DotNetNuke.Framework.CDefault) this.Page;
                bPage.Title = dataRecordLang.GetXmlProperty("genxml/textbox/pagetitle");
                bPage.KeyWords = dataRecordLang.GetXmlProperty("genxml/textbox/tagwords"); 
                bPage.Description = dataRecordLang.GetXmlProperty("genxml/textbox/pagedescription"); 

            }
            catch (Exception exc)
            {
                //ignore
            }

        }



    }

}
