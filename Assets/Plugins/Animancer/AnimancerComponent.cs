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
        /// The <see cref="UnityEngine.Animator"/> 此脚本控制的组件
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
        /// 如果已初始化，则销毁播放模式的数据。
        /// Searches for an <see cref="UnityEngine.Animator"/> on this object, or it's children or parents.
        /// Removes the <see cref="Animator.runtimeAnimatorController"/> if it finds one.
        /// <para></para>
        /// This method also prevents you from adding multiple copies of this component to a single object. Doing so
        /// will destroy the new one immediately and change the old one's type to match the new one, allowing you to
        /// change the type without losing the values of any serialized fields they share.
        /// </summary>
        protected virtual void Reset()
        {
            OnDestroy();

            _Animator = Editor.AnimancerEditorUtilities.GetComponentInHierarchy<Animator>(gameObject);

            if (_Animator != null)
            {
                _Animator.runtimeAnimatorController = null;
                Editor.AnimancerEditorUtilities.SetIsInspectorExpanded(_Animator, false);

                // Collapse the Animator property because the custom Inspector uses that to control whether the
                // Animator's Inspector is expanded.
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
        /// Called by Unity when this component becomes enabled and active.
        /// <para></para>
        /// Ensures that the <see cref="PlayableGraph"/> is playing.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (IsPlayableInitialised)
                _Playable.UnpauseGraph();
        }

        /// <summary>
        /// Called by Unity when this component becomes disabled or inactive. Acts according to the
        /// <see cref="ActionOnDisable"/>.
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

        /// <summary>Creates a new <see cref="AnimancerPlayable"/> if it doesn't already exist.</summary>
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
        /// Called by Unity when this component is destroyed.
        /// Ensures that the <see cref="Playable"/> is properly cleaned up.
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
        /// Ensures that the <see cref="PlayableGraph"/> is destroyed.
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
        #region Play Management
        /************************************************************************************************************************/

        /// <summary>
        /// Returns the `clip` itself. This method is used to determine the dictionary key to use for an animation
        /// when none is specified by the user, such as in <see cref="Play(AnimationClip)"/>. It can be overridden by
        /// child classes to use something else as the key.
        /// </summary>
        public virtual object GetKey(AnimationClip clip)
        {
            return clip;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Stops all other animations, plays the `clip`, and returns its state.
        /// <para></para>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(clip, layerIndex).Time = 0;</c>.
        /// </summary>
        public AnimancerState Play(AnimationClip clip)
        {
            return Play(States.GetOrCreate(clip));
        }

        /// <summary>
        /// Stops all other animations, plays the `state`, and returns it.
        /// <para></para>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(state).Time = 0;</c>.
        /// </summary>
        public AnimancerState Play(AnimancerState state)
        {
            return Playable.Play(state);
        }

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on <see cref="ITransition.CrossFadeFromStart"/>.
        /// </summary>
        public AnimancerState Play(ITransition transition)
        {
            return Playable.Play(transition);
        }

        /// <summary>
        /// Stops all other animations, plays the animation registered with the `key`, and returns that
        /// state. If no state is registered with the `key`, this method does nothing and returns null.
        /// <para></para>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(key).Time = 0;</c>.
        /// </summary>
        public AnimancerState Play(object key)
        {
            return Playable.Play(key);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Starts fading in the `clip` over the course of the `fadeDuration` while fading out all others in the same
        /// layer. Returns its state.
        /// <para></para>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play(AnimationClip)"/> the `clip`.
        /// <para></para>
        /// Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in a runtime build.
        /// </summary>
        public AnimancerState Play(AnimationClip clip, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            return Play(States.GetOrCreate(clip), fadeDuration, mode);
        }

        /// <summary>
        /// Starts fading in the `state` over the course of the `fadeDuration` while fading out all others in the same
        /// layer. Returns the `state`.
        /// <para></para>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play(AnimancerState)"/> the `state`.
        /// <para></para>
        /// Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in a runtime build.
        /// </summary>
        public AnimancerState Play(AnimancerState state, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            return Playable.Play(state, fadeDuration, mode);
        }

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on <see cref="ITransition.CrossFadeFromStart"/>.
        /// </summary>
        public AnimancerState Play(ITransition transition, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            return Playable.Play(transition, fadeDuration, mode);
        }

        /// <summary>
        /// Starts fading in the animation registered with the `key` over the course of the `fadeDuration` while fading
        /// out all others in the same layer. Returns the animation's state (or null if none was registered).
        /// <para></para>
        /// If the state was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play(AnimancerState)"/> the state.
        /// <para></para>
        /// Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in a runtime build.
        /// </summary>
        public AnimancerState Play(object key, float fadeDuration, FadeMode mode = FadeMode.FixedSpeed)
        {
            return Playable.Play(key, fadeDuration, mode);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the state associated with the `clip`, stops and rewinds it to the start, then returns it.
        /// </summary>
        public AnimancerState Stop(AnimationClip clip)
        {
            return Stop(GetKey(clip));
        }

        /// <summary>
        /// Gets the state registered with the <see cref="IHasKey.Key"/>, stops and rewinds it to the start, then
        /// returns it.
        /// </summary>
        public AnimancerState Stop(IHasKey hasKey)
        {
            if (_Playable != null)
                return _Playable.Stop(hasKey);
            else
                return null;
        }

        /// <summary>
        /// Gets the state associated with the `key`, stops and rewinds it to the start, then returns it.
        /// </summary>
        public AnimancerState Stop(object key)
        {
            if (_Playable != null)
                return _Playable.Stop(key);
            else
                return null;
        }

        /// <summary>
        /// Stops all animations and rewinds them to the start.
        /// </summary>
        public void Stop()
        {
            if (_Playable != null)
                _Playable.Stop();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if a state is registered for the `clip` and it is currently playing.
        /// <para></para>
        /// The actual dictionary key is determined using <see cref="GetKey"/>.
        /// </summary>
        public bool IsPlaying(AnimationClip clip)
        {
            return IsPlaying(GetKey(clip));
        }

        /// <summary>
        /// Returns true if a state is registered with the <see cref="IHasKey.Key"/> and it is currently playing.
        /// </summary>
        public bool IsPlaying(IHasKey hasKey)
        {
            if (_Playable != null)
                return _Playable.IsPlaying(hasKey);
            else
                return false;
        }

        /// <summary>
        /// Returns true if a state is registered with the `key` and it is currently playing.
        /// </summary>
        public bool IsPlaying(object key)
        {
            if (_Playable != null)
                return _Playable.IsPlaying(key);
            else
                return false;
        }

        /// <summary>
        /// Returns true if at least one animation is being played.
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
        /// Returns true if the `clip` is currently being played by at least one state.
        /// <para></para>
        /// This method is inefficient because it searches through every state to find any that are playing the `clip`,
        /// unlike <see cref="IsPlaying(AnimationClip)"/> which only checks the state registered using the `clip`s key.
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
        /// Evaluates all of the currently playing animations to apply their states to the animated objects.
        /// </summary>
        public void Evaluate()
        {
            Playable.Evaluate();
        }

        /// <summary>
        /// Advances all currently playing animations by the specified amount of time (in seconds) and evaluates the
        /// graph to apply their states to the animated objects.
        /// </summary>
        public void Evaluate(float deltaTime)
        {
            Playable.Evaluate(deltaTime);
        }

        /************************************************************************************************************************/
        #region Key Error Methods
#if UNITY_EDITOR
        /************************************************************************************************************************/
        // These are overloads of other methods that take a System.Object key to ensure the user doesn't try to use an
        // AnimancerState as a key, since the whole point of a key is to identify a state in the first place.
        /************************************************************************************************************************/

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just call <see cref="AnimancerState.Stop"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just call AnimancerState.Stop().", true)]
        public AnimancerState Stop(AnimancerState key)
        {
            key.Stop();
            return key;
        }

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just check <see cref="AnimancerState.IsPlaying"/>.
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
        #region Enumeration
        /************************************************************************************************************************/
        // IEnumerable for 'foreach' statements.
        /************************************************************************************************************************/

        /// <summary>
        /// Returns an enumerator that will iterate through all states in each layer (not states inside mixers).
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
        // IEnumerator for yielding in a coroutine to wait until all animations have stopped.
        /************************************************************************************************************************/

        /// <summary>
        /// Determines if any animations are still playing so this object can be used as a custom yield instruction.
        /// </summary>
        bool IEnumerator.MoveNext()
        {
            if (!IsPlayableInitialised)
                return false;

            return ((IEnumerator)_Playable).MoveNext();
        }

        /// <summary>Returns null.</summary>
        object IEnumerator.Current { get { return null; } }

#pragma warning disable UNT0006 // Incorrect message signature.
        /// <summary>Does nothing.</summary>
        void IEnumerator.Reset() { }
#pragma warning restore UNT0006 // Incorrect message signature.

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
        /// Gathers all the animations in the <see cref="Playable"/>.
        /// <para></para>
        /// In the Unity Editor this method also gathers animations from other components on parent and child objects.
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
