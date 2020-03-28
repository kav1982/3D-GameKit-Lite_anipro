// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告.
using System;
using UnityEngine;

namespace Animancer.Examples.StateMachines.Brains
{
    /// <summary>
    /// 一个 <see cref="Creature"/>的数值细节.
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

        // 生命值满.
        // 力量、敏捷、智力.
        // 承载能力.
        // Etc.

        /************************************************************************************************************************/
    }
}
