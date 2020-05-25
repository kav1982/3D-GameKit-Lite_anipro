using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    namespace Message
    {
        // 消息类型
        public enum MessageType
        {
            DAMAGED,// 受到伤害
            DEAD,// 死亡
            RESPAWN,// 重生
            //Add your user defined message type after
        }

        /// <summary>
        /// 战斗系统的消息接口
        /// </summary>
        public interface IMessageReceiver
        {
            void OnReceiveMessage(MessageType type, object sender, object msg);
        }
    } 
}
