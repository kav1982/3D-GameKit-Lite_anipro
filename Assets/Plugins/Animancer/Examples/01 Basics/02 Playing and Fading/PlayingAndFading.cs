// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽Unity对于字段从未被赋值，并且始终具有其默认值的警告

using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>
    /// 演示动画之间各种播放方式和淡入淡出之间的区别
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Basics - Playing and Fading")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Basics/PlayingAndFading")]

    public sealed class PlayingAndFading : MonoBehaviour //sealed 最终类，不能被继承
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private AnimationClip _Idle;
        [SerializeField] private AnimationClip _Action;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Animancer.Play(_Idle);
        }

        /************************************************************************************************************************/

        // Called by a UI Button.
        public void Play()
        {
            // 立即从前一个姿势切换到动作开始。

            _Animancer.Play(_Action);

            //如果动作已经开始播放，则从当前时间开始播放。

            // 动画将定格在最后的poss
        }

        // Called by a UI Button.
        public void PlayThenIdle()
        {
            //播放，动作完成后返回待机动画

            _Animancer.Play(_Action).Events.OnEnd = () => _Animancer.Play(_Idle);

            //这里我们使用一个“Lambda表达式”来声明当前方法中的回调方法

            //在这种情况下，我们可以将OnEnable分配给事件，因为它做的是相同的事情:

            // _Animancer.Play(_Action).Events.OnEnd = OnEnable;

            //但是这并不总是那么方便，而且“OnEnable”也并不是一个真正合适的名称

            //我们可以使用这样的多行:

            // var state = _Animancer.Play(_Action);
            // state.Events.OnEnd = () => _Animancer.Play(_Idle);

            //但是因为我们只想对状态做一件事，所以我们只能在一行上做。

            //注意，当播放新动画时，所有事件都会自动清除。

            //这样就保证了上面的Play方法不用担心其他的方法

            //可以设置自己的事件。

        }

        // Called by a UI Button.
        public void PlayFromStart()
        {
            //播放时每一次都是从头开始，而不允许从当前的时间继续

            _Animancer.Play(_Action).Time = 0;
        }

        // Called by a UI Button.
        public void PlayFromStartThenIdle()
        {
            //以上两者结合。

            var state = _Animancer.Play(_Action);
            state.Time = 0;
            state.Events.OnEnd = () => _Animancer.Play(_Idle);
            //state.Events.OnEnd = public void xxx() => _Animancer.Play(_Idel);
        }

        /************************************************************************************************************************/

        // Called by a UI Button.
        public void CrossFade()
        {
            //在0.25秒内平稳过渡到其它动作

            _Animancer.Play(_Action, 0.25f);
        }

        // Called by a UI Button.
        public void CrossFadeThenIdle()
        {
            //和PlayThenIdle是一样的，但是因为队列变长了，我们可以把它分开
            //注意第一行末尾没有分号

            _Animancer.Play(_Action, 0.25f)
                .Events.OnEnd = () => _Animancer.Play(_Idle, 0.25f);
        }

        // Called by a UI Button.
        public void BadCrossFadeFromStart()
        {
            //与PlayFromStart不同的是，交叉淡出时设置时间不太好，因为这样会妨碍平滑
            //从以前的姿势过渡到新的动画

            _Animancer.Play(_Action, 0.25f).Time = 0;
        }

        // Called by a UI Button.
        public void GoodCrossFadeFromStart()
        {
            //相反，我们可以从开始就使用FadeMode，以确保它是顺利的
            //查看FadeMode的文档。FromStart详情。

            _Animancer.Play(_Action, 0.25f, FadeMode.FromStart);
        }

        // Called by a UI Button.
        public void GoodCrossFadeFromStartThenIdle()
        {
            //将以上所有整合起来

            _Animancer.Play(_Action, 0.25f, FadeMode.FromStart)
                .Events.OnEnd = () => _Animancer.Play(_Idle, 0.25f);
        }

        /************************************************************************************************************************/
    }
}
