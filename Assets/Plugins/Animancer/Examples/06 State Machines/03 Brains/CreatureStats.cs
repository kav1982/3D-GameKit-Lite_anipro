// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // �����ֶδ�δ����ֵ������ʼ�վ�����Ĭ��ֵ�ľ���.
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Brains
{
    /// <summary>
    /// һ�� <see cref="Creature"/>����ֵϸ��.
    /// </summary>
    [Serializable]
    public sealed class CreatureStats
    {
        /************************************************************************************************************************/

        [SerializeField]
        private float _WalkSpeed = 2;
        public float WalkSpeed { get { return _WalkSpeed; } }

        [SerializeField]
        private float _RunSpeed = 4;
        public float RunSpeed { get { return _RunSpeed; } }

        public float GetMoveSpeed(bool isRunning)
        {
            return isRunning ? _RunSpeed : _WalkSpeed;
        }

        /************************************************************************************************************************/

        [SerializeField]
        private float _TurnSpeed = 360;
        public float TurnSpeed { get { return _TurnSpeed; } }

        /************************************************************************************************************************/

        // ����ֵ��.
        // ���������ݡ�����.
        // ��������.
        // Etc.

        /************************************************************************************************************************/
    }
}
