// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 //  屏蔽字段从未被赋值，并且始终具有其默认值的警告.

using Animancer.FSM;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Brains
{
    /// <summary>
    /// 使用键盘输入控制角色的<see cref="CreatureBrain"/>.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Brains - Keyboard Brain")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Brains/KeyboardBrain")]
    public sealed class KeyboardBrain : CreatureBrain
    {
        /************************************************************************************************************************/

        [SerializeField] private CreatureState _Locomotion;

        /************************************************************************************************************************/

        private void Update()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input != Vector2.zero)
            {
                // 获取相机的前向和右向矢量,并将它们平放在XZ平面上.
                var camera = Camera.main.transform;

                var forward = camera.forward;
                forward.y = 0;
                forward.Normalize();

                var right = camera.right;
                right.y = 0;
                right.Normalize();

                // 通过将输入乘以这些轴来构建运动矢量.
                MovementDirection =
                    right * input.x +
                    forward * input.y;

                // 确定玩家是否想要运行.
                IsRunning = Input.GetButton("Fire3");//默认左 Shift.

                // 如果还没有进入移动状态,就进入移动状态.
                _Locomotion.TryEnterState();
            }
            else
            {
                // 清除移动向量,尽管空闲时不应该使用它.
                MovementDirection = Vector3.zero;

                // 如果还没有进入待机状态，则进入待机状态.
                Creature.Idle.TryEnterState();
            }
        }

        /************************************************************************************************************************/
    }
}
