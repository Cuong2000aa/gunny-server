using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Bussiness;
using System.Web.Services;
using log4net;
using System.Reflection;

namespace Tank.Request
{
    /// <summary>
    /// Summary description for AccountRegister
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AccountRegister : IHttpHandler
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void ProcessRequest(HttpContext context)
        {
            XElement result = new XElement("Result");
            bool registerResult=false;
            try
            {
                string username = HttpUtility.UrlDecode(context.Request["username"]);
                string password = HttpUtility.UrlDecode(context.Request["password"]);
                string nickName = context.Request["nickname"] != null ? HttpUtility.UrlDecode(context.Request["nickname"]) : username;
                bool sex = false;
                int money = 100000;
                int giftoken = 100000;
                int gold = 100000;
                using (PlayerBussiness db = new PlayerBussiness())
                {
                    registerResult = db.RegisterUser(username, nickName, password, sex, money, giftoken, gold);
                }
            }
            catch (Exception ex)
            {
                log.Error("RegisterResult", ex);
            }
            finally
            {
                result.Add(new XAttribute("value", registerResult));
                result.Add(new XAttribute("message", registerResult ? "Success!" : "Failed!"));
                context.Response.ContentType = "text/plain";
                context.Response.Write(result.ToString(false));
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}