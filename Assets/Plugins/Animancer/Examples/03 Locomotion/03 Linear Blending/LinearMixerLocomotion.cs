// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0618 // Type or member is obsolete (for MixerStates in Animancer Lite).
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// 一个关于如何使用 <see cref="LinearMixerState"/> 混合一组基于 <see cref="Speed"/> 参数的动画的例子.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Locomotion - Linear Mixer Locomotion")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Locomotion/LinearMixerLocomotion")]
    public sealed class LinearMixerLocomotion : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private LinearMixerTransition _Mixer;

        private LinearMixerState _State;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Animancer.Play(_Mixer);
            _State = _Mixer.Transition.State;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 由<see cref="UnityEngine.Events.UnityEvent"/> 在 <see cref="UnityEngine.UI.Slider"/> 上设置.
        /// </summary>
        public float Speed
        {
            get { return _State.Parameter; }
            set { _State.Parameter = value; }
        }

        /************************************************************************************************************************/
    }
}
