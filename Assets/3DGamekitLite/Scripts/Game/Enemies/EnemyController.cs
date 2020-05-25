using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

namespace Gamekit3D
{
//this assure it's runned before any behaviour that may use it, as the animator need to be fecthed
    [DefaultExecutionOrder(-1)]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        public bool interpolateTurning = false;// 是否允许插值去旋转
        public bool applyAnimationRotation = false;// 是否应用动画的旋转

        public Animator animator { get { return m_Animator; } }
        public Vector3 externalForce { get { return m_ExternalForce; } }
        public NavMeshAgent navmeshAgent { get { return m_NavMeshAgent; } }
        public bool followNavmeshAgent { get { return m_FollowNavmeshAgent; } }
        public bool grounded { get { return m_Grounded; } }

        protected NavMeshAgent m_NavMeshAgent;// 寻路
        protected bool m_FollowNavmeshAgent;
        protected Animator m_Animator;// 动画控制器
        protected bool m_UnderExternalForce;// 是否受到额外的力
        protected bool m_ExternalForceAddGravity = true;// 额外的力是否增加重力
        protected Vector3 m_ExternalForce;// 额外的力
        protected bool m_Grounded;// 是否在地上

        protected Rigidbody m_Rigidbody;

        const float k_GroundedRayDistance = .8f;// 向地面发出射线的距离 用于检测Enemy是否在地面上

        void OnEnable()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();// 寻路的
            m_Animator = GetComponent<Animator>();// 动画
            m_Animator.updateMode = AnimatorUpdateMode.AnimatePhysics;//设置动画更新的模式

            m_NavMeshAgent.updatePosition = false;// 寻路 更新位置

            // 处理Enemy刚体
            m_Rigidbody = GetComponentInChildren<Rigidbody>();
            if (m_Rigidbody == null)
                m_Rigidbody = gameObject.AddComponent<Rigidbody>();

            m_Rigidbody.isKinematic = true;// 不受动力学影响
            m_Rigidbody.useGravity = false;// 不使用重力
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            m_FollowNavmeshAgent = true;
        }

        private void FixedUpdate()
        {
            animator.speed = PlayerInput.Instance != null && PlayerInput.Instance.HaveControl() ? 1.0f : 0.0f;

            CheckGrounded();// 不断的检测自己是否在地面上

            if (m_UnderExternalForce)// 如果受到了额外的
                ForceMovement();// 强制移动
        }

        void CheckGrounded()
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
            m_Grounded = Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers,QueryTriggerInteraction.Ignore);
        }

        void ForceMovement()
        {
            if(m_ExternalForceAddGravity)// 是否额外增加重力
            {
                // F * t = m * v --> 假设物体的质量是1kg, 那么Physics.gravity * Time.deltaTime 表示的是 移动速度
                m_ExternalForce += Physics.gravity * Time.deltaTime;// Physics.gravity -> (0.0f,-9.8f,0.0f)
                // F * t = m * v
                // 
            }


            // 碰撞检测
            RaycastHit hit;
            // v * t = s , 所以 movement 表示物体将要进行位移变化的位移
            Vector3 movement = m_ExternalForce * Time.deltaTime;


            // 碰撞检测
            if (!m_Rigidbody.SweepTest(movement.normalized, out hit, movement.sqrMagnitude))
            {
                m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
            }

            // 瞬间移动到这个位置
            m_NavMeshAgent.Warp(m_Rigidbody.position);
        }

        private void OnAnimatorMove()
        {
            if (m_UnderExternalForce)// 如果受力 
                return;

            if (m_FollowNavmeshAgent)
            {
                m_NavMeshAgent.speed = (m_Animator.deltaPosition / Time.deltaTime).magnitude;
                transform.position = m_NavMeshAgent.nextPosition;
            }
            else
            {
                RaycastHit hit;
                if (!m_Rigidbody.SweepTest(m_Animator.deltaPosition.normalized, out hit,
                    m_Animator.deltaPosition.sqrMagnitude))
                {
                    m_Rigidbody.MovePosition(m_Rigidbody.position + m_Animator.deltaPosition);
                }
            }

            if (applyAnimationRotation)
            {
                transform.forward = m_Animator.deltaRotation * transform.forward;
            }
        }

        /// <summary>
        /// 通常用于禁止导航到某点, 在我们想要用动画去移动怪物的时候使用
        /// </summary>
        /// <param name="follow"></param>
        // used to disable position being set by the navmesh agent, for case where we want the animation to move the enemy instead (e.g. Chomper attack)
        public void SetFollowNavmeshAgent(bool follow)
        {
            if (!follow && m_NavMeshAgent.enabled)
            {
                m_NavMeshAgent.ResetPath();
            }
            else if(follow && !m_NavMeshAgent.enabled)
            {
                m_NavMeshAgent.Warp(transform.position);
            }

            m_FollowNavmeshAgent = follow;
            m_NavMeshAgent.enabled = follow;
        }

        // Enemy增加一个力, 默认会增加重力
        public void AddForce(Vector3 force, bool useGravity = true)
        {
            if (m_NavMeshAgent.enabled)// 组件是否被启用
                m_NavMeshAgent.ResetPath();// 清理掉当前的寻路信息

            m_ExternalForce = force;// 额外的力
            m_NavMeshAgent.enabled = false;// 禁用寻路
            m_UnderExternalForce = true;// 受到了额外的力
            m_ExternalForceAddGravity = useGravity;// 是否受到重力
        }

        public void ClearForce()
        {
            m_UnderExternalForce = false;
            m_NavMeshAgent.enabled = true;
        }

        /// <summary>
        /// 设置Enemy的方向
        /// </summary>
        /// <param name="forward"></param>
        public void SetForward(Vector3 forward)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward);

            if (interpolateTurning)// 如果允许插值转向, 那么就做插值
            {
                targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                    m_NavMeshAgent.angularSpeed * Time.deltaTime);
            }

            transform.rotation = targetRotation;
        }

        /// <summary>
        /// 为寻路设置一个目标
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool SetTarget(Vector3 position)
        {
            return m_NavMeshAgent.SetDestination(position);
        }
    }
}