// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0618 // 屏蔽类型或成员已过时(对于 Animancer Lite 中的 ControllerStates)的警告
#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告

using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// 这是一个示例，演示如何在一个 <see cref="Float1ControllerState"/> 中封装一个包含单一混合树的 
    /// <see cref="RuntimeAnimatorController"/> 来轻松控制它的参数.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Locomotion - Linear Blend Tree Locomotion")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Locomotion/LinearBlendTreeLocomotion")]
    public sealed class LinearBlendTreeLocomotion : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private Float1ControllerTransition _Controller;

        private Float1ControllerState _State;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            // 因为Float1ControllerTransition是一个可以在多个对象之间共享的转换资产，所以我们不能简单地访问_Controller.Transition.
            // 因为它只会保存最近播放的状态(只对一个实例是正确的，而对其他实例则不正确).

            // 相反，我们会在播放后立即获取状态.

            _Animancer.Play(_Controller);
            _State = _Controller.Transition.State;

            // Play方法返回的状态会做同样的事情，但它只返回一个基本的AnimancerState，我们需要一个Float1ControllerState来访问它下面的参数属性，
            // 所以我们需要对它进行类型转换:_State = (Float1ControllerState)_Animancer.Play(_Controller);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 由一个 <see cref="UnityEngine.Events.UnityEvent"/> 在一个 <see cref="UnityEngine.UI.Slider"/>上设置.
        /// </summary>
        public float Speed
        {
            get { return _State.Parameter; }
            set { _State.Parameter = value; }
        }

        /************************************************************************************************************************/
    }
}
