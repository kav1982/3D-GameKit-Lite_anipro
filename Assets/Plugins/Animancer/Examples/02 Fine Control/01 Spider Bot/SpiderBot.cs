// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽Unity对于字段从未被赋值，并且始终具有其默认值的警告

using UnityEngine;

namespace Animancer.Examples.FineControl
{
    /// <summary>
    /// 演示如何播放一个单一的“唤醒”动画,向前播放"激活"和向后播放回到"休眠"
    /// <para></para>
    /// 这是一个抽象类，它由 <see cref="SpiderBotSimple"/> 和 <see cref="Locomotion.SpiderBotAdvanced"/>继承, 
    /// 这意味着您不能将这个脚本附加到一个对象上(因为它本身是无用的)，并且这两个脚本可以共享它的功能，
    /// 而不需要将相同的方法复制到每个对象中
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Fine Control - Spider Bot")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.FineControl/SpiderBot")]
    public abstract class SpiderBot : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private AnimancerComponent _Animancer;
        public AnimancerComponent Animancer { get { return _Animancer; } }

        [SerializeField] private ClipState.Transition _WakeUp;
        [SerializeField] private ClipState.Transition _Sleep;

        private bool _WasMoving;

        /************************************************************************************************************************/

        protected abstract bool IsMoving { get; } //抽象方法的接口1

        protected abstract ITransition MovementAnimation { get; } //抽象方法的接口2

        /************************************************************************************************************************/

        protected virtual void Awake()
        {
            // Start在动画开始时暂停。
            _Animancer.Play(_WakeUp);
            _Animancer.Evaluate();
            _Animancer.Playable.PauseGraph();

            // 在这里初始化OnEnd事件，这样我们就不会每次使用它们时都分配垃圾。
            _WakeUp.Events.Sequence.OnEnd = () => _Animancer.Play(MovementAnimation);
            _Sleep.Events.Sequence.OnEnd = _Animancer.Playable.PauseGraph;
        }

        /************************************************************************************************************************/

        protected virtual void Update()
        {
            if (IsMoving)
            {
                if (!_WasMoving)
                {
                    _WasMoving = true;

                    // 确保图形没有暂停(因为我们在返回休眠状态时会暂停它)。
                    _Animancer.Playable.UnpauseGraph();
                    _Animancer.Play(_WakeUp);
                }
            }
            else
            {
                if (_WasMoving)
                {
                    _WasMoving = false;

                    var state = _Animancer.Play(_Sleep);

                    // 如果它已经超过了最后一帧，那就跳到最后一帧，因为它正在回放,
                    // 否则就只能从当前时间倒放。
                    if (state.NormalizedTime > 1)
                        state.NormalizedTime = 1;

                    // 如果我们没有在Awake中初始化OnEnd事件，我们可以在这里设置它:
                    // state.OnEnd = _Animancer.Playable.PauseGraph;
                }
            }
        }

        /************************************************************************************************************************/
    }
}
