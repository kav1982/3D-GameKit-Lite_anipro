using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gamekit3D.GameCommands
{
    public enum GameCommandType
    {
        None,
        Activate,// ����
        Deactivate,// ȡ������
        Open,// ���� ������
        Close,// �ر� ������
        Spawn,
        Destroy,
        Start,
        Stop
    }
}
