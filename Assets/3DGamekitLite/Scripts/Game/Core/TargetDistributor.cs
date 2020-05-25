using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    // This class allow to distribute arc around a target, used for "crowding" by ennemis, so they all
    // come at the player (or any target) from different direction.
    // 简单来说， 这个类就是让攻击Player的怪物均匀分布在玩家附近， 而不会在一个点集中
    // 此类的功能类似于代理, 负责分配位置给怪物
    // 代理与Player(玩家)绑定
    [DefaultExecutionOrder(-1)]
    public class TargetDistributor : MonoBehaviour
    {
        // 内部类, 此类表示玩家周围的一些点
        // 如果与怪物交战 ,则怪物会寻路在这些点上, 以此来达到怪物是均匀分布在玩家附近的
        //Use as a mean to communicate between this target and the followers
        public class TargetFollower
        {
            // 是否要求系统分配一个位置
            //target should set that to true when they require the system to give them a position
            public bool requireSlot;
            // 分配的位置的序号
            //will be -1 if none is currently assigned
            public int assignedSlot;
            // 分配的位置
            //the position the follower want to reach for the target.
            public Vector3 requiredPoint;

            //
            public TargetDistributor distributor;

            public TargetFollower(TargetDistributor owner)
            {
                distributor = owner;
                requiredPoint = Vector3.zero;// 默认 是 玩家当前的位置
                requireSlot = false;
                assignedSlot = -1;
            }
        }

        public int arcsCount;// 代理分配的TargetFollower的数目

        protected Vector3[] m_WorldDirection;// 管理TargetFollower的方向

        protected bool[] m_FreeArcs;// 管理TargetFollower的状态是否有怪物占领
        protected float arcDegree;// 分配TargetFollower的平均角度

        protected List<TargetFollower> m_Followers;// 分配的TargetFollower的列表

        public void OnEnable()
        {
            m_WorldDirection = new Vector3[arcsCount];
            m_FreeArcs = new bool[arcsCount];

            m_Followers = new List<TargetFollower>();

            arcDegree = 360.0f / arcsCount;
            Quaternion rotation = Quaternion.Euler(0, -arcDegree, 0);// 逆时针计算 绕y轴逆时针选装

            // 初始化所有点
            Vector3 currentDirection = Vector3.forward;// 当前的方向是z轴方向 
            for (int i = 0; i < arcsCount; ++i)
            {
                m_FreeArcs[i] = true;
                m_WorldDirection[i] = currentDirection;
                currentDirection = rotation * currentDirection;
            }
        }

        /// <summary>
        /// 由怪物调用此函数来注册玩家附近的一个TargetFollower
        /// 怪物找到了目标，便会调用此函数来注册
        /// </summary>
        /// <returns></returns>
        public TargetFollower RegisterNewFollower()
        {
            TargetFollower follower = new TargetFollower(this);
            m_Followers.Add(follower);
            return follower;
        }

        /// <summary>
        /// 有怪物来调用此函数来取消注册玩家附近的一个TargetFollower
        /// 1. 此怪物disable的时候, 取消注册
        /// 2. 怪物丢失目标,取消注册
        /// </summary>
        /// <param name="follower"></param>
        public void UnregisterFollower(TargetFollower follower)
        {
            if (follower.assignedSlot != -1)// 该follower已经分配过位置了
            {
                m_FreeArcs[follower.assignedSlot] = true;// 没有怪物占用此follower了
            }


            m_Followers.Remove(follower);
        }

        /// <summary>
        /// 在这里， 我们会为每一个TargetFolloewr分配一个位置
        /// </summary>
        //at the end of the frame, we distribute target position to all follower that asked for one.
        private void LateUpdate()
        {
            for (int i = 0; i < m_Followers.Count; ++i)
            {
                var follower = m_Followers[i];

                //we free whatever arc this follower may already have. 
                //If it still need it, it will be picked again next lines.
                //if it changed position the new one will be picked.
                // 如果这个TargetFollower已经被一个怪物预定了, 那么我将释放这个点, 让它可以被其它的follower占据
                if (follower.assignedSlot != -1)
                {
                    m_FreeArcs[follower.assignedSlot] = true;
                }

                if (follower.requireSlot)// 是否请求分配了, 请求分配, 在这里才会分配一个位置给follwers
                {
                    follower.assignedSlot = GetFreeArcIndex(follower);
                }
            }
        }

        /// <summary>
        /// 获取当前索引的一个角度
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 GetDirection(int index)
        {
            return m_WorldDirection[index];
        }

        /// <summary>
        /// 获取一个位置
        /// </summary>
        /// <param name="follower"></param>
        /// <returns></returns>
        public int GetFreeArcIndex(TargetFollower follower)
        {
            bool found = false;// 标记变量, 表示是否在玩家附近找到这样一个点

            Vector3 wanted = follower.requiredPoint - transform.position;// 0 - 玩家的位置
            Vector3 rayCastPosition = transform.position + Vector3.up * 0.4f;

            wanted.y = 0;
            float wantedDistance = wanted.magnitude;// 模

            wanted.Normalize();


            // 想要的点距离玩家面向的角度
            float angle = Vector3.SignedAngle(wanted, Vector3.forward, Vector3.up);
            if (angle < 0)
                angle = 360 + angle;

            int wantedIndex = Mathf.RoundToInt(angle / arcDegree);// 四舍五入 获取想要的点在玩家周围一些预先分号的点中的序号
            if (wantedIndex >= m_WorldDirection.Length)// 如果角度超出范围, 则重新分配
                wantedIndex -= m_WorldDirection.Length;

            int choosenIndex = wantedIndex;

            // 在玩家位置上 的 略高一点(高0.4f左右) ，向预设点的方向发出wantedDistance长的射线, 检测碰撞体
            // 没有检测到碰撞体, 则证明该位置是可以分配的,
            // 如果有碰撞体, 则证明该位置不可分配
            RaycastHit hit;
            if (!Physics.Raycast(rayCastPosition, GetDirection(choosenIndex), out hit, wantedDistance))
                found = m_FreeArcs[choosenIndex];// 如果可以分配 , 确定这个位置是否已经被分配过了

            if (!found)// 如果仍然是没有找到
            {//we are going to test left right with increasing offset
                int offset = 1;
                int halfCount = arcsCount / 2;

                while (offset <= halfCount)
                {
                    int leftIndex = wantedIndex - offset;// 找wantedIndex左边的点
                    int rightIndex = wantedIndex + offset;// 找wantedIndex右边的点

                    // 临界条件判定
                    if (leftIndex < 0) leftIndex += arcsCount;
                    if (rightIndex >= arcsCount) rightIndex -= arcsCount;

                    // 先对其左侧的点做射线检测
                    if (!Physics.Raycast(rayCastPosition, GetDirection(leftIndex), wantedDistance) && m_FreeArcs[leftIndex])
                    {// 如果这个点没有被占用 && 没有被碰撞体占据
                        choosenIndex = leftIndex;
                        found = true;
                        break;
                    }

                    // 再对其右侧的点做射线检测
                    if (!Physics.Raycast(rayCastPosition, GetDirection(rightIndex), wantedDistance) && m_FreeArcs[rightIndex])
                    {// 如果这个点没有被占用 && 没有被碰撞体占据
                        choosenIndex = rightIndex;
                        found = true;
                        break;
                    }

                    offset += 1;
                }
            }

            // 如果还没有找到, 则返回-1， 告诉调用者附近没有坑了
            if (!found)
            {//we couldn't find a free direction, return -1 to tell the caller there is no free space
                return -1;
            }

            m_FreeArcs[choosenIndex] = false;// 这个位置已经被占据
            return choosenIndex;
        }

        /// <summary>
        /// 释放这个位置, 标示这个位置没有被占据
        /// </summary>
        /// <param name="index"></param>
        public void FreeIndex(int index)
        {
            m_FreeArcs[index] = true;
        }
    }

}