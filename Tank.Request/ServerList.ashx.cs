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
                            total += s.Online;
                            // Local play: always advertise a joinable server. Road may push State=2
                            // (busy/full) into DB even when the zone is empty, which traps the
                            // Flash client on the loading screen after templates finish.
                            int state = s.State <= 0 ? 1 : s.State;
                            if (state == 2 || state == 3 || state == 4)
                            {
                                state = 1;
                            }
                            int mustLevel = s.MustLevel < 1000 ? 1000 : s.MustLevel;
                            result.Add(Road.Flash.FlashUtils.CreateServerInfo(s.ID, s.Name, "127.0.0.1", s.Port, state, mustLevel, 0, s.Online));
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
            context.Response.Write(result.ToString(false));
        }

        public bool IsReusable => false;
    }
}
