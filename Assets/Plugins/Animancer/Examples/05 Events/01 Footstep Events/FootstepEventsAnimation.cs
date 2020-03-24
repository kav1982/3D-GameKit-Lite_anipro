// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Events
{
    /// <summary>
    /// 一种基于 <see cref="FootstepEvents"/> 的变体，它通过播放从数组中随机选择的声音来响应动画事件，称为“Footstep”.
    /// 使用事件的Int参数作为索引来确定在哪个脚上播放声音。0是左脚，1是右脚.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Footstep Events - Animation")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.AnimationEvents/FootstepEventsAnimation")]
    public sealed class FootstepEventsAnimation : FootstepEvents
    {
        /************************************************************************************************************************/

        [SerializeField] private AudioSource[] _FootSources;

        /************************************************************************************************************************/

        // 由动画事件调用.
        private void Footstep(int foot)
        {
            PlaySound(_FootSources[foot]);
        }

        /************************************************************************************************************************/
    }
}
