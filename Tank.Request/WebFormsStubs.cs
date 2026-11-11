using System;
using System.Web;

namespace System.Web.UI
{
    public class Page : IHttpHandler
    {
        public HttpContext Context => HttpContext.Current;
        public HttpRequest Request => Context?.Request;
        public HttpResponse Response => Context?.Response;
        public HttpServerUtility Server => Context?.Server;

        public virtual void ProcessRequest(HttpContext context)
        {
            Page_Load(this, EventArgs.Empty);
        }

        protected virtual void Page_Load(object sender, EventArgs e) {}
        public bool IsReusable => false;
    }
}

namespace System.Web.UI.HtmlControls
{
    public class HtmlForm
    {
    }
}

namespace System.Web.UI.WebControls
{
    public class TextBox
    {
        public string Text { get; set; } = string.Empty;
    }

    public class Button
    {
        public string Text { get; set; } = string.Empty;
    }
}

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPart
    {
    }
}

namespace System.Web.SessionState
{
    public interface IRequiresSessionState
    {
    }

    public interface IReadOnlySessionState : IRequiresSessionState
    {
    }
}

namespace System.Web.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WebServiceAttribute : Attribute
    {
        public string Namespace { get; set; } = string.Empty;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class WebServiceBindingAttribute : Attribute
    {
        public WsiProfiles ConformsTo { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public enum WsiProfiles
    {
        None,
        BasicProfile1_1
    }
}

namespace System.Web.Services.Protocols
{
    public class SoapHttpClientProtocol
    {
    }
}

namespace Ajax
{
    public class Utility
    {
        public static void RegisterTypeForAjax(Type type) {}
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AjaxMethodAttribute : Attribute {}
}
