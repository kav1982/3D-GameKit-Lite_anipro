using UnityEngine;

namespace Gamekit3D.GameCommands
{
    public abstract class SimpleTransformer : GameCommandHandler
    {
        public enum LoopType
        {
            Once,
            PingPong,
            Repeat
        }

        public LoopType loopType;

        public float duration = 1;
        public AnimationCurve accelCurve;// ��������

        public bool activate = false;// �Ƿ񱻼���
        public SendGameCommand OnStartCommand, OnStopCommand;

        public AudioSource onStartAudio, onEndAudio;//����Դ

        [Range(0, 1)]
        public float previewPosition;
        float time = 0f;
        float position = 0f;
        float direction = 1f;

        protected Platform m_Platform;

        // ���Ժ���, �����ſ�������
        [ContextMenu("Test Start Audio")]
        void TestPlayAudio()
        {
            if (onStartAudio != null) onStartAudio.Play();
        }

        protected override void Awake()
        {
            base.Awake();

            m_Platform = GetComponentInChildren<Platform>();
        }

        public override void PerformInteraction()
        {
            activate = true;// ����GameObject����Ϊ����
            if (OnStartCommand != null) OnStartCommand.Send();
            if (onStartAudio != null) onStartAudio.Play();
        }

        public void FixedUpdate()
        {
            if (activate)
            {
                //print("come here ... ");
                // Time.deltaTime ֡ʱ��
                time = time + (direction * Time.deltaTime / duration);
                switch (loopType)
                {
                    case LoopType.Once:
                        LoopOnce();
                        break;
                    case LoopType.PingPong:
                        LoopPingPong();
                        break;
                    case LoopType.Repeat:
                        LoopRepeat();
                        break;
                }
                PerformTransform(position);
            }
        }

        public virtual void PerformTransform(float position)
        {

        }

        void LoopPingPong()
        {
            //print("time: " + time);
            position = Mathf.PingPong(time, 1f);
            //print("position: " + position);
        }

        void LoopRepeat()
        {
            position = Mathf.Repeat(time, 1f);
        }

        void LoopOnce()
        {
            print("call LoopOnce....");
            position = Mathf.Clamp01(time);
            if (position >= 1)
            {
                enabled = false;//�ű��ᱻ���� �����ٵ���FixedUpdate������
                if (OnStopCommand != null) OnStopCommand.Send();
                direction *= -1;// ������
            }
            // ������ʵ�����Լ���һ��
            // ��position <= 0 ��ʱ��, �����ٷ�ת
        }
    }
}
