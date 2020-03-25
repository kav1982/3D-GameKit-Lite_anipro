// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Creatures
{
    /// <summary>
    /// 一个 <see cref="Creature"/> 的状态,只播放一个动画.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Creatures - Creature State")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Creatures/CreatureState")]
    public sealed class CreatureState : StateBehaviour<CreatureState>
    {
        /************************************************************************************************************************/

        [SerializeField] private Creature _Creature;
        [SerializeField] private AnimationClip _Animation;

        /************************************************************************************************************************/

        /// <summary>
        /// 播放动画，如果没有循环，则返回 <see cref="Creature"/> 到待机状态.
        /// </summary>
        private void OnEnable()
        {
            var state = _Creature.Animancer.Play(_Animation, 0.25f);
            if (!_Animation.isLooping)
                state.Events.OnEnd = _Creature.ForceIdleState;
        }

        /************************************************************************************************************************/
    }
}
