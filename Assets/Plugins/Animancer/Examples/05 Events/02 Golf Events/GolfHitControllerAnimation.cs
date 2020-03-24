// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Events
{
    /// <summary>
    /// 使用动画事件的 <see cref="GolfHitController"/> 
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Golf Events - Animation")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.AnimationEvents/GolfHitControllerAnimation")]
    public sealed class GolfHitControllerAnimation : GolfHitController
    {
        /************************************************************************************************************************/

        /// <summary>
        /// 调用基本的 <see cref="GolfHitController.Awake"/> 方法，并在swing动画结束时调用寄存器 <see cref="GolfHitController.EndSwing"/> 
        /// <para></para>
        /// 通常,Animancer可以在转换中定义的结束时间调用注册的方法,但是在这种情况下,与此脚本一起使用的有一个函数名为"End"的动画事件,
        /// 当该事件时间过去时,它将执行注册的方法.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _Swing.Events.Sequence.OnEnd = EndSwing;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 调用 <see cref="GolfHitController.HitBall"/>. 与此脚本一起使用的 <see cref="AnimationClip"/> 有一个函数名为“Event”的动画事件,这将使它执行此方法.
        /// <para></para>
        /// 通常情况下,事件直接使用“HitBall”作为它的函数名,但是同样的动画也用于 <see cref="GolfHitControllerAnimationSimple"/> 它依赖于函数名为“event”.
        /// </summary>
        private void Event()
        {
            HitBall();
        }

        /************************************************************************************************************************/
    }
}
