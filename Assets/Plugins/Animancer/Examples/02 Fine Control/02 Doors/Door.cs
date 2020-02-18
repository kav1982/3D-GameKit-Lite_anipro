// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽Unity对于字段从未被赋值，并且始终具有其默认值的警告

using UnityEngine;

namespace Animancer.Examples.FineControl
{
    /// <summary>
    /// <see cref="IInteractable"/> 当有东西与门相互作用时，门在打开和关闭之间切换
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Fine Control - Door")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.FineControl/Door")]
    [SelectionBase]
    public sealed class Door : MonoBehaviour, IInteractable
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private AnimationClip _Open;

        [SerializeField, Range(0, 1)]
        private float _Openness;

        /************************************************************************************************************************/

        private void Awake()
        {
            // 应用启动状态并暂停渲染
            var state = _Animancer.Play(_Open);
            state.NormalizedTime = _Openness;
            _Animancer.Evaluate();
            _Animancer.Playable.PauseGraph();

            // 并在动画结束时暂停它以保存当前状态
            state.Events.OnEnd = _Animancer.Playable.PauseGraph;

            // 通常情况下，当我们播放一个新的动画时，OnEnd事件会被清除，但是因为在这个例子中只有一个动画，所以我们让它继续播放并暂停/取消暂停图形。
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IInteractable"/>]
        /// 这个接口是门打开和关闭的开关
        /// </summary>
        public void Interact()
        {
            // 让状态设置它打开关闭的速度(或者我们可以保持唤醒状态)
            var state = _Animancer.States[_Open];

            // 如果当前处于关闭状态，则向前播放动画
            if (_Openness < 0.5f)
            {
                state.Speed = 1;
                _Openness = 1;
            }
            else// 否则倒过来播放
            {
                state.Speed = -1;
                _Openness = 0;
            }

            // 确认渲染是处在播放状态的
            _Animancer.Playable.UnpauseGraph();
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// 每当加载该脚本的实例或在检查器中更改值时，在编辑模式下由Unity Editor引导
        /// <para></para>
        /// 在编辑模式中将初始开放值应用于门
        /// </summary>
        private void OnValidate()
        {
            if (_Animancer == null || _Open == null)
                return;

            // 延迟一帧,否则Unity会在重新编译脚本后给出一个错误
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (_Animancer == null || _Open == null)
                    return;

                Awake();
            };
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
