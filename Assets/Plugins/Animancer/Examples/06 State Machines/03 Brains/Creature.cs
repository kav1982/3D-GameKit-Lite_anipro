// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // �����ֶδ�δ����ֵ������ʼ�վ�����Ĭ��ֵ�ľ���.

using Animancer.FSM;
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Brains
{
    /// <summary>
    /// ��������һ����ɫ�Ĺ������ֺ����ǵĶ���״̬��.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Brains - Creature")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Brains/Creature")]
    [DefaultExecutionOrder(-5000)]// �����ʼ��״̬��.
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
                // ����� Brainsʾ��������ʱʹ�����ַ�����Brains֮����н���.

                if (_Brain == value)
                    return;

                var oldBrain = _Brain;
                _Brain = value;

                // ȷ���ϵ� brain ������Ȼ���������ɫ.
                if (oldBrain != null)
                    oldBrain.Creature = null;

                // ���µ� brain һ�����ڴ˽�ɫ������.
                if (value != null)
                    value.Creature = this;
            }
        }

        [SerializeField]
        private CreatureStats _Stats;
        public CreatureStats Stats { get { return _Stats; } }

        // ������.
        // �����ͷ���.
        // Ѱ·.
        // Etc.
        // �κ���������ɫ��ͬ�Ķ���.

        /************************************************************************************************************************/

        /// <summary>
        /// ����ý�ɫ�Ķ���������״̬��.
        /// </summary>
        public StateMachine<CreatureState> StateMachine { get; private set; }

        /// <summary>
        /// ǿ�� <see cref="StateMachine"/> ���� <see cref="Idle"/> ״̬.
        /// </summary>
        public Action ForceEnterIdleState { get; private set; }

        /************************************************************************************************************************/

        private void Awake()
        {
            // ע�⣬�������һ��[DefaultExecutionOrder]���ԣ���ȷ������������κ�����������Ҫ�����������֮ǰ����.

            ForceEnterIdleState = () => StateMachine.ForceSetState(_Idle);

            StateMachine = new StateMachine<CreatureState>(_Idle);
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Inspector gadget רҵ���ڻ��Ƴ���Inspector GUI����ô˷���,����˽ű��ڲ���ģʽ����ʾ�䵱ǰ״̬.
        /// </summary>
        /// <remarks>
        /// Inspector gadget Pro���������ɵ��Զ���Inspector,�������д�������Զ���Inspector��,ֻ�����һ�����д����Ƶķ�������. 
        /// û�� Inspector Gadgets, �������ʲôҲ������,���Դ� https://kybernetik.com.au/inspector-gadgets/pro ����˹���.
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
