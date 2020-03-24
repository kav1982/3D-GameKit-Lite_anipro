// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Events
{
    /// <summary>
    /// 一个<see cref="GolfHitController"/> 它使用一个 <see cref="SimpleEventReceiver"/> 来重现一个名为<c>"Event"</c>的动画事件.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Golf Events - Animation Simple")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.AnimationEvents/GolfHitControllerAnimationSimple")]
    public sealed class GolfHitControllerAnimationSimple : GolfHitController
    {
        /************************************************************************************************************************/

        [SerializeField] private SimpleEventReceiver _EventReceiver;

        /************************************************************************************************************************/

        /// <summary>
        /// 调用base<see cref="GolfHitController.Awake"/> 方法，并在swing动画结束时调用寄存器 <see cref="GolfHitController.EndSwing"/>
        /// <para></para>
        /// 通常，Animancer可以在转换中定义的结束时间调用注册的方法，但是在这种情况下，<see cref="AnimationClip"/> 
        /// 使用这个脚本有一个动画事件，函数名为“End”,当该事件时间过去时,它将执行注册的方法.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _Swing.Events.Sequence.OnEnd = EndSwing;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 在开始swing动画之后，我们还需要给 <see cref="SimpleEventReceiver"/> 回调,我们希望它在接收到名为“Event”的动画事件时触发这个回调.
        /// <para></para>
        /// 在这种情况下,由于只有一个具有该事件的动画,我们可以在<see cref="Awake"/>中注册它,而无需将它绑定到特定的状态,但是通常情况下
        /// <see cref="SimpleEventReceiver"/> 的关键是允许多个脚本为它们正在播放的任何动画注册它们自己的回调.
        /// </summary>
        protected override void StartSwing()
        {
            base.StartSwing();

            var state = _Animancer.States.Current;

            // 当函数名为“Event”的动画事件发生时:
            // 如果swing动画没有具有该函数名的事件,则会记录一个警告.
            _EventReceiver.onEvent.Set(state, (animationEvent) => HitBall());
        }

        /************************************************************************************************************************/
    }
}
