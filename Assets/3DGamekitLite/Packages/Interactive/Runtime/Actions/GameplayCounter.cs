using UnityEngine;

namespace Gamekit3D.GameCommands
{

    public class GameplayCounter : GameCommandHandler
    {
        public int currentCount = 0;
        public int targetCount = 3;

        // ������ÿ����һ��ʱ��Ҫִ�е���Ϊ
        [Space]
        [Tooltip("Send a command when increment is performed. (optional)")]
        public SendGameCommand onIncrementSendCommand;
        [Tooltip("Perform an action when increment is performed. (optional)")]
        public GameCommandHandler onIncrementPerformAction;

        // �������ﵽĿ��Ҫ��ʱ��Ҫִ�е���Ϊ
        [Space]
        [Tooltip("Send a command when target count is reacted. (optional)")]
        public SendGameCommand onTargetReachedSendCommand;
        [Tooltip("Perform an action when target count is reacted. (optional)")]
        public GameCommandHandler onTargetReachedPerformAction;


        public override void PerformInteraction()
        {
            // isTriggered ��Ҫ������ơ� ��Ϊ����������ֻ����ѭһ���������
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
