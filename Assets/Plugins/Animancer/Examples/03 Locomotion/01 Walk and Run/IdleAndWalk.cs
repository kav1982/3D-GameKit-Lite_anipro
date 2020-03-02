// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// 根据用户输入匹配一个简单的字符，使其能够空闲或向前或向后行走.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Locomotion - Idle And Walk")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Locomotion/IdleAndWalk")]
    public class IdleAndWalk : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private AnimancerComponent _Animancer;
        public AnimancerComponent Animancer { get { return _Animancer; } }

        [SerializeField]
        private AnimationClip _Idle;
        public AnimationClip Idle { get { return _Idle; } }

        [SerializeField]
        private AnimationClip _Walk;
        public AnimationClip Walk { get { return _Walk; } }

        /************************************************************************************************************************/

        protected void Update()
        {
            // W or UpArrow = 1.
            // S or DownArrow = -1.
            // Otherwise 0.
            var movement = Input.GetAxisRaw("Vertical");
            if (movement != 0)
            {
                PlayMove();

                // 我们没有向后移动的动画，所以只是把正向的行走动画倒过来播放用作后退动画.
                _Animancer.States.Current.Speed = movement;

                // PlayMove可以返回它播放的AnimancerState，但是使用CurrentState会节省力一点.
            }
            else
            {
                // 如果我们不移动，回到待机状态.
                _Animancer.Play(_Idle, 0.25f);
            }
        }

        /************************************************************************************************************************/

        // 我们希望在IdleAndWalkAndRun脚本中重写此方法.
        protected virtual void PlayMove()
        {
            _Animancer.Play(_Walk, 0.25f);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 如果您将此类型派生的第二个脚本添加到相同的对象，它将更改现有组件的类型，从而允许您轻松地在 <see cref="IdleAndWalk"/> 
        /// 和 <see cref="IdleAndWalkAndRun"/>之间进行交换.
        /// </summary>
        protected virtual void Reset()
        {
            AnimancerUtilities.IfMultiComponentThenChangeType(this); //如果有多个组件,则改变类型.
        }

        /************************************************************************************************************************/
    }
}
