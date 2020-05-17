using UnityEngine;

namespace Gamekit3D.GameCommands
{

    public class GameplayCounter : GameCommandHandler
    {
        public int currentCount = 0;
        public int targetCount = 3;

        // 计数器每增加一个时需要执行的行为
        [Space]
        [Tooltip("Send a command when increment is performed. (optional)")]
        public SendGameCommand onIncrementSendCommand;
        [Tooltip("Perform an action when increment is performed. (optional)")]
        public GameCommandHandler onIncrementPerformAction;

        // 计数器达到目标要求时需要执行的行为
        [Space]
        [Tooltip("Send a command when target count is reacted. (optional)")]
        public SendGameCommand onTargetReachedSendCommand;
        [Tooltip("Perform an action when target count is reacted. (optional)")]
        public GameCommandHandler onTargetReachedPerformAction;


        public override void PerformInteraction()
        {
            // isTriggered 需要重新设计。 因为父类的设计这只是遵循一次来处理的
            currentCount += 1;
            if (currentCount >= targetCount)
            {
                if (onTargetReachedPerformAction != null) onTargetReachedPerformAction.PerformInteraction();
                if (onTargetReachedSendCommand != null) onTargetReachedSendCommand.Send();
                isTriggered = true;
            }
            else
            {
                if (onIncrementPerformAction != null) onIncrementPerformAction.PerformInteraction();
                if (onIncrementSendCommand != null) onIncrementSendCommand.Send();
                isTriggered = false;
            }
        }

    }

}
