using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D.GameCommands
{
    // 此类是用来处理命令接收的
    //Class used to call the proper GameCommandHandler subclass to a given GameCommandType received from a subclass of SendGameCommand
    public class GameCommandReceiver : MonoBehaviour
    {
        // 这里维护了一个字典, 用来保存各种命令类型和对应行为列表
        Dictionary<GameCommandType, List<System.Action>> handlers = new Dictionary<GameCommandType, List<System.Action>>();

        // 收到某一个指令， 触发回调函数
        public void Receive(GameCommandType e)
        {
            List<System.Action> callbacks = null;
            if (handlers.TryGetValue(e, out callbacks))
            {
                foreach (var i in callbacks) i();
            }
        }

        // 注册一个指令
        public void Register(GameCommandType type, GameCommandHandler handler)
        {
            List<System.Action> callbacks = null;
            if (!handlers.TryGetValue(type, out callbacks))
            {
                callbacks = handlers[type] = new List<System.Action>();
            }
            callbacks.Add(handler.OnInteraction);
        }

        // 移除一个指令
        public void Remove(GameCommandType type, GameCommandHandler handler)
        {
            handlers[type].Remove(handler.OnInteraction);
        }


    }

}
