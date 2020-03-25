// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Creatures
{
    /// <summary>
    /// 对角色和状态机的公共部分的操作的集中引用组.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Creatures - Creature")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Creatures/Creature")]
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

        // Rigidbody.
        // Ground Detector.
        // Stats.
        // Health and Mana.
        // Pathfinding.
        // Etc.
        // Anything common to most creatures.

        /************************************************************************************************************************/

        /// <summary>
        /// 管理该角色的动作的有限状态机.
        /// </summary>
        public StateMachine<CreatureState> StateMachine { get; private set; }

        /// <summary>
        /// 促使 <see cref="StateMachine"/> 返回到 <see cref="Idle"/> 状态.
        /// </summary>
        public Action ForceIdleState { get; private set; }

        /************************************************************************************************************************/

        private void Awake()
        {
            // 注意,这个类有一个[DefaultExecutionOrder]属性,以确保这个方法在任何其他可能想要访问它的组件之前运行.

            ForceIdleState = () => StateMachine.ForceSetState(_Idle);

            StateMachine = new StateMachine<CreatureState>(_Idle);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 调用 <see cref="StateMachine{TState}.TrySetState"/>. 通常,您可以直接访问 <see cref="StateMachine"/> 此方法只存在于由UI按钮调用时.
        /// </summary>
        public void TrySetState(CreatureState state)
        {
            StateMachine.TrySetState(state);
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Inspector Gadget Pro在绘制常规Inspector GUI后调用此方法,允许此脚本在播放模式中显示其当前状态.
        /// </summary>
        /// <remarks>
        /// Inspector Gadget Pro允许您轻松地自定义Inspector，而无需编写完整的自定义Inspector类，只需添加一个具有此名称的方法即可. 
        /// 没有 Inspector Gadgets,这个方法什么也做不了.可以从 https://kybernetik.com.au/inspector-gadgets/pro 购买.
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
