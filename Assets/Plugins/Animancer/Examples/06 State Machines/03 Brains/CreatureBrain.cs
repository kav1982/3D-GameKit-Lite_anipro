// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告.

using UnityEngine;

namespace Animancer.Examples.StateMachines.Brains
{
    /// <summary>
    /// 控制一个 <see cref="Brains.Creature"/> 动作的基类.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Brains - Creature Brain")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Brains/CreatureBrain")]
    public abstract class CreatureBrain : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private Creature _Creature;
        public Creature Creature
        {
            get { return _Creature; }
            set
            {
                // 更多Brains示例使用这个在运行时在Brains之间进行切换.

                if (_Creature == value)
                    return;

                var oldCreature = _Creature;
                _Creature = value;

                // 确保那个旧的角色不再映射这个brain.
                if (oldCreature != null)
                    oldCreature.Brain = null;

                // 给一个新的角色来映射这个 brain.
                // 我也只希望 brains 在有角色控制的时候才会被激活.
                if (value != null)
                {
                    value.Brain = this;
                    enabled = true;
                }
                else
                {
                    enabled = false;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary> brain 想要移动的方向. </summary>
        public Vector3 MovementDirection { get; protected set; }

        /// <summary> 显示 brain 是否在跑的状态.</summary>
        public bool IsRunning { get; protected set; }

        /************************************************************************************************************************/
    }
}
