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
        public AnimationCurve accelCurve;// 加速曲线

        public bool activate = false;// 是否被激活
        public SendGameCommand OnStartCommand, OnStopCommand;

        public AudioSource onStartAudio, onEndAudio;//声音源

        [Range(0, 1)]
        public float previewPosition;
        float time = 0f;
        float position = 0f;
        float direction = 1f;

        protected Platform m_Platform;

        // 测试函数, 测试门开的声音
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
            activate = true;// 将该GameObject设置为激活
            if (OnStartCommand != null) OnStartCommand.Send();
            if (onStartAudio != null) onStartAudio.Play();
        }

        public void FixedUpdate()
        {
            if (activate)
            {
                //print("come here ... ");
                // Time.deltaTime 帧时间
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
                enabled = false;//脚本会被禁用 不会再调用FixedUpdate函数了
                if (OnStopCommand != null) OnStopCommand.Send();
                direction *= -1;// 方向反向
            }
            // 这里其实还可以加上一段
            // 当position <= 0 的时候, 方向再反转
        }
    }
}
