using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Bussiness.CenterService
{
    [DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "4.0.0.0")]
	public class CenterServiceClient : ClientBase<ICenterService>, ICenterService
    {
        private static NetTcpBinding CreateDefaultBinding()
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            binding.MaxReceivedMessageSize = 2147483647;
            return binding;
        }

        private static EndpointAddress CreateDefaultAddress()
        {
            return new EndpointAddress("net.tcp://127.0.0.1:2009/CenterService");
        }

        public CenterServiceClient()
            : base(CreateDefaultBinding(), CreateDefaultAddress())
        {
        }

        public CenterServiceClient(Binding binding, EndpointAddress remoteAddress)
			: base(binding, remoteAddress)
        {
        }

        public ServerData[] GetServerList()
        {
			return base.Channel.GetServerList();
        }

        public bool ChargeMoney(int userID, string chargeID)
        {
			return base.Channel.ChargeMoney(userID, chargeID);
        }

        public bool SystemNotice(string msg)
        {
			return base.Channel.SystemNotice(msg);
        }

        public bool KitoffUser(int playerID, string msg)
        {
			return base.Channel.KitoffUser(playerID, msg);
        }

        public bool ReLoadServerList()
        {
			return base.Channel.ReLoadServerList();
        }

        public bool MailNotice(int playerID)
        {
			return base.Channel.MailNotice(playerID);
        }

        public bool ActivePlayer(bool isActive)
        {
			return base.Channel.ActivePlayer(isActive);
        }

        public bool CreatePlayer(int id, string name, string password, bool isFirst)
        {
			return base.Channel.CreatePlayer(id, name, password, isFirst);
        }

        public bool ValidateLoginAndGetID(string name, string password, int zoneId, ref int userID, ref bool isFirst)
        {
			return base.Channel.ValidateLoginAndGetID(name, password, zoneId, ref userID, ref isFirst);
        }

        public bool AASUpdateState(bool state)
        {
			return base.Channel.AASUpdateState(state);
        }

        public int AASGetState()
        {
			return base.Channel.AASGetState();
        }

        public int ExperienceRateUpdate(int serverId)
        {
			return base.Channel.ExperienceRateUpdate(serverId);
        }

        public int NoticeServerUpdate(int serverId, int type)
        {
			return base.Channel.NoticeServerUpdate(serverId, type);
        }

        public bool UpdateConfigState(int type, bool state)
        {
			return base.Channel.UpdateConfigState(type, state);
        }

        public int GetConfigState(int type)
        {
			return base.Channel.GetConfigState(type);
        }

        public bool Reload(string type)
        {
			return base.Channel.Reload(type);
        }
    }
}
