using System;
using UnityEngine;

namespace Gamekit3D.GameCommands
{
    public class SimpleTranslator : SimpleTransformer
    {
        public new Rigidbody rigidbody;
        public Vector3 start = -Vector3.forward;
        public Vector3 end = Vector3.forward;

        // ���յ�ָ��ִ�е���Ϊ
        public override void PerformTransform(float position)
        {
            base.PerformTransform(position);
            var curvePosition = accelCurve.Evaluate(position);// �����ϵĵ�
            var pos = transform.TransformPoint(Vector3.Lerp(start, end, curvePosition));// ����
            Vector3 deltaPosition = pos - rigidbody.position;
            if (Application.isEditor && !Application.isPlaying)// Editor ģʽ�� ֱ�ӽ���λ�ø�ֵ ��rigidbody
                rigidbody.transform.position = pos;
            rigidbody.MovePosition(pos);// ����֮�����ײ����Ӱ���������Ψһ; pos ָ ��Ҫ�ƶ�����λ��

            if (m_Platform != null)// ����������һ���ƶ�
                m_Platform.MoveCharacterController(deltaPosition);
        }
    }
}
