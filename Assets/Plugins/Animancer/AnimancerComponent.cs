// Animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// 其他脚本可以通过主要组件与<see cref="Animancer"/>进行交互. 它允许你
    /// 驱动一个动画 <see cref="UnityEngine.Animator"/> 在不使用<see cref="RuntimeAnimatorController"/>自带状态机的情况下.
    /// <para></para>
    /// 这个类可以作为一个自定义的携程指令来等待，直到所有的动画完成播放
    /// </summary>
    /// <remarks>
    /// 这个类主要是一个包装器,它将 <see cref="AnimancerPlayable"/> 连接到 <see cref="UnityEngine.Animator"/>.
    /// </remarks>
    [AddComponentMenu(Strings.MenuPrefix + "Animancer Component")]
    [HelpURL(Strings.APIDocumentationURL + "/AnimancerComponent")]
    [DefaultExecutionOrder(-5000)] //在其他组件尝试使用此组件之前进行初始化
    public class AnimancerComponent : MonoBehaviour,
        IAnimancerComponent, IEnumerable<AnimancerState>, IEnumerator, IAnimationClipSource, IAnimationClipCollection //AnimancerState:动画状态
    {
        /************************************************************************************************************************/
        #region Fields and Properties //区域字段和属性
        /************************************************************************************************************************/

        [SerializeField, Tooltip("The Animator component which this script controls")]//这个脚本控制的Animator组件
        private Animator _Animator;

        /// <summary>[<see cref="SerializeField"/>]
        /// <see cref="UnityEngine.Animator"/> 此脚本控制的组件
        /// </summary>
        public Animator Animator
        {
            get { return _Animator; }
            set
            {
#if UNITY_EDITOR
                Editor.AnimancerEditorUtilities.SetIsInspectorExpanded(_Animator, true);
                //Animancer编辑工具.设置为展开检查器.
                Editor.AnimancerEditorUtilities.SetIsInspectorExpanded(value, false);
#endif


                _Animator = value;
                if (IsPlayableInitialised)//指示是否可以初始化
                    _Playable.SetOutput(value, this);
            }
        }

#if UNITY_EDITOR
        /// <summary>[编辑模式下有效] 序列化支持字段的名称为 <see cref="Animator"/> 属性</summary>
        string IAnimancerComponent.AnimatorFieldName { get { return "_Animator"; } }
#endif

        /************************************************************************************************************************/

        private AnimancerPlayable _Playable;

        /// <summary>
        ///管理播放动画的内部系统
        ///访问此属性将自动初始化它
        /// </summary>
        public AnimancerPlayable Playable
        {
            get
            {
                InitialisePlayable();
                return _Playable;
            }
        }

        /// <summary>指示 <see cref="Playable"/> 是否已经初始化.</summary>
        public bool IsPlayableInitialised { get { return _Playable != null && _Playable.IsValid; } }

        /************************************************************************************************************************/

        /// <summary> 由该组件管理的状态 </summary>
        public AnimancerPlayable.StateDictionary States { get { return Playable.States; } } //状态机

        /// <summary> 每个层管理自己的一套动画 </summary>
        public AnimancerPlayable.LayerList Layers { get { return Playable.Layers; } } //动画层级

        /// <summary>返回0层级</summary>
        public static implicit operator AnimancerLayer(AnimancerComponent animancer) //隐式操作返回0层级
        {
            return animancer.Playable.Layers[0];
        }

        /************************************************************************************************************************/
        //当这个组件被禁用或者它的游戏对象变的不再活跃
        [SerializeField, Tooltip("Determines what happens when this component is disabled" +
            " or its GameObject becomes inactive (i.e. in OnDisable):" +
            "\n- Stop all animations" +
            "\n- Pause all animations" +
            "\n- Continue playing" +
            "\n- Reset to the original values" +
            "\n- Destroy all layers and states")]
        private DisableAction _ActionOnDisable;

        /// <summary>[<see cref="SerializeField"/>]
        /// 确定当此组件被禁用或 <see cref="GameObject"/> 变为非活动状态时会发生什么
        /// (i.e. in <see cref="OnDisable"/>).
        /// <para></para>
        /// 默认值为 <see cref="DisableAction.Stop"/>.
        /// </summary>
        public DisableAction ActionOnDisable
        {
            get { return _ActionOnDisable; }
            set { _ActionOnDisable = value; }
        }

        /// <summary>确定禁用对象时是否将重置为其原始值</summary>
        bool IAnimancerComponent.ResetOnDisable { get { return _ActionOnDisable == DisableAction.Reset; } }

        /// <summary>
        /// 当禁用 <see cref="AnimancerComponent"/>时要执行的操作,主要看<see cref="ActionOnDisable"/>.
        /// </summary>
        public enum DisableAction
        {
            /// <summary>
            /// 停止所有的动画并回滚它们，但是保留所有的动画值 (不同于<see cref="Reset"/>).
            /// <para></para>
            /// 调用 <see cref="Stop()"/> 和<see cref="AnimancerPlayable.PauseGraph"/>.
            /// </summary>
            Stop,

            /// <summary>
            /// 暂停当前状态的所有动画，以便稍后继续
            /// <para></para>
            /// Calls <see cref="AnimancerPlayable.PauseGraph"/>.
            /// </summary>
            Pause,

            /// <summary>不在活动状态时继续播放</summary>
            Continue,

            /// <summary>
            /// 停止所有的动画，倒播它们，迫使对象回到它的原始状态(通常称为绑定姿态)
            /// <para></para>
            /// 警告:这必须在 <see cref="UnityEngine.Animator"/> 接收到它的OnDisable调用之前发生，
            /// 这意味着<see cref="AnimancerComponent"/> 必须在检查器或子对象的上面，以便 <see cref="OnDisable"/> 首先被调用
            /// <para></para>
            /// 调用 <see cref="Stop()"/>, <see cref="Animator.Rebind"/>, 和 <see cref="AnimancerPlayable.PauseGraph"/>.
            /// </summary>
            Reset,

            /// <summary>
            /// 销毁及其所有层和状态。这意味着其他脚本引用的任何层或状态将不再有效，因此如果您想再次使用该对象，则需要重新创建它们
            /// <para></para>
            /// Calls <see cref="AnimancerPlayable.Destroy()"/>.
            /// </summary>
            Destroy,
        }

        /************************************************************************************************************************/
        #region Update Mode
        /************************************************************************************************************************/

        /// <summary>
        /// 确定何时更新动画以及使用哪个时间源。这个属性主要是对 <see cref="Animator.updateMode"/> 的封装
        /// <para></para>
        /// 注意再运行时,对 <see cref="AnimatorUpdateMode.AnimatePhysics"/> 读写是没有效果的
        /// </summary>
        /// <exception cref="NullReferenceException">抛出异常,如果<see cref="Animator"/> 没有被分配</exception>
        public AnimatorUpdateMode UpdateMode
        {
            get { return _Animator.updateMode; }
            set
            {
                _Animator.updateMode = value;

                if (!IsPlayableInitialised)
                    return;

                //当使用Playables API时，动画器上的UnscaledTime实际上和普通的是一样的，
                //所以我们需要设置图形的DirectorUpdateMode来确定它是如何得到时间增量的
                _Playable.UpdateMode = value == AnimatorUpdateMode.UnscaledTime ?
                    DirectorUpdateMode.UnscaledGameTime :
                    DirectorUpdateMode.GameTime;

#if UNITY_EDITOR
                if (InitialUpdateMode == null)
                {
                    InitialUpdateMode = value;
                }
                else if (UnityEditor.EditorApplication.isPlaying)
                {
                    if (AnimancerPlayable.HasChangedToOrFromAnimatePhysics(InitialUpdateMode, value))
                        Debug.LogWarning("Changing the Animator.updateMode to or from AnimatePhysics at runtime will have no effect." +
                            " You must set it in the Unity Editor or on startup.");
                }
#endif
            }
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// <see cref="UpdateMode"/> 是此脚本初始化时首次使用的内容
        /// 当使用 <see cref="AnimatorUpdateMode.AnimatePhysics"/> 或者是从它获取信息时,它会给出一个警告，因为它不能正常工作。
        /// </summary>
        public AnimatorUpdateMode? InitialUpdateMode { get; private set; }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Animation Events //动画事件
        /************************************************************************************************************************/
        //这些方法超出了常规的重载，所以动画事件优先调用它们(因为其他方法不能使用)
        /************************************************************************************************************************/

        /// <summary>调用 <see cref="Play(AnimationClip, int)"/>.</summary>
        /// <remarks>此方法由动画事件来调用.</remarks>
        private void Play(AnimationEvent animationEvent)
        {
            var clip = (AnimationClip)animationEvent.objectReferenceParameter;//对象引用参数
            var layerIndex = animationEvent.intParameter;
            if (layerIndex < 0)//LayerIndex动画层级
                Play(clip);
            else
                Layers[layerIndex].Play(clip);
        }

        /// <summary>
        /// 调用 <see cref="Play(AnimationClip, int)"/> 设置 <see cref="AnimancerState.Time"/> = 0.
        /// </summary>
        /// <remarks>此方法由动画事件来调用.</remarks>
        private void PlayFromStart(AnimationEvent animationEvent)
        {
            var clip = (AnimationClip)animationEvent.objectReferenceParameter;
            var layerIndex = animationEvent.intParameter;
            if (layerIndex < 0)
                Play(clip).Time = 0;
            else
                Layers[layerIndex].Play(clip).Time = 0;
        }

        /// <summary>调用 <see cref="CrossFade(AnimationClip, float, int)"/>.</summary>
        /// <remarks>此方法由动画事件来调用.</remarks>
        private void CrossFade(AnimationEvent animationEvent)
        {
            var clip = (AnimationClip)animationEvent.objectReferenceParameter;

            var fadeDuration = animationEvent.floatParameter;
            if (fadeDuration <= 0)
                fadeDuration = AnimancerPlayable.DefaultFadeDuration;

            var layerIndex = animationEvent.intParameter;
            if (layerIndex < 0)
                Play(clip, fadeDuration);
            else
                Layers[layerIndex].Play(clip, fadeDuration);
        }

        /// <summary>调用 <see cref="CrossFadeFromStart(AnimationClip, float, int)"/>.</summary>
        /// <remarks>此方法由动画事件来调用.</remarks>
        private void CrossFadeFromStart(AnimationEvent animationEvent)
        {
            var clip = (AnimationClip)animationEvent.objectReferenceParameter;

            var fadeDuration = animationEvent.floatParameter;//fade Duration 淡入淡出的持续时间
            if (fadeDuration <= 0)
                fadeDuration = AnimancerPlayable.DefaultFadeDuration;

            var layerIndex = animationEvent.intParameter;
            if (layerIndex < 0)
                Play(clip, fadeDuration, FadeMode.FromStart);
            else
                Layers[layerIndex].Play(clip, fadeDuration, FadeMode.FromStart);
        }

        /// <summary>调用 <see cref="Transition(ITransition, int)"/>.</summary>
        /// <remarks>此方法由动画事件来调用.</remarks>
        private void Transition(AnimationEvent animationEvent)
        {
            var transition = (ITransition)animationEvent.objectReferenceParameter;
            var layerIndex = animationEvent.intParameter;
            if (layerIndex < 0)
                Play(transition);
            else
                Layers[layerIndex].Play(transition);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 如果播放指向事件的 <see cref="AnimationClip"/> ,就在 <see cref="CurrentState"/>中调用 <see cref="Animancerstate.Events.OnEndCallback"/> 
        /// <para></para>
        /// 如果没有为该动画注册状态，则记录一个警告
        /// </summary>
        /// <remarks>此方法由动画事件来调用.</remarks>
        private void End(AnimationEvent animationEvent)
        {
            if (_Playable == null)
            {
                //只有当另一个动画器以某种方式触发该对象上的事件时，才会发生这种情况
                Debug.LogWarning("AnimationEvent 'End' was triggered by " + animationEvent.animatorClipInfo.clip +
                    ", but the AnimancerComponent.Playable hasn't been initialised.",
                    this);
                return;
            }

            if (_Playable.OnEndEventReceived(animationEvent))
                return;

            if (animationEvent.messageOptions == SendMessageOptions.RequireReceiver)
            {
                Debug.LogWarning("AnimationEvent 'End' was triggered by " + animationEvent.animatorClipInfo.clip +
                    ", but no state was found with that key.",
                    this);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialisation //初始
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[仅在编辑模式]
        /// 当这个组件第一次被添加时(在编辑模式下)，以及当重置命令从它的上下文菜单中执行时，由Unity编辑器调用
        /// <para></para>
        /// 如果已初始化，则销毁播放模式的数据
        /// 在此对象或其子对象或父对象上搜索<see cref="UnityEngine.Animator"/> 
        /// 删除 <see cref="Animator.runtimeAnimatorController"/> 如果找到的话
        /// <para></para>
        /// 此方法还可以防止将此组件的多个副本添加到单个对象。这样做将立即销毁新字段并更改旧字段的类型以匹配新字段，
        /// 从而允许您更改类型而不会丢失它们共享的任何序列化字段的值.
        /// </summary>
        protected virtual void Reset()
        {
            OnDestroy();

            _Animator = Editor.AnimancerEditorUtilities.GetComponentInHierarchy<Animator>(gameObject);//获取层次结构中的组件

            if (_Animator != null)
            {
                _Animator.runtimeAnimatorController = null;
                Editor.AnimancerEditorUtilities.SetIsInspectorExpanded(_Animator, false);

                // 折叠Animator属性，因为自定义检查器使用它来控制是否展开Animator的检查器
                using (var serializedObject = new UnityEditor.SerializedObject(this))
                {
                    var property = serializedObject.FindProperty("_Animator");
                    property.isExpanded = false;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            AnimancerUtilities.IfMultiComponentThenChangeType(this);
        }
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// 当此组件启用并激活时，由Unity调用
        /// <para></para>
        /// 确保 <see cref="PlayableGraph"/> 在运行状态
        /// </summary>
        protected virtual void OnEnable()
        {
            if (IsPlayableInitialised)
                _Playable.UnpauseGraph();
        }

        /// <summary>
        /// 当此组件被禁用或不活动时. 由Unity根据<see cref="ActionOnDisable"/>调用
        /// </summary>
        protected virtual void OnDisable()
        {
            if (!IsPlayableInitialised)
                return;

            switch (_ActionOnDisable)
            {
                case DisableAction.Stop:
                    Stop();
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Pause:
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Continue:
                    break;

                case DisableAction.Reset:
                    Debug.Assert(_Animator.isActiveAndEnabled,
                        "DisableAction.Reset failed because the Animator is not enabled." +
                        " This most likely means you are disabling the GameObject and the Animator is above the" +
                        " AnimancerComponent in the Inspector so it got disabled right before this method was called." +
                        " See the Inspector of " + this + " to fix the issue or use DisableAction.Stop and call Animator.Rebind" +
                        " manually before disabling the GameObject.",
                        this);

                    Stop();
                    _Animator.Rebind();
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Destroy:
                    _Playable.Destroy();
                    _Playable = null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("ActionOnDisable");
            }
        }

        /************************************************************************************************************************/

        /// <summary>创建一个新的<see cref="AnimancerPlayable"/> 如果它不存在.</summary>
        private void InitialisePlayable()
        {
            if (IsPlayableInitialised)
                return;

#if UNITY_EDITOR
            var currentEvent = Event.current;
            if (currentEvent != null && (currentEvent.type == EventType.Layout || currentEvent.type == EventType.Repaint))
                Debug.LogWarning("Creating an AnimancerPlayable during a " + currentEvent.type + " event is likely undesirable.");
#endif

            if (_Animator == null)
                _Animator = GetComponent<Animator>();

            AnimancerPlayable.SetNextGraphName(name + ".Animancer");
            _Playable = AnimancerPlayable.Create();
            _Playable.SetOutput(_Animator, this);

#if UNITY_EDITOR
            if (_Animator != null)
                InitialUpdateMode = UpdateMode;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 当这个组件被销毁时被Unity调用
        /// 确保<see cref="Playable"/>被正确的清理
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (IsPlayableInitialised)
            {
                _Playable.Destroy();
                _Playable = null;
            }
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// 确保<see cref="PlayableGraph"/>被销毁
        /// </summary>
        ~AnimancerComponent()
        {
            if (_Playable != null)
                Editor.AnimancerEditorUtilities.EditModeDelayCall(OnDestroy);
        }
#endif

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Play Management //播放管理
        /************************************************************************************************************************/

        /// <summary>
        /// 返回“clip”本身。此方法用于确定当用户未指定动画时,如<see cref="Play(AnimationClip)"/>使用的字典key.
        /// 它可以被子类覆盖，以使用其他东西作为key.
        /// </summary>
        public virtual object GetKey(AnimationClip clip)
        {
            return clip;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 停止所有其他动画，播放“clip”，并返回其状态
        /// <para></para>
        /// 动画将从当前的 <see cref="AnimancerState.Time"/> 继续播放
        /// 要从头开始启动它,可以使用 <c>...播放(clip, layerIndex).Time = 0;</c>.
        /// </summary>
        public AnimancerState Play(AnimationClip clip)
        {
            return Play(States.GetOrCreate(clip));
        }

        /// <summary>
        /// 停止所有其他动画，播放“state”，并返回它
        /// <para></para>
        /// 动画将从当前的 <see cref="AnimancerState.Time"/>继续播放
        /// 要从头开始启动它,可以使用 <c>...Play(state).Time = 0;</c>.
        /// </summary>
        public AnimancerState Play(AnimancerState state)
        {
            return Playable.Play(state);
        }

        /// <summary>
        /// 为`transition` 创建一个状态(如果它不存在),然后调用
        /// <see cref="Play(AnimancerState)"/> 或 <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// 取决于<see cref="ITransition.CrossFadeFromStart"/>.
        /// </summary>
        public AnimancerState Play(ITransition transition)
        {
            return Playable.Play(transition);
        }

        /// <summary>
        /// 停止所有其他动画，播放用'key'注册的动画，并返回该动画状态。
        /// 如果没有向'key'注册任何状态，则此方法不执行任何操作并返回null。
        /// <para></para>
        /// 动画将从当前的 <see cref="AnimancerState.Time"/>继续播放
        /// 要从头开始启动它,可以使用 <c>...Play(state).Time = 0;</c>.
        /// </summary>
        public AnimancerState Play(object key)
        {
            return Playable.Play(key);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 在fadeDuration过程中，开始淡出剪辑，同时淡出同一层中的所有其它元素,返回它的状态
        /// <para></para>
        /// 如果`state` 并且淡出的时间比 `fadeDuration`少, 这种方法将允许它完成现有的淡出，而不是开始一个较慢的淡出。
        /// <para></para>
        /// 如果当前图层有0 <see cref="AnimancerNode.Weight"/>权重, 这个方法将在层本身中消失，并简单地<see cref="AnimancerState.Play(AnimationClip)"/> 片段.
        /// <para></para>
        /// Animancer Lite在运行时版本中只允许默认的“fadeDuration”(0.25秒)
        /// </summary>
        public AnimancerState Play(AnimationClip clip, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            return Play(States.GetOrCreate(clip), fadeDuration, mode);
        }

        /// <summary>
        /// 在“fadeDuration”的过程中，开始淡出“state”，同时淡出同一层中的其他所有对象。返回“状态”
        /// <para></para>
        /// 如果“状态”已经在播放并且淡出的时间比“淡出”的时间少，这种方法将允许它完成现有的淡出，而不是开始一个较慢的淡出
        /// <para></para>
        /// 如果当前层的<see cref="AnimancerNode.Weight"/>是0, 则该方法将在层本身中淡出，
        /// 并简单地 <see cref="AnimancerState.Play(AnimancerState)"/> `state`.
        /// <para></para>
        /// 在运行时版本中，nimancer Lite只允许默认的“fadeDuration”(0.25秒)
        /// </summary>
        public AnimancerState Play(AnimancerState state, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            return Playable.Play(state, fadeDuration, mode);
        }

        /// <summary>
        /// 为`transition`创建一个状态(如果它还不存在)，然后调用
        /// <see cref="Play(AnimancerState)"/> 或者 <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// 依赖于 <see cref="ITransition.CrossFadeFromStart"/>.
        /// </summary>
        public AnimancerState Play(ITransition transition, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            return Playable.Play(transition, fadeDuration, mode);
        }

        /// <summary>
        /// 在' fadeDuration '过程中，在' key '注册的动画中开始淡出，同时淡出同一层中的所有其他对象。
        /// 返回动画的状态(如果没有注册，则返回null)。
        /// <para></para>
        /// 如果状态已经在播放，并且淡出的时间比fadeDuration剩下的时间更少，
        /// 那么这个方法将允许它完成现有的fade而不是开始一个更慢的fade
        /// <para></para>
        /// 如果该层的<see cref="AnimancerNode.Weight"/>是0, 这个方法会在层中自己淡出
        /// 简单的播放 <see cref="AnimancerState.Play(AnimancerState)"/> 状态.
        /// <para></para>
        /// Animancer Lite在运行时版本中只允许默认的“fadeDuration”(0.25秒)。
        /// </summary>
        public AnimancerState Play(object key, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            return Playable.Play(key, fadeDuration, mode);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 获取与“clip”关联的状态，停止并将其重绕到开始处，然后返回
        /// </summary>
        public AnimancerState Stop(AnimationClip clip)
        {
            return Stop(GetKey(clip));
        }

        /// <summary>
        /// 获取使用<see cref="IHasKey.Key"/>注册的状态，停止并回滚到开始，然后返回
        /// </summary>
        public AnimancerState Stop(IHasKey hasKey)
        {
            if (_Playable != null)
                return _Playable.Stop(hasKey);
            else
                return null;
        }

        /// <summary>
        /// 获取与“key”关联的状态，停止并将其重绕到开始处，然后返回
        /// </summary>
        public AnimancerState Stop(object key)
        {
            if (_Playable != null)
                return _Playable.Stop(key);
            else
                return null;
        }

        /// <summary>
        /// 停止所有的动画并将它们倒回开始
        /// </summary>
        public void Stop()
        {
            if (_Playable != null)
                _Playable.Stop();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 如果为“clip”注册了状态，并且当前正在播放，则返回true.
        /// <para></para>
        /// 实际的字典key是用<see cref="GetKey"/>确定的
        /// </summary>
        public bool IsPlaying(AnimationClip clip)
        {
            return IsPlaying(GetKey(clip));
        }

        /// <summary>
        /// 如果状态是用<see cref="IHasKey.Key"/>注册的,并且当前正在播放,则返回true.
        /// </summary>
        public bool IsPlaying(IHasKey hasKey)
        {
            if (_Playable != null)
                return _Playable.IsPlaying(hasKey);
            else
                return false;
        }

        /// <summary>
        /// 如果状态是用“key”注册的,并且当前正在播放,则返回true.
        /// </summary>
        public bool IsPlaying(object key)
        {
            if (_Playable != null)
                return _Playable.IsPlaying(key);
            else
                return false;
        }

        /// <summary>
        /// 如果正在播放至少一个动画，则返回true.
        /// </summary>
        public bool IsPlaying()
        {
            if (_Playable != null)
                return _Playable.IsPlaying();
            else
                return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 如果' clip '当前由至少一个状态播放，则返回true.
        /// <para></para>
        /// 这个方法效率很低，因为它会搜索每个状态来找到任何正在播放“clip”的状态,
        /// 不像 <see cref="IsPlaying(AnimationClip)"/> 只检查状态注册使用`clip`的key.
        /// </summary>
        public bool IsPlayingClip(AnimationClip clip)
        {
            if (_Playable != null)
                return _Playable.IsPlayingClip(clip);
            else
                return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 计算所有当前正在播放的动画，以将它们的状态应用于动画对象.
        /// </summary>
        public void Evaluate()
        {
            Playable.Evaluate();
        }

        /// <summary>
        /// 按照指定的时间(秒)推进当前正在播放的所有动画，并计算图形以将其状态应用于动画对象.
        /// </summary>
        public void Evaluate(float deltaTime)
        {
            Playable.Evaluate(deltaTime);
        }

        /************************************************************************************************************************/
        #region Key Error Methods
#if UNITY_EDITOR
        /************************************************************************************************************************/
        // 这些是占用系统的其他方法的重载对象key，以确保用户不会尝试使用AnimancerState作为key，因为key的关键是首先标识一个状态.
        /************************************************************************************************************************/

        /// <summary>[Warning]
        /// 你不能使用 <see cref="AnimancerState"/> 作为一个key.
        /// 使用 <see cref="AnimancerState.Stop"/>即可.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just call AnimancerState.Stop().", true)]
        public AnimancerState Stop(AnimancerState key)
        {
            key.Stop();
            return key;
        }

        /// <summary>[Warning]
        /// 你不能使用一个 <see cref="AnimancerState"/> 作为一个 key.
        /// 只需检查 <see cref="AnimancerState.IsPlaying"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just check AnimancerState.IsPlaying.", true)]
        public bool IsPlaying(AnimancerState key)
        {
            return key.IsPlaying;
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Enumeration //枚举和协程
        /************************************************************************************************************************/
        // IEnumerable for 'foreach'语句.
        /************************************************************************************************************************/

        /// <summary>
        /// 返回一个枚举器，它将遍历每一层中的所有状态(而不是混合器中的状态).
        /// </summary>
        public IEnumerator<AnimancerState> GetEnumerator()
        {
            if (!IsPlayableInitialised)
                yield break;

            foreach (var state in _Playable.Layers.GetAllStateEnumerable())
                yield return state;
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /************************************************************************************************************************/
        // 在协同程序中生成的IEnumerator，等待所有的动画停止.
        /************************************************************************************************************************/

        /// <summary>
        /// 确定是否有动画仍在播放，因此此对象可以用作自定义的yield指令.
        /// </summary>
        bool IEnumerator.MoveNext()
        {
            if (!IsPlayableInitialised)
                return false;

            return ((IEnumerator)_Playable).MoveNext();
        }

        /// <summary>返回 null.</summary>
        object IEnumerator.Current { get { return null; } }

#pragma warning disable UNT0006 // 错误的消息签名.
        /// <summary>什么也不做.</summary>
        void IEnumerator.Reset() { }
#pragma warning restore UNT0006 // 错误的消息签名.

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipSource"/>]
        /// Calls <see cref="GatherAnimationClips(ICollection{AnimationClip})"/>.
        /// </summary>
        public void GetAnimationClips(List<AnimationClip> clips)
        {
            var set = ObjectPool.AcquireSet<AnimationClip>();
            set.UnionWith(clips);

            GatherAnimationClips(set);

            clips.Clear();
            clips.AddRange(set);

            ObjectPool.Release(set);
        }

        /// <summary>[<see cref="IAnimationClipCollection"/>]
        /// 在<see cref="Playable"/>中收集所有的动画.
        /// <para></para>
        /// 在Unity编辑器中，这个方法也从父对象和子对象的其他组件中收集动画.
        /// </summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            if (IsPlayableInitialised)
                _Playable.GatherAnimationClips(clips);

#if UNITY_EDITOR
            Editor.AnimationGatherer.GatherFromGameObject(gameObject, clips);

            if (_Animator != null && _Animator.gameObject != gameObject)
                Editor.AnimationGatherer.GatherFromGameObject(_Animator.gameObject, clips);
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}
