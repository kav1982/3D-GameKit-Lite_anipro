// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 停止系统关于Field从来没有被赋值，但是有它的默认值的警告

using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>
    ///从一个空闲动画开始，并在用户单击鼠标时执行操作，然后返回到空闲状态
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Basics - Play Animation On Click")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Basics/PlayAnimationOnClick")]
    public sealed class PlayAnimationOnClick : MonoBehaviour
    {
        /************************************************************************************************************************/

        //如果没有Animancer，你可以引用Animator组件来控制动画
        //但是对于Animancer，你引用的是一个Animancer组建
        [SerializeField] private AnimancerComponent _Animancer;

        //如果没有Animancer，你可以创建一个Animator控制器来定义动画状态和过渡
        //但是使用Animancer，你可以直接引用你想要播放的AnimationClips
        [SerializeField] private AnimationClip _Idle;
        [SerializeField] private AnimationClip _Action;

        /************************************************************************************************************************/

        /// <summary>
        /// 在一开始的时候播放idle动画.
        /// </summary>
        private void OnEnable()
        {
            // 启动时播放待机动画
            _Animancer.Play(_Idle);
        }

        /************************************************************************************************************************/

        private void Update()
        {
            //每次更新时，检查用户是否单击了鼠标左键(鼠标按钮0)
            if (Input.GetMouseButtonDown(0))
            {
                // 点击鼠标左键播放指定的动画
                var state = _Animancer.Play(_Action);

                // Play方法返回AnimancerState，它管理动画，所以你可以访问和控制各种细节，例如:
                // state.Time = 1; //在动画开始后跳过一秒
                // state.NormalizedTime = 0.5f; //动画开始后跳过动画的一半
                // state.Speed = 2; //以两倍的速度播放动画

                //在本例中，我们只想让它在动画结束时调用OnActionEnd方法
                state.Events.OnEnd = OnActionEnd;
            }
        }

        /************************************************************************************************************************/

        private void OnActionEnd()
        {
            //现在动作已经完成，回到idle状态。但不是立即切换到新的动画，设置它在0.25秒内逐渐淡出，以便过渡平稳
            _Animancer.Play(_Idle, 0.25f);
        }

        /************************************************************************************************************************/
    }
}
