using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gamekit3D.GameCommands
{
    public enum GameCommandType
    {
        None,
        Activate,// 激活
        Deactivate,// 取消激活
        Open,// 开启 比如门
        Close,// 关闭 比如门
        Spawn,
        Destroy,
        Start,
        Stop
    }
}
