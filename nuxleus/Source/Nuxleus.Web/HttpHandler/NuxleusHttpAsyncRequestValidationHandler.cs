using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Threading;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.SessionState;
using System.Collections;
using Memcached.ClientLibrary;
using System.Text;
using Saxon.Api;
using System.Xml;
using Nuxleus.Configuration;
using Nuxleus.Transform;
using System.Collections.Generic;
using Nuxleus.Memcached;
using Nuxleus.Cryptography;
using Nuxleus.Atom;
using Nuxleus.Storage;
using Nuxleus.Geo;
using Nuxleus.Async;
using System.Net;
using Nuxleus.Web.HttpApplication;

namespace Nuxleus.Web.HttpHandler
{
    public struct NuxleusHttpAsyncRequestValidationHandler : IHttpAsyncHandler
    {
        NuxleusAsyncResult m_asyncResult;
        PledgeCount m_pledgeCount;
        
        public void ProcessRequest(HttpContext context)
        {
            
        }

        public bool IsReusable
        {
            get { return false; }
        }

        #region IHttpAsyncHandler Members

        public IAsyncResult BeginProcessRequest (HttpContext context, AsyncCallback cb, object extraData)
        {
            m_asyncResult = new NuxleusAsyncResult(cb, extraData);

            m_pledgeCount = (PledgeCount)context.Application["pledgeCount"];

            using(XmlWriter writer = XmlWriter.Create(context.Response.Output))
            {
                DateTime now = DateTime.Now;

                writer.WriteStartDocument();
                    writer.WriteStartElement("message", "http://nuxleus.com/message/response");
                        writer.WriteStartElement("session");
                            writer.WriteStartAttribute("request-total");
                                writer.WriteString(m_pledgeCount.PledgeCountTotal.ToString());
                            writer.WriteEndAttribute();
                            writer.WriteStartAttribute("request-district");
                            writer.WriteString(m_pledgeCount.PledgeCountDistrict.ToString());
                            writer.WriteEndAttribute();
                        writer.WriteEndElement();
                    writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            m_asyncResult.CompleteCall();

            return m_asyncResult;
        }

        public void EndProcessRequest (IAsyncResult result)
        {
            
        }

        #endregion
    }
}