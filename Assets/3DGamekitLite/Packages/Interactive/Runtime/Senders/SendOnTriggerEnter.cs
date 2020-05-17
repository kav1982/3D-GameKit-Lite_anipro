using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gamekit3D.GameCommands
{

    public class SendOnTriggerEnter : TriggerCommand
    {
        public LayerMask layers;

        void OnTriggerEnter(Collider other) //进入碰撞器范围
        {
            if (0 != (layers.value & 1 << other.gameObject.layer)) //根据选择的层级触发事件
            {
                Send();
            }
        }
    }
}
