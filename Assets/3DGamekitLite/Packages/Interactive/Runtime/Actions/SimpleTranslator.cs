using System;
using UnityEngine;

namespace Gamekit3D.GameCommands
{
    public class SimpleTranslator : SimpleTransformer
    {
        public new Rigidbody rigidbody;
        public Vector3 start = -Vector3.forward;
        public Vector3 end = Vector3.forward;

        // 接收到指令执行的行为
        public override void PerformTransform(float position)
        {
            base.PerformTransform(position);
            var curvePosition = accelCurve.Evaluate(position);// 曲线上的点
            var pos = transform.TransformPoint(Vector3.Lerp(start, end, curvePosition));// 本地
            Vector3 deltaPosition = pos - rigidbody.position;
            if (Application.isEditor && !Application.isPlaying)// Editor 模式下 直接将此位置赋值 给rigidbody
                rigidbody.transform.position = pos;
            rigidbody.MovePosition(pos);// 刚体之间的碰撞不会影响接下来的唯一; pos 指 将要移动到的位置

            if (m_Platform != null)// 这个带着玩家一起移动
                m_Platform.MoveCharacterController(deltaPosition);
        }
    }
}
