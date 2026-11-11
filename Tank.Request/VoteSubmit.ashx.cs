using System;
using System.Web;
using System.Xml.Linq;

namespace Tank.Request
{
    public class VoteSubmit : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            XElement result = new XElement("Result",
                new XAttribute("value", true),
                new XAttribute("message", "Success!")
            );
            context.Response.ContentType = "text/xml";
            context.Response.Write(result.ToString());
        }

        public bool IsReusable => false;
    }
}
