using Microchip.SAML.SPLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microchip.SAML.SPLibrary.Saml;
using System.Web.Security;

namespace ServiceProvider
{
    public partial class ConsumeSSO : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["user"])))
                {
                    // replace with an instance of the users account.
                    AccountSettings accountSettings = new AccountSettings();
                    Response samlResponse = new Response(accountSettings);
                    samlResponse.LoadXmlFromBase64(Request.Form["SAMLResponse"]);
                    if (samlResponse.IsValid())
                    {
                        lblUser.Text += "<b>User:" + samlResponse.GetNameID() + "</b><br>";
                        foreach (KeyValuePair<string, string> kvp in samlResponse.AttibuteCollection())
                        {
                            lblUser.Text +=kvp.Key + " = " + kvp.Value+"<br>";
                        }
                        HttpContext.Current.Session["user"] = lblUser.Text;
                    }
                    else
                    {
                        HttpContext.Current.Session["user"] = "";
                        lblUser.Text = "Failed";
                    }
                }
                else
                {
                    //Response.Redirect("ServicePage.aspx");
                    lblUser.Text = Convert.ToString(HttpContext.Current.Session["user"]);
                }
            }
        }
        protected void lnkInitiateSSO_Click(object sender, EventArgs e)
        {
            AccountSettings accountSettings = new AccountSettings();
            LogoutRequest req = new LogoutRequest(new AppSettings(), accountSettings);
            Response.Redirect(accountSettings.idp_slo_target_url + "?SAMLRequest=" + Server.UrlEncode(req.GetRequest(LogoutRequest.LogoutRequestFormat.Base64)));
        }
    }
}