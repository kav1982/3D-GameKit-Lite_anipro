using UnityEngine;

namespace Gamekit3D
{
    public partial class Damageable : MonoBehaviour
    {
        public struct DamageMessage
        {
            public MonoBehaviour damager;
            public int amount;
            public Vector3 direction;//
            public Vector3 damageSource;// 攻击点--> 攻击者所在的位置
            public bool throwing;

            public bool stopCamera;
        }
    } 
}
