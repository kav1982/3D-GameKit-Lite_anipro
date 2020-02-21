// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽Unity对于字段从未被赋值，并且始终具有其默认值的警告

using System;
using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>
    /// 演示如何使用<see cref="NamedAnimancerComponent"/> 按名称播放动画
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Basics - Named Animations")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Basics/NamedAnimations")]
    public sealed class NamedAnimations : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private NamedAnimancerComponent _Animancer;
        [SerializeField] private AnimationClip _Walk;
        [SerializeField] private AnimationClip _Run;

        /************************************************************************************************************************/
        // Idle.
        /************************************************************************************************************************/

        // Called by a UI Button.
        /// <summary>
        /// 按名称播放idle动画。要求动画在 <see cref="NamedAnimancerComponent"/> 中已经有一个状态, 在本例中已经通过将其添加到检查器中的
        /// <see cref="NamedAnimancerComponent.Animations"/> 列表来完成
        /// <para></para>
        ///如果没有添加它,这个方法将什么也不做
        /// </summary>
        public void PlayIdle()
        {
            _Animancer.Play("Humanoid-Idle");
        }

        /************************************************************************************************************************/
        // Walk.
        /************************************************************************************************************************/

        // Called by a UI Button.
        /// <summary>
        ///按名称播放walk动画。与idle动画不同的是，这个动画没有被添加到Inspector列表中，所以它不存在，
        ///除非你调用它，否则这个方法会提示一条消息:
        /// <see cref="InitialiseWalkState"/> first.
        /// </summary>
        public void PlayWalk()
        {
            var state = _Animancer.Play("Humanoid-Walk");
            if (state == null)
            {
                Debug.Log("No state called 'Humanoid-Walk' exists yet." +
                    " Click 'Initialise Walk State' to create it then try again.", this);
            }

            // _Animancer.Play(_Walk.name); 也会生效,
            // 但是如果我们要使用clip我们应该使用 _Animancer.Play(_Walk);
        }

        /************************************************************************************************************************/

        // Called by a UI Button.
        /// <summary>
        /// Creates a state for the walk animation so that <see cref="PlayWalk"/> can play it.
        /// </summary>
        /// <remarks>
        /// //多次调用该方法将抛出 <see cref="ArgumentException(参数异常)"/> 因为一个状态已经存在，它试图使用的key(动画名)已经存在
        /// <para></para>
        /// 如果我们想允许重复调用，我们可以使用 <see cref="AnimancerLayer.GetOrCreateState(AnimationClip, bool)"/> 来代替，
        /// 这将在第一次创建一个状态，然后在以后每次都返回相同的状态。
        /// <para></para>
        ///如果我们想要为同一个动画创建多个状态，我们必须使用可选的‘key’参数来为每个状态指定不同的键。
        /// </remarks>
        public void InitialiseWalkState()
        {
            _Animancer.States.Create(_Walk);
            Debug.Log("Created a state to play " + _Walk, this);
        }

        /************************************************************************************************************************/
        // Run.
        /************************************************************************************************************************/

        // Called by a UI Button.
        /// <summary>
        /// 使用直接引用来播放动画,显示这种在 <see cref="NamedAnimancerComponent"/> 中按名称播放的能力,
        /// 并不会影响它直接引用基类 <see cref="AnimancerComponent"/>.
        /// </summary>
        public void PlayRun()
        {
            _Animancer.Play(_Run);

            // 内部实际发生的事情看起来更像这样:

            // object key = _Animancer.GetKey(_Run);
            // var state = _Animancer.GetOrCreate(key, _Run);
            // _Animancer.Play(state);

           
            //基类AnimancerComponent.GetKey返回AnimationClip作为它自己的键，但是NamedAnimancerComponent会覆盖它来返回剪辑的名字
            //这样做的效率稍低一些，但它允许我们交替地使用剪辑(like we are here)或名称(like with the idle)
            //在“Run”状态创建之后，我们可以执行以下操作:
            // _Animancer.GetState(_Run) or GetState("Run").
            // _Animancer.Play(_Run) or Play("Run").
            // Same for CrossFade, and CrossFadeFromStart.
        }

        /************************************************************************************************************************/
    }
}
