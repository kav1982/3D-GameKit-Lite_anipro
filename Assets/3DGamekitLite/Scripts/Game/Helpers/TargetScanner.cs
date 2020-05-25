using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{

    /// <summary>
    /// 使用这个类可以根据相关参数【扫描和定位玩家】
    /// 这个是怪物使用的类
    /// </summary>
    //use this class to simply scan & spot the player based on the parameters.
    //Used by enemies behaviours.
    [System.Serializable]
    public class TargetScanner
    {
        public float heightOffset = 0.0f;// 探测器的高度偏移
        public float detectionRadius = 10;// 检测半径
        [Range(0.0f, 360.0f)]
        public float detectionAngle = 270;// 检测角度 
        public float maxHeightDifference = 1.0f;// 高度差 ,超过这个返回了, 即放弃追击
        public LayerMask viewBlockerLayerMask;// 可以阻挡探测视野的层级

        /// <summary>
        /// Check if the player is visible according to that Scanner parameter.
        /// 检测玩家(通过相应参数检测玩家是否可以通过此探测器看见)
        /// </summary>
        /// <param name="detector">The transform from which run the detection 持有监视器的GameObject的Transform</param>
        /// /// <param name="useHeightDifference">If the computation should comapre the height difference to the maxHeightDifference value or ignore是否忽略高度差</param>
        /// <returns>The player controller if visible, null otherwise如果检测到了, 返回PlayerController实例, 否则返回null</returns>
        public PlayerController Detect(Transform detector, bool useHeightDifference = true)
        {
            // 如果没有玩家 || 玩家正在出生, 则返回null
            //if either the player is not spwned or they are spawning, we do not target them
            if (PlayerController.instance == null || PlayerController.instance.respawning)
                return null;

            Vector3 eyePos = detector.position + Vector3.up * heightOffset;// 探测器的位置
            Vector3 toPlayer = PlayerController.instance.transform.position - eyePos;//探测器--> 玩家的位移
            Vector3 toPlayerTop = PlayerController.instance.transform.position + Vector3.up * 1.5f - eyePos;//探测器到玩家上方的一个位移

            // 如果使用高度差检测
            // 如果目标太高 || 太低, 则不需要尝试追击了
            // 这里加上heightset 感觉有点问题！！！ 有时间再测试吧
            if (useHeightDifference && Mathf.Abs(toPlayer.y + heightOffset) > maxHeightDifference)
            { //if the target is too high or too low no need to try to reach it, just abandon pursuit
                return null;
            }

            // 到玩家的一个水平方向
            Vector3 toPlayerFlat = toPlayer;
            toPlayerFlat.y = 0;

            if (toPlayerFlat.sqrMagnitude <= detectionRadius * detectionRadius)
            {// 如果到玩家的距离小于检测距离---> 检测范围
                // 检测角度
                // 感觉这里 >= 也可以检测到
                // 玩家在可以可以检测到的角度内
                if (Vector3.Dot(toPlayerFlat.normalized, detector.forward) >
                    Mathf.Cos(detectionAngle * 0.5f * Mathf.Deg2Rad))
                {

                    bool canSee = false;

                    Debug.DrawRay(eyePos, toPlayer, Color.blue);// 指向玩家的脚底
                    Debug.DrawRay(eyePos, toPlayerTop, Color.blue);// 指向玩家的头顶

                    // 忽视了开启了isTrigger的GameObject
                    canSee |= !Physics.Raycast(eyePos, toPlayer.normalized, detectionRadius,
                        viewBlockerLayerMask, QueryTriggerInteraction.Ignore);

                    // 忽视了开启了isTrigger的GameObject
                    canSee |= !Physics.Raycast(eyePos, toPlayerTop.normalized, toPlayerTop.magnitude,
                        viewBlockerLayerMask, QueryTriggerInteraction.Ignore);

                    // 只要上面这两条射线有一条可以没有被阻挡, 即视为检测到了玩家

                    if (canSee)// 如果检测到了玩家， 直接返回
                        return PlayerController.instance;
                }
            }

            return null;
        }


#if UNITY_EDITOR

        /// <summary>
        /// 绘制PlayerScanner的一些辅助UI
        /// </summary>
        /// <param name="transform">怪物的Transform</param>
        public void EditorGizmo(Transform transform)
        {
            Color c = new Color(0, 0, 0.7f, 0.4f);

            // 在3D空间内画一个圆形扇形
            UnityEditor.Handles.color = c;
            Vector3 rotatedForward = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward;// 

            // by sixgod
            // 我觉得这里可能会有点问题, 在斜坡上！！！
            // 扇形的中心点、 法线(垂直于扇形的线)、开始的角度、需要绘制的一个角度, 半径
            //Vector3 center, Vector3 normal, Vector3 from, float angle, float radius
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, rotatedForward, detectionAngle, detectionRadius);

            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);// 黄色 绘制GameObject头顶的那只检测眼睛
            Gizmos.DrawWireSphere(transform.position + Vector3.up * heightOffset, 0.2f);
        }

#endif
    }

}