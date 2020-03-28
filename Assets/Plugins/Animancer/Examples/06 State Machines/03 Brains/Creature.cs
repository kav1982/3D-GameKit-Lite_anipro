// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告.

using Animancer.FSM;
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Brains
{
    /// <summary>
    /// 集中引用一个角色的公共部分和它们的动作状态机.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Brains - Creature")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Brains/Creature")]
    [DefaultExecutionOrder(-5000)]// 尽早初始化状态机.
    public sealed class Creature : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private AnimancerComponent _Animancer;
        public AnimancerComponent Animancer { get { return _Animancer; } }

        [SerializeField]
        private CreatureState _Idle;
        public CreatureState Idle { get { return _Idle; } }

        [SerializeField]
        private Rigidbody _Rigidbody;
        public Rigidbody Rigidbody { get { return _Rigidbody; } }

        [SerializeField]
        private CreatureBrain _Brain;
        public CreatureBrain Brain
        {
            get { return _Brain; }
            set
            {
                // 更多的 Brains示例在运行时使用这种方法在Brains之间进行交换.

                if (_Brain == value)
                    return;

                var oldBrain = _Brain;
                _Brain = value;

                // 确保老的 brain 不会仍然引用这个角色.
                if (oldBrain != null)
                    oldBrain.Creature = null;

                // 给新的 brain 一个关于此角色的引用.
                if (value != null)
                    value.Creature = this;
            }
        }

        [SerializeField]
        private CreatureStats _Stats;
        public CreatureStats Stats { get { return _Stats; } }

        // 地面检测.
        // 体力和法力.
        // 寻路.
        // Etc.
        // 任何与大多数角色相同的东西.

        /************************************************************************************************************************/

        /// <summary>
        /// 管理该角色的动作的有限状态机.
        /// </summary>
        public StateMachine<CreatureState> StateMachine { get; private set; }

        /// <summary>
        /// 强制 <see cref="StateMachine"/> 返回 <see cref="Idle"/> 状态.
        /// </summary>
        public Action ForceEnterIdleState { get; private set; }

        /************************************************************************************************************************/

        private void Awake()
        {
            // 注意，这个类有一个[DefaultExecutionOrder]属性，以确保这个方法在任何其他可能想要访问它的组件之前运行.

            ForceEnterIdleState = () => StateMachine.ForceSetState(_Idle);

            StateMachine = new StateMachine<CreatureState>(_Idle);
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Inspector gadget 专业版在绘制常规Inspector GUI后调用此方法,允许此脚本在播放模式中显示其当前状态.
        /// </summary>
        /// <remarks>
        /// Inspector gadget Pro允许您轻松地自定义Inspector,而无需编写完整的自定义Inspector类,只需添加一个具有此名称的方法即可. 
        /// 没有 Inspector Gadgets, 这个方法什么也做不了,可以从 https://kybernetik.com.au/inspector-gadgets/pro 购买此工具.
        /// </remarks>
        private void AfterInspectorGUI()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                var enabled = GUI.enabled;
                GUI.enabled = false;
                UnityEditor.EditorGUILayout.ObjectField("Current State", StateMachine.CurrentState, typeof(CreatureState), true);
                GUI.enabled = enabled;
            }
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
