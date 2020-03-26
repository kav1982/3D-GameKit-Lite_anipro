// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0618 // 屏蔽类型或成员已过时(对于Animancer Lite中的Animancer事件)的警告.
#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告.

using UnityEngine;

namespace Animancer.Examples.Events
{
    /// <summary>
    /// 一个 <see cref="GolfHitController"/> 它使用了一个Animancer事件,在检查器中设置了它的时间,但是它的回调是空的,
    /// 所以它可以被代码赋值(一个检查器和基于代码的系统之间的“混合”).
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Golf Events - Animancer Hybrid")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.AnimationEvents/GolfHitControllerAnimancerHybrid")]
    public sealed class GolfHitControllerAnimancerHybrid : GolfHitController // 高尔夫击球控制器动画混合
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Calls the base <see cref="GolfHitController.Awake"/> method and register
        /// <see cref="GolfHitController.EndSwing"/> to be called whenever the swing animation ends.
        /// <para></para>
        /// The <see cref="GolfHitController._Swing"/> transition has its End Time set so that it will execute the
        /// registered method at some point during the animation, but its End Callback was left blank so it can be
        /// assigned here.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            Debug.Assert(_Swing.Events.Sequence.Count == 1, "Expected one event for hitting the ball", this);
            _Swing.Events.Sequence.Set(0, HitBall);

            // 如果我们没有在检查器中创建事件，我们可以添加到这里:
            //_Swing.Events.Sequence.Add(new AnimancerEvent(0.375f, OnHitBall));

            _Swing.Events.Sequence.OnEnd = EndSwing;
        }

        /************************************************************************************************************************/
    }
}
