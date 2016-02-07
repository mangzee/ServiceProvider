using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.IO;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Collections.Generic;
using System.Text;
using Microchip.SAML.SPLibrary;


/// <summary>
/// Summary description for saml
/// </summary>

namespace Microchip.SAML.SPLibrary
{
    namespace Saml
    {
        public class Certificate
        {
            public X509Certificate2 cert;

            public void LoadCertificate(string certificate)
            {
                cert = new X509Certificate2(HttpRuntime.AppDomainAppPath + "\\" + certificate);
            }

            public void LoadCertificate(byte[] certificate)
            {
                cert = new X509Certificate2();
                cert.Import(certificate);
            }

            private byte[] StringToByteArray(string st)
            {
                byte[] bytes = new byte[st.Length];
                for (int i = 0; i < st.Length; i++)
                {
                    bytes[i] = (byte)st[i];
                }
                return bytes;
            }
        }

        public class Response
        {
            private XmlDocument xmlDoc;
            private AccountSettings accountSettings;
            private Certificate certificate;

            public Response(AccountSettings accountSettings)
            {
                this.accountSettings = accountSettings;
                certificate = new Certificate();
                certificate.LoadCertificate(accountSettings.certificate);
            }

            public void LoadXml(string xml)
            {
                xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.XmlResolver = null;
                xmlDoc.LoadXml(xml);
            }

            public void LoadXmlFromBase64(string response)
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                LoadXml(enc.GetString(Convert.FromBase64String(response)));
            }

            public bool IsValid()
            {
                bool status = false;

                XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
                manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
                XmlNodeList nodeList = xmlDoc.SelectNodes("//ds:Signature", manager);

                SignedXml signedXml = new SignedXml(xmlDoc);
                foreach (XmlNode node in nodeList)
                {
                    signedXml.LoadXml((XmlElement)node);
                    status = signedXml.CheckSignature(certificate.cert, true);
                    if (!status)
                        return false;
                }
                manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
                XmlNode statusnode = xmlDoc.SelectSingleNode("/samlp:Response/samlp:Status/samlp:StatusCode", manager);
                status = (statusnode.Attributes["Value"].Value == "urn:oasis:names:tc:SAML:2.0:status:Success") ? true : false;
                return status;
            }

            public string GetNameID()
            {
                XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
                manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
                manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

                XmlNode node = xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:NameID", manager);
                return node.InnerText;
            }

            public Dictionary<string, string> AttibuteCollection()
            {
                XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
                manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
                manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
                Dictionary<string, string> attributeDict = new Dictionary<string, string>();
                //XmlNode node = xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement", manager);
                XmlNodeList nodelist = xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute", manager);
                foreach (XmlNode node in nodelist)
                {
                    attributeDict.Add(node.Attributes["Name"].Value.ToString(), node.InnerText);
                }
                return attributeDict;
            }
        }

        public class AuthRequest
        {
            public string id;
            private string issue_instant;
            private AppSettings appSettings;
            private AccountSettings accountSettings;

            public enum AuthRequestFormat
            {
                Base64 = 1
            }

            public AuthRequest(AppSettings appSettings, AccountSettings accountSettings)
            {
                this.appSettings = appSettings;
                this.accountSettings = accountSettings;

                id = "_" + System.Guid.NewGuid().ToString();
                issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            }

            public string GetRequest(AuthRequestFormat format)
            {
                using (StringWriter sw = new StringWriter())
                {
                    XmlWriterSettings xws = new XmlWriterSettings();
                    xws.OmitXmlDeclaration = true;

                    using (XmlWriter xw = XmlWriter.Create(sw, xws))
                    {
                        xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                        xw.WriteAttributeString("ID", id);
                        xw.WriteAttributeString("Version", "2.0");
                        xw.WriteAttributeString("IssueInstant", issue_instant);
                        xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                        xw.WriteAttributeString("AssertionConsumerServiceURL", appSettings.assertionConsumerServiceUrl);

                        xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                        xw.WriteString(appSettings.issuer);
                        xw.WriteEndElement();

                        xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                        xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:unspecified");
                        xw.WriteAttributeString("AllowCreate", "true");
                        xw.WriteEndElement();

                        xw.WriteStartElement("samlp", "RequestedAuthnContext", "urn:oasis:names:tc:SAML:2.0:protocol");
                        xw.WriteAttributeString("Comparison", "exact");
                        xw.WriteEndElement();

                        xw.WriteStartElement("saml", "AuthnContextClassRef", "urn:oasis:names:tc:SAML:2.0:assertion");
                        xw.WriteString("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport");
                        xw.WriteEndElement();

                        xw.WriteEndElement();
                    }

                    if (format == AuthRequestFormat.Base64)
                    {
                        byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(sw.ToString());
                        return System.Convert.ToBase64String(toEncodeAsBytes);
                    }

                    return null;
                }
            }
        }

