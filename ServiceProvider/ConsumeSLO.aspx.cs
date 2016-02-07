using Microchip.SAML.SPLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microchip.SAML.SPLibrary.Saml;

namespace ServiceProvider
{
    public partial class ConsumeSLO : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AccountSettings accountSettings = new AccountSettings();

            LogoutResponse samlResponse = new LogoutResponse(accountSettings);
            samlResponse.LoadXmlFromBase64(Request.Form["SAMLResponse"]);
            if(samlResponse.IsValid())
            {
                HttpContext.Current.Session["user"] = "";
                lblMsg.Text = "<h1>You are logged out</h1>";
            }
            else
            {
                lblMsg.Text = "Logout failed";
            }
            //Response.Write(samlResponse.ToString());
        }
    }
}