// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告.

using UnityEngine;

namespace Animancer.Examples.DirectionalSprites
{
    /// <summary>
    /// 使用<see cref="DirectionalAnimationSet"/>s中定义的动画使角色处于空闲或行走状态.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Directional Sprites - Sprite Movement Controller")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.DirectionalSprites/SpriteMovementController")]
    public sealed class SpriteMovementController : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer; 
        [SerializeField] private DirectionalAnimationSet _Idles; //定向动画设置 待机
        [SerializeField] private DirectionalAnimationSet _Walks; //定向动画设置 行走
        [SerializeField] private Vector2 _Facing = Vector2.down;

        /************************************************************************************************************************/

        private void Awake()
        {
            Play(_Idles);
        }

        /************************************************************************************************************************/

        private void Update()
        {
            // WASD Controls.根据移动的水平或者垂直状态判定是否播放行走动画
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input != Vector2.zero)
            {
                _Facing = input;

                Play(_Walks);

                // Play 可以返回从 _Animancer.Play 获取的AnimancerState,
                // 但是我们也可以使用 _Animancer.States.Current 来访问它

                var isRunning = Input.GetButton("Fire3");// 左 Shift 按键.
                _Animancer.States.Current.Speed = isRunning ? 2 : 1;
            }
            else
            {
                // 当我们不动的时候,仍然还记得我们面朝的方向,我们可以继续使用正确的空闲动画方向.
                Play(_Idles);
            }
        }

        /************************************************************************************************************************/

        private void Play(DirectionalAnimationSet animations)
        {
            // 装载每一个单独的动画时, 我们有不同的动画对应每个面朝的方向.
            // 所以我们选择适合那个方向的，然后播放它.

            var clip = animations.GetClip(_Facing);
            _Animancer.Play(clip);

            // 或者我们可以在一行中这样做: _Animancer.Play(animations.GetClip(_Facing));
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// 每当加载该脚本的实例或在检查器中更改值时，在编辑模式下由Unity Editor引导.
        /// <para></para>
        /// 在编辑模式下设置角色的初始脚本，这样你可以在场景中工作时看到它.
        /// </summary>
        private void OnValidate()
        {
            if (_Idles == null)
                return;

            AnimancerUtilities.EditModePlay(_Animancer, _Idles.GetClip(_Facing), true);
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
