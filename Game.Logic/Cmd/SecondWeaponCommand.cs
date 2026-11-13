using Game.Base.Packets;
using Game.Logic.Phy.Object;

namespace Game.Logic.Cmd
{
    [GameCommand(84, "副武器")]
    public class SecondWeaponCommand : ICommandHandler
    {
        public void HandleCommand(BaseGame game, Player player, GSPacketIn packet)
        {
            if (player == null || !player.IsAttacking)
            {
                return;
            }
            if (player.DeputyWeapon == null || player.DeputyWeapon.Template == null)
            {
                return;
            }
            if (player.deputyWeaponCount <= 0)
            {
                return;
            }

            int before = player.deputyWeaponCount;
            player.UseSecondWeapon();

            // Đồng bộ số lần dùng còn lại về client (kể cả khi UseSecondWeapon đã SendUseDeputyWeapon)
            GSPacketIn pkg = new GSPacketIn(91, player.Id)
            {
                Parameter1 = player.Id
            };
            pkg.WriteByte(84);
            pkg.WriteInt(player.deputyWeaponCount);
            player.PlayerDetail.SendTCP(pkg);

            // Log nhẹ khi dùng thất bại (số lần không đổi) — giúp debug thiên sứ/BallConfig
            if (before == player.deputyWeaponCount && before > 0)
            {
                // silent: CanUseItem false (thiếu năng lượng / không tới lượt) — không trừ count
            }
        }
    }
}
