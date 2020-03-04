// Animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>在启动时播放单个 <see cref="AnimationClip"/> .</summary>
    [AddComponentMenu(Strings.MenuPrefix + "Solo Animation")]
    [HelpURL(Strings.APIDocumentationURL + "/SoloAnimation")]
    [DefaultExecutionOrder(-5000)]//在使用此组件之前进行初始化
    public sealed class SoloAnimation : MonoBehaviour, IAnimationClipSource
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        [SerializeField, Tooltip("这个脚本控制的Animator组件")]
        private Animator _Animator;

        /// <summary>[<see cref="SerializeField"/>]
        /// 这个脚本控制的组件<see cref="UnityEngine.Animator"/> 
        /// <para></para>
        /// 如果你需要在运行时设置这个值，你最好使用合适的 <see cref="AnimancerComponent"/>.
        /// </summary>
        public Animator Animator
        {
            get { return _Animator; }
            set
            {
                _Animator = value;
                Awake();
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Tooltip("将由OnEnable播放的动画片段")]
        private AnimationClip _Clip;

        /// <summary>[<see cref="SerializeField"/>]
        /// <see cref="AnimationClip"/> 将由 <see cref="OnEnable"/> 播放.
        /// <para></para>
        /// 如果您需要在运行时设置此值，您最好使用适当的 <see cref="AnimancerComponent"/>.
        /// </summary>
        public AnimationClip Clip
        {
            get { return _Clip; }
            set
            {
                _Clip = value;
                Awake();
            }
        }

        /************************************************************************************************************************/

#if UNITY_2018_1_OR_NEWER
        /// <summary>
        /// 如果为 true，禁用此对象将停止并重新播放动画.否则它将被暂停,并在重新启用时从当前状态恢复.
        /// <para></para>
        /// 默认值为true。
        /// <para></para>
        /// 此属性封装在 <see cref="Animator.keepAnimatorControllerStateOnDisable"/> 并且反转它的值
        /// 该值由 <see cref="UnityEngine.Animator"/>序列化.
        /// <para></para>
        /// 需要 Unity 2018.1 或者更新的版本.
        /// </summary>
        public bool StopOnDisable
        {
            get { return !_Animator.keepAnimatorControllerStateOnDisable; }
            set { _Animator.keepAnimatorControllerStateOnDisable = !value; }
        }
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// <see cref="PlayableGraph"/> 被用来播放 <see cref="Clip"/>.
        /// </summary>
        private PlayableGraph _Graph;

        /// <summary>
        /// <see cref="AnimationClipPlayable"/> 被用来播放 <see cref="Clip"/>.
        /// </summary>
        private AnimationClipPlayable _Playable;

        /************************************************************************************************************************/

        private bool _IsPlaying;

        /// <summary>
        /// 指示动画是否正在播放(true)或暂停(false).
        /// </summary>
        public bool IsPlaying
        {
            get { return _IsPlaying; }
            set
            {
                _IsPlaying = value;

                if (!IsInitialised)
                    return;

                if (value)
                    _Graph.Play();
                else
                    _Graph.Stop();
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Tooltip("动画播放的速度(默认1)")]
        private float _Speed = 1;

        /// <summary>[<see cref="SerializeField"/>]
        /// 动画播放的速度(默认1)
        /// </summary>
        /// <exception cref="ArgumentException">被抛出, 如果该组件尚未 <see cref="Awake"/>.</exception>
        public float Speed
        {
            get { return _Speed; }
            set
            {
                _Speed = value;
                _Playable.SetSpeed(value);
                IsPlaying = value != 0;
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Tooltip("Determines whether Foot IK will be applied to the model (if it is Humanoid)")]
        private bool _FootIK;

        /// <summary>[<see cref="SerializeField"/>]
        /// 决定脚IK是否将应用于模型(如果它是类人的).
        /// <para></para>
        /// Unity的开发者表示，他们相信在启用了这个功能后，它看起来会更好，但通常情况下，它只是让腿摆出了与动画师想要的稍微不同的姿势.
        /// </summary>
        /// <exception cref="ArgumentException">被抛出,如果这个组件还没有 <see cref="Awake"/>.</exception>
        public bool FootIK
        {
            get { return _FootIK; }
            set
            {
                _FootIK = value;
                _Playable.SetApplyFootIK(value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 动画开始后经过的秒数.
        /// </summary>
        /// <exception cref="ArgumentException">被抛出,如果这个组件还没有 <see cref="Awake"/>.</exception>
        public float Time
        {
            get { return (float)_Playable.GetTime(); }
            set
            {
                // 我们需要调用两次SetTime以确保动画事件正确地触发.
                _Playable.SetTime(value);
                _Playable.SetTime(value);

                IsPlaying = true;
            }
        }

        /// <summary>
        /// 该状态的 <see cref="Time"/> 是 <see cref="AnimationClip.length"/>的一部分, 这意味着该值在从头到尾播放时从0到1，不管实际需要多长时间.
        /// <para></para>
        /// 这个值将在动画经过其长度的末尾之后继续增加，并且根据其是否循环，其将冻结在适当的位置或从头开始重新开始.
        /// <para></para>
        /// 数值的小数部分(NormalizedTime % 1)是当前循环的进度百分比(0-1)，整数部分((int)NormalizedTime)是动画循环的次数.
        /// </summary>
         /// <exception cref="ArgumentException">被抛出,如果这个组件还没有 <see cref="Awake"/>.</exception>
        public float NormalizedTime
        {
            get { return Time / _Clip.length; }
            set { Time = value * _Clip.length; }
        }

        /************************************************************************************************************************/

        /// <summary>指示 <see cref="PlayableGraph"/> 是否有效</summary>
        public bool IsInitialised { get { return _Graph.IsValid(); } }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// 当这个组件第一次被添加时(在编辑模式下)，以及当重置命令从它的上下文菜单中执行时，由Unity编辑器调用.
        /// <para></para>
        /// 试图在这个 <see cref="GameObject"/> 上找到一个 <see cref="UnityEngine.Animator"/> 组件或它的子组件或父组件(按这个顺序).
        /// </summary>
        private void Reset()
        {
            _Animator = Editor.AnimancerEditorUtilities.GetComponentInHierarchy<Animator>(gameObject);
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// 当在检查器中加载此脚本的实例或更改值时，由Unity编辑器在编辑模式中调用.
        /// <para></para>
        /// 尝试在此 <see cref="GameObject"/> 或者它的父子层级上查找 <see cref="UnityEngine.Animator"/> (按照这个顺序).
        /// </summary>
        private void OnValidate()
        {
            if (IsInitialised)
            {
                Speed = Speed;
                FootIK = FootIK;
            }
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>
        /// 当这个组件第一次创建时被Unity调用.
        /// <para></para>
        /// 初始化播放 <see cref="Clip"/>所需的内容.
        /// </summary>
        private void Awake()
        {
            if (_Clip == null || _Animator == null)
                return;

            if (_Graph.IsValid())
                _Graph.Destroy();

            _Playable = AnimationPlayableUtilities.PlayClip(_Animator, _Clip, out _Graph);

            _Playable.SetSpeed(_Speed);

            if (!_FootIK)
                _Playable.SetApplyFootIK(false);

            if (!_Clip.isLooping)
                _Playable.SetDuration(_Clip.length);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 当此组件启用并激活时，由Unity调用.
        /// <para></para>
        /// 在目标 <see cref="Animator"/>上播放 <see cref="Clip"/>.
        /// </summary>
        private void OnEnable()
        {
            IsPlaying = true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 当这个组件被激活时，Unity会在每一帧调用它.
        /// <para></para>
        /// 检查动画是否完成，这样它可以暂停 <see cref="PlayableGraph"/> 来提高性能.
        /// </summary>
        private void Update()
        {
            if (!IsPlaying)
                return;

            if (_Graph.IsDone())
            {
                IsPlaying = false;
            }
            else if (_Speed < 0 && Time <= 0)
            {
                IsPlaying = false;
                Time = 0;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 当此组件被禁用或不活动时，由Unity调用.
        /// <para></para>
        /// 确保 <see cref="_Graph"/> 被正确清理.
        /// </summary>
        private void OnDisable()
        {
            IsPlaying = false;

#if UNITY_2018_1_OR_NEWER
            if (_Animator.keepAnimatorControllerStateOnDisable)
                return;
#endif

            if (IsInitialised)
            {
                // 我们需要调用两次SetTime以确保动画事件正确地触发.
                _Playable.SetTime(0);
                _Playable.SetTime(0);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 当这个组件被破坏时被Unity调用.
        /// <para></para>
        /// 确保 <see cref="PlayableGraph"/> 被正确的清理.
        /// </summary>
        private void OnDestroy()
        {
            if (IsInitialised)
                _Graph.Destroy();
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// 确保 <see cref="PlayableGraph"/> 被正确的清理.
        /// </summary>
        ~SoloAnimation()
        {
            UnityEditor.EditorApplication.delayCall += OnDestroy;
        }
#endif

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipSource"/>]
        /// 把 <see cref="Clip"/> 添加到列表.
        /// </summary>
        public void GetAnimationClips(List<AnimationClip> clips) //获取动画片段
        {
            if (_Clip != null)
                clips.Add(_Clip);
        }

        /************************************************************************************************************************/
    }
}

/************************************************************************************************************************/
#if UNITY_EDITOR
/************************************************************************************************************************/

namespace Animancer.Editor
{
    [UnityEditor.CustomEditor(typeof(SoloAnimation)), UnityEditor.CanEditMultipleObjects]
    internal sealed class SoloAnimationEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        /// <summary>被每个目标引用的animator.</summary>
        private Animator[] _Animators;

        /// <summary>一个 <see cref="UnityEditor.SerializedObject"/> 封装了 <see cref="_Animators"/>.</summary>
        private UnityEditor.SerializedObject _SerializedAnimator;

#if UNITY_2018_1_OR_NEWER
        /// <summary> <see cref="Animator.keepAnimatorControllerStateOnDisable"/> 属性.</summary>
        private UnityEditor.SerializedProperty _KeepStateOnDisable;
#endif

        /************************************************************************************************************************/

        public override void OnInspectorGUI()
        {
            DoSerializedFieldsGUI();
            RefreshSerializedAnimator();
            DoStopOnDisableGUI();
            DoRuntimeDetailsGUI();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 绘制目标的序列化字段.
        /// </summary>
        private void DoSerializedFieldsGUI()
        {
            serializedObject.Update();

            var property = serializedObject.GetIterator();

            property.NextVisible(true);

            if (property.name != "m_Script")
                UnityEditor.EditorGUILayout.PropertyField(property, true);

            while (property.NextVisible(false))
            {
                UnityEditor.EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /************************************************************************************************************************/

        private void RefreshSerializedAnimator() //刷新序列化的动画控制器
        {
            var targets = this.targets;

            if (_Animators == null || _Animators.Length != targets.Length)
                _Animators = new Animator[targets.Length];

            var dirty = false;
            var hasAll = true;

            for (int i = 0; i < _Animators.Length; i++)
            {
                var animator = (targets[i] as SoloAnimation).Animator;
                if (_Animators[i] != animator)
                {
                    _Animators[i] = animator;
                    dirty = true;
                }

                if (animator == null)
                    hasAll = false;
            }

            if (!dirty)
                return;

            OnDisable();

            if (!hasAll)
                return;

            _SerializedAnimator = new UnityEditor.SerializedObject(_Animators);
#if UNITY_2018_1_OR_NEWER
            _KeepStateOnDisable = _SerializedAnimator.FindProperty("m_KeepAnimatorControllerStateOnDisable");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 从 <see cref="Animator.keepAnimatorControllerStateOnDisable"/> 字段中反向绘制一个切换.
        /// </summary>
        private void DoStopOnDisableGUI()
        {
#if UNITY_2018_1_OR_NEWER
            var area = AnimancerGUI.LayoutSingleLineRect();

            var label = AnimancerGUI.TempContent("Stop On Disable",
                " 如果为真，禁用此对象将停止并回滚所有动画." +
                " 否则，它们将被暂停，并在重新启用时从当前状态继续.");

            if (_KeepStateOnDisable != null)
            {
                _KeepStateOnDisable.serializedObject.Update();

                label = UnityEditor.EditorGUI.BeginProperty(area, label, _KeepStateOnDisable);

                _KeepStateOnDisable.boolValue = !UnityEditor.EditorGUI.Toggle(area, label, !_KeepStateOnDisable.boolValue);

                UnityEditor.EditorGUI.EndProperty();

                _KeepStateOnDisable.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var enabled = GUI.enabled;
                GUI.enabled = false;
                UnityEditor.EditorGUI.Toggle(area, label, false);
                GUI.enabled = enabled;
            }
#endif
        }

        /************************************************************************************************************************/

        /// <summary> 绘制目标的运行时详细信息. </summary>
        private void DoRuntimeDetailsGUI()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ||
                targets.Length != 1)
                return;

            AnimancerGUI.BeginVerticalBox(GUI.skin.box);

            var target = (SoloAnimation)this.target;
            if (!target.IsInitialised)
            {
                GUILayout.Label("Not Initialised");
            }
            else
            {
                UnityEditor.EditorGUI.BeginChangeCheck();
                var isPlaying = UnityEditor.EditorGUILayout.Toggle("Is Playing", target.IsPlaying);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.IsPlaying = isPlaying;

                UnityEditor.EditorGUI.BeginChangeCheck();
                var time = UnityEditor.EditorGUILayout.FloatField("Time", target.Time);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.Time = time;

                time = target.NormalizedTime.Wrap01();
                if (time == 0 && target.Time != 0)
                    time = 1;

                UnityEditor.EditorGUI.BeginChangeCheck();
                time = UnityEditor.EditorGUILayout.Slider("Normalized Time", time, 0, 1);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.NormalizedTime = time;
            }

            AnimancerGUI.EndVerticalBox(GUI.skin.box);
            Repaint();
        }

        /************************************************************************************************************************/

        private void OnDisable()
        {
            if (_SerializedAnimator != null)
            {
                _SerializedAnimator.Dispose();
                _SerializedAnimator = null;
#if UNITY_2018_1_OR_NEWER
                _KeepStateOnDisable = null;
#endif
            }
        }

        /************************************************************************************************************************/
    }
}

/************************************************************************************************************************/
#endif
/************************************************************************************************************************/

