using Bussiness.Managers;
using SqlDataProvider.Data;
using System;

namespace Fighting.Server.GameObjects
{
    public class ProxyPlayerInfo
    {
        public int WeaponStrengthLevel { get; set; }

        public double AntiAddictionRate { get; set; }

        public float AuncherExperienceRate { get; set; }

        public float AuncherOfferRate { get; set; }

        public float AuncherRichesRate { get; set; }

        public double BaseAgility { get; set; }

        public double BaseAttack { get; set; }

        public double BaseBlood { get; set; }

        public double BaseDefence { get; set; }

        public bool CanUserProp { get; set; }

        public bool CanX2Exp { get; set; }

        public bool CanX3Exp { get; set; }

        public string FightFootballStyle { get; set; }

        public float GMExperienceRate { get; set; }

        public float GMOfferRate { get; set; }

        public float GMRichesRate { get; set; }

        public double GPAddPlus { get; set; }

        public int Healstone { get; set; }

        public int HealstoneCount { get; set; }

        public double OfferAddPlus { get; set; }

        public int SecondWeapon { get; set; }

        public int ServerId { get; set; }

        public int StrengthLevel { get; set; }

        public int TemplateId { get; set; }

        public int ZoneId { get; set; }

        public string ZoneName { get; set; }

        public int GoldTemplateId { get; set; }

        public DateTime goldBeginTime { get; set; }

        public int goldValidDate { get; set; }

        public ItemInfo GetHealstone()
        {
			ItemInfo info = null;
			if (Healstone != 0)
			{
				ItemTemplateInfo tpl = ItemMgr.FindItemTemplate(Healstone);
				if (tpl == null)
				{
					return null;
				}
				info = ItemInfo.CreateFromTemplate(tpl, 1, 1);
				if (info != null)
				{
					info.Count = HealstoneCount;
				}
			}
			return info;
        }

        public ItemInfo GetItemInfo()
        {
			ItemInfo info = null;
			if (SecondWeapon != 0)
			{
				ItemTemplateInfo tpl = ItemMgr.FindItemTemplate(SecondWeapon);
				if (tpl == null)
				{
					return null;
				}
				info = ItemInfo.CreateFromTemplate(tpl, 1, 1);
				if (info != null)
				{
					info.StrengthenLevel = StrengthLevel;
				}
			}
			return info;
        }

        public ItemInfo GetItemTemplateInfo()
        {
			ItemInfo itemInfo = null;
			int tid = TemplateId != 0 ? TemplateId : 7008;
			ItemTemplateInfo tpl = ItemMgr.FindItemTemplate(tid) ?? ItemMgr.FindItemTemplate(7008);
			if (tpl != null)
			{
				itemInfo = ItemInfo.CreateFromTemplate(tpl, 1, 1);
				if (itemInfo != null)
				{
					itemInfo.StrengthenLevel = WeaponStrengthLevel;
				}
			}
			if (itemInfo != null && GoldTemplateId != 0)
			{
				itemInfo.GoldEquip = ItemMgr.FindItemTemplate(GoldTemplateId);
				itemInfo.goldBeginTime = goldBeginTime;
				itemInfo.goldValidDate = goldValidDate;
			}
			return itemInfo;
        }
    }
}
