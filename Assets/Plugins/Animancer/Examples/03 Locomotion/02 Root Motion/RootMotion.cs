// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// 关于如何对某些动画使用根运动，但对其他动画不使用根运动.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Locomotion - Root Motion")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Locomotion/RootMotion")]
    public sealed class RootMotion : MonoBehaviour
    {
        /************************************************************************************************************************/

        /// <summary>
        /// 一个 <see cref="ClipState.Transition"/> 与一个 <see cref="_ApplyRootMotion"/> 切换.
        /// </summary>
        [Serializable]
        public class MotionTransition : ClipState.Transition
        {
            /************************************************************************************************************************/

            [SerializeField, Tooltip("确定动画播放时是否应启用根运动")]
            private bool _ApplyRootMotion;

            /************************************************************************************************************************/

            public override void Apply(AnimancerState state)
            {
                base.Apply(state);
                state.Root.Component.Animator.applyRootMotion = _ApplyRootMotion;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private float _MaxDistance;
        [SerializeField] private MotionTransition[] _Animations;

        private Vector3 _Start;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            _Start = transform.position;
            Play(0);
        }

        /************************************************************************************************************************/

        /// <summary> 播放 <see cref="_Animations"/> 合集中指定的' index '的动画 </summary>
        /// <remarks> 此方法由UI按钮调用 </remarks>
        public void Play(int index)
        {
            _Animancer.Play(_Animations[index]);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// 如果移动太远，就传送此对象到它的起始位置.
        /// </summary>
        private void FixedUpdate()
        {
            if (Vector3.Distance(_Start, transform.position) > _MaxDistance)
                transform.position = _Start;
        }

        /************************************************************************************************************************/

        // 这些字段决定根运动将应用于哪个对象.
        // 通常情况下，对于您用来移动角色的任何系统，都只能使用其中之一.
        // 但是在这个例子中，我们可以同时使用他们并且演示如何使用.
        [SerializeField] private Transform _MotionTransform;
        [SerializeField] private Rigidbody _MotionRigidbody;
        [SerializeField] private CharacterController _MotionCharacterController;

        /// <summary>
        /// 调用 <see cref="Animator"/> 时,将启用根运动,将根运动应用于另一个对象.
        /// <para></para>
        /// 如果角色的 <see cref="Rigidbody"/> 或 <see cref="CharacterController"/> 位于 <see cref="Animator"/> 的父节点上，
        /// 因此模型与角色的机制是分离的，那么这将非常有用.
        /// </summary>
        private void OnAnimatorMove()
        {
            if (!_Animancer.Animator.applyRootMotion)
                return;

            if (_MotionTransform != null)
            {
                _MotionTransform.position += _Animancer.Animator.deltaPosition;
                _MotionTransform.rotation *= _Animancer.Animator.deltaRotation;
            }
            else if (_MotionRigidbody != null)
            {
                _MotionRigidbody.MovePosition(_MotionRigidbody.position + _Animancer.Animator.deltaPosition);
                _MotionRigidbody.MoveRotation(_MotionRigidbody.rotation * _Animancer.Animator.deltaRotation);
            }
            else if (_MotionCharacterController != null)
            {
                _MotionCharacterController.Move(_Animancer.Animator.deltaPosition);
                _MotionCharacterController.transform.rotation *= _Animancer.Animator.deltaRotation;
            }
            else
            {
                // 我们不是在重新定位，只是让Unity正常地应用根的运动.
                _Animancer.Animator.ApplyBuiltinRootMotion();
            }
        }

        /************************************************************************************************************************/
    }
}