        public class LogoutRequest
        {
            public string id;
            private string issue_instant;
            private AppSettings appSettings;
            private AccountSettings accountSettings;

            public enum LogoutRequestFormat
            {
                Base64 = 1
            }
            public LogoutRequest(AppSettings appSettings, AccountSettings accountSettings)
            {
                this.appSettings = appSettings;
                this.accountSettings = accountSettings;

                id = "_" + System.Guid.NewGuid().ToString();
                issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            public string GetRequest(LogoutRequestFormat format)
            {
                using (StringWriter sw = new StringWriter())
                {
                    XmlWriterSettings xws = new XmlWriterSettings();
                    xws.OmitXmlDeclaration = true;

                    using (XmlWriter xw = XmlWriter.Create(sw, xws))
                    {
                        xw.WriteStartElement("samlp", "LogoutRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                        xw.WriteAttributeString("ID", id);
                        xw.WriteAttributeString("Version", "2.0");
                        xw.WriteAttributeString("IssueInstant", issue_instant);
                        xw.WriteAttributeString("SingleLogoutServiceUrl", appSettings.singleLogoutServiceUrl);
                        xw.WriteAttributeString("SingleLogoutServiceBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                        xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                        xw.WriteString(appSettings.issuer);
                        xw.WriteEndElement();


                        xw.WriteStartElement("saml", "NameID", "urn:oasis:names:tc:SAML:2.0:assertion");
                        xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:unspecified");
                        xw.WriteString("idp-user");
                        xw.WriteEndElement();

                        xw.WriteStartElement("samlp", "SessionIndex", "urn:oasis:names:tc:SAML:2.0:protocol");
                        xw.WriteString(id); //please check this
                        xw.WriteEndElement();

                        xw.WriteEndElement();
                    }

                    if (format == LogoutRequestFormat.Base64)
                    {
                        byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(sw.ToString());
                        return System.Convert.ToBase64String(toEncodeAsBytes);
                    }

                    return null;
                }
            }
        }

        public class LogoutResponse
        {
            private XmlDocument xmlDoc;
            private AccountSettings accountSettings;
            private Certificate certificate;

            public LogoutResponse(AccountSettings accountSettings)
            {
                this.accountSettings = accountSettings;
                certificate = new Certificate();
                certificate.LoadCertificate(accountSettings.certificate);
            }

            public void LoadXml(string xml)
            {
                xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.XmlResolver = null;
                xmlDoc.LoadXml(xml);
            }

            public void LoadXmlFromBase64(string response)
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                LoadXml(enc.GetString(Convert.FromBase64String(response)));
            }

            public bool IsValid()
            {
                bool status = false;

                XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
                manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
                XmlNodeList nodeList = xmlDoc.SelectNodes("//ds:Signature", manager);

                SignedXml signedXml = new SignedXml(xmlDoc);
                foreach (XmlNode node in nodeList)
                {
                    signedXml.LoadXml((XmlElement)node);
                    status = signedXml.CheckSignature(certificate.cert, true);
                    if (!status)
                        return false;
                }
                manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
                manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
                XmlNode statusnode = xmlDoc.SelectSingleNode("/samlp:LogoutResponse/samlp:Status/samlp:StatusCode", manager);
                status = (statusnode.Attributes["Value"].Value == "urn:oasis:names:tc:SAML:2.0:status:Success") ? true : false;
                return status;
            }


        }
    }
}
