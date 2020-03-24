// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.Events
{
    /// <summary>
    /// 使用动画事件播放从数组中随机选择的声音.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Footstep Events - Animancer")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.AnimationEvents/FootstepEvents")]
    public class FootstepEvents : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private ClipState.Transition _Walk;
        [SerializeField] private AudioClip[] _Sounds;

        /************************************************************************************************************************/

        protected void OnEnable()
        {
            _Animancer.Play(_Walk);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 被Animancer事件调用.在指定的“源”上播放随机声音(因为每只脚都有自己的<see cref="AudioSource"/>).
        /// </summary>
        public void PlaySound(AudioSource source)
        {
            source.clip = _Sounds[Random.Range(0, _Sounds.Length)];
            source.Play();

            // 注意，在随机的最小值。Range是包容性的(因此它可以选择0)，而最大值是排他的(因此它不能选择' _Sounds.Length ').
            // 这对于选择随机数组元素来说是完美的.

            // 一个更复杂的系统可能会有不同的脚步声，这取决于被踩的表面.
            // 这可以通过从脚开始的光线投射来实现，并基于 3D Game Kit 示例中所演示的地面渲染器的共享材质来决定使用哪一种声音.
            // 甚至可以通过一个简单的脚本来保存一个enum来指示类型。
        }

        /************************************************************************************************************************/
    }
}
