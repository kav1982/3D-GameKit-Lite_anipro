namespace Gamekit3D.GameCommands
{
    public class RespawnPlayer : GameCommandHandler
    {// 角色出生的指令
        public Gamekit3D.PlayerController player;

        public override void PerformInteraction()
        {
            player.Respawn();
        }
    }
}
