using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Microchip.SAML.SPLibrary
{
    public class AppSettings
    {
        public string assertionConsumerServiceUrl = ConfigurationManager.AppSettings["AssertionConsumeURL"]; //"http://localhost:57816/Consume.aspx";
        public string singleLogoutServiceUrl = ConfigurationManager.AppSettings["SingleLogoutServiceUrl"]; //"http://localhost:57816/Consume.aspx";
        public string issuer = "urn:componentspace:ExampleServiceProvider";
    }
}
