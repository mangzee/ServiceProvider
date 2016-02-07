using Microchip.SAML.SPLibrary;
using Microchip.SAML.SPLibrary.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ServiceProvider
{
    public partial class ServicePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            {
                if (!(string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["user"]))))
                {
                    Response.Redirect("ConsumeSSO.aspx");
                }
            }
        }

        protected void lnkInitiateSSO_Click(object sender, EventArgs e)
        {
            AccountSettings accountSettings = new AccountSettings();
            AuthRequest req = new AuthRequest(new AppSettings(), accountSettings);
            Response.Redirect(accountSettings.idp_sso_target_url + "?SAMLRequest=" + Server.UrlEncode(req.GetRequest(AuthRequest.AuthRequestFormat.Base64)));
        }
    }
}