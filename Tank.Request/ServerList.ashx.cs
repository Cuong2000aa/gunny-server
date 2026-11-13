using System;
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Xml.Linq;
using log4net;
using Bussiness;
using SqlDataProvider.Data;

namespace Tank.Request
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class ServerList : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            bool value = false;
            string message = "Fail!";
            int total = 0;

            XElement result = new XElement("Result");

            try
            {
                using (ServiceBussiness sb = new ServiceBussiness())
                {
                    ServerInfo[] list = sb.GetServerList();
                    if (list != null)
                    {
                        foreach (ServerInfo s in list)
                        {
                            if (s.State == -1)
                                continue;
                            // total = số server (Flash/UI), không phải Online — Online=0 khiến total=0 → dễ treo 13/13.
                            total += 1;
                            // Flash ServerInfo: MAINTAIN=1, UNIMPEDED=2, HALF=4, ALL_FULL=5
                            // Local play: luôn trả UNIMPEDED (2). Trả State=1 = bảo trì → treo 13/13.
                            int state = 2;
                            int mustLevel = s.MustLevel < 1000 ? 1000 : s.MustLevel;
                            // Advertise the host the browser used (LAN IP / hostname), so other
                            // devices on Wi‑Fi get a reachable address; Ruffle socketProxy tunnels to Road.
                            string advertiseIp = context.Request.Url?.Host;
                            if (string.IsNullOrWhiteSpace(advertiseIp) ||
                                string.Equals(advertiseIp, "localhost", StringComparison.OrdinalIgnoreCase))
                            {
                                advertiseIp = "127.0.0.1";
                            }
                            // Online tối thiểu 1 để client không coi server “trống/chết”.
                            int online = s.Online > 0 ? s.Online : 1;
                            result.Add(Road.Flash.FlashUtils.CreateServerInfo(s.ID, s.Name, advertiseIp, s.Port, state, mustLevel, 0, online));
                        }
                    }
                }

                value = true;
                message = "Success!";
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ServerList.ashx] Error: " + ex);
                log.Error("Load server list error:", ex);
            }

            result.Add(new XAttribute("value", value));
            result.Add(new XAttribute("message", message));
            result.Add(new XAttribute("total", total));
            result.Add(new XAttribute("agentId", 1));
            result.Add(new XAttribute("AreaName", "Gunny macOS"));

            context.Response.ContentType = "text/plain";
            // UTF8 không BOM — tránh Flash/Ruffle XML() parse fail
            context.Response.BinaryWrite(new System.Text.UTF8Encoding(false).GetBytes(result.ToString(false)));
        }

        public bool IsReusable => false;
    }
}
