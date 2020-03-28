// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 //  �����ֶδ�δ����ֵ������ʼ�վ�����Ĭ��ֵ�ľ���.

using Animancer.FSM;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Brains
{
    /// <summary>
    /// ʹ�ü���������ƽ�ɫ��<see cref="CreatureBrain"/>.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Brains - Keyboard Brain")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Brains/KeyboardBrain")]
    public sealed class KeyboardBrain : CreatureBrain
    {
        /************************************************************************************************************************/

        [SerializeField] private CreatureState _Locomotion;

        /************************************************************************************************************************/

        private void Update()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input != Vector2.zero)
            {
                // ��ȡ�����ǰ�������ʸ��,��������ƽ����XZƽ����.
                var camera = Camera.main.transform;

                var forward = camera.forward;
                forward.y = 0;
                forward.Normalize();

                var right = camera.right;
                right.y = 0;
                right.Normalize();

                // ͨ�������������Щ���������˶�ʸ��.
                MovementDirection =
                    right * input.x +
                    forward * input.y;

                // ȷ������Ƿ���Ҫ����.
                IsRunning = Input.GetButton("Fire3");//Ĭ���� Shift.

                // �����û�н����ƶ�״̬,�ͽ����ƶ�״̬.
                _Locomotion.TryEnterState();
            }
            else
            {
                // ����ƶ�����,���ܿ���ʱ��Ӧ��ʹ����.
                MovementDirection = Vector3.zero;

                // �����û�н������״̬����������״̬.
                Creature.Idle.TryEnterState();
            }
        }

        /************************************************************************************************************************/
    }
}
