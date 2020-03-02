// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告.

using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// 继承了 <see cref="IdleAndWalk"/> 并且增加了跑步的能力.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Locomotion - Idle And Walk And Run")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Locomotion/IdleAndWalkAndRun")]
    public class IdleAndWalkAndRun : IdleAndWalk
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimationClip _Run;

        /************************************************************************************************************************/

        protected override void PlayMove()
        {
            // 我们将播放Walk或Run动画.

            // 我们需要知道哪些动画我们正在尝试播放,哪些是其它的.
            AnimationClip playAnimation, otherAnimation;

            if (Input.GetButton("Fire3"))// Left Shift 默认按键
            {
                playAnimation = _Run;
                otherAnimation = Walk;
            }
            else
            {
                playAnimation = Walk;
                otherAnimation = _Run;
            }

            // 播放我们想要的那个.
            var playState = Animancer.Play(playAnimation, 0.25f);

            // 如果另一个动画仍在淡出，调整它们的归一化时间，以确保它们在播放周期中保持相同的相对进展.
            var otherState = Animancer.States[otherAnimation];
            if (otherState != null && otherState.IsPlaying)
                playState.NormalizedTime = otherState.NormalizedTime;
        }

        /************************************************************************************************************************/
    }
}
