// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0618 // 屏蔽类型或成员过时(对于Animancer Lite中的MixerState)的警告.
#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告.

using Animancer.Examples.FineControl;
using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// 一个<see cref="SpiderBot"/> 和 <see cref="MixerState.Transition2D"/> 和一个<see cref="Rigidbody"/> 允许机器人向任何方向移动.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Locomotion - Spider Bot Advanced")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Locomotion/SpiderBotAdvanced")]
    public sealed class SpiderBotAdvanced : SpiderBot
    {
        /************************************************************************************************************************/

        [SerializeField] private Rigidbody _Body; //刚体
        [SerializeField] private float _TurnSpeed = 90; //转向速度
        [SerializeField] private float _MovementSpeed = 1.5f; //移动速度
        [SerializeField] private float _SprintMultiplier = 2;  //冲刺乘数

        /************************************************************************************************************************/

        [SerializeField]
        private MixerState.Transition2D _Move;

        protected override ITransition MovementAnimation
        {
            get { return _Move; }
        }

        /************************************************************************************************************************/

        private Vector3 _MovementDirection;

        protected override bool IsMoving
        {
            get { return _MovementDirection != Vector3.zero; }
        }

        /************************************************************************************************************************/

        protected override void Awake()
        {
            base.Awake();

            // 创建移动状态，但不播放它.
            // 这确保我们可以访问_MovementAnimation.在实际播放它之前，在其他方法中声明.
            Animancer.States.GetOrCreate(_Move);
        }

        /************************************************************************************************************************/

        protected override void Update()
        {
            // 计算运动方向，必要时调用基本方法唤醒或进入睡眠状态.
            _MovementDirection = GetMovementDirection();
            base.Update();

            // 如果运动状态正在播放且未淡出:
            if (_Move.State.IsActive)
            {
                // 旋转到与相机相同的Y角度.
                var eulerAngles = transform.eulerAngles;
                var targetEulerY = Camera.main.transform.eulerAngles.y;
                eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, targetEulerY, _TurnSpeed * Time.deltaTime);
                transform.eulerAngles = eulerAngles;

                // 运动的方向是在世界空间，所以我们需要把它转换成局部空间来适应.
                // 对于当前的旋转，通过使用点积来确定该方向上的夹角.
                // 对于每个轴,如果我们根本不旋转，这就没有必要了.
                _Move.State.Parameter = new Vector2(
                    Vector3.Dot(transform.right, _MovementDirection),
                    Vector3.Dot(transform.forward, _MovementDirection));

                // 它的速度取决于你是否在冲刺.
                var isSprinting = Input.GetMouseButton(0);
                _Move.State.Speed = isSprinting ? _SprintMultiplier : 1;
            }
            else// 否则就完全停止.
            {
                _Move.State.Parameter = Vector2.zero;
                _Move.State.Speed = 0;
            }
        }

        /************************************************************************************************************************/

        private Vector3 GetMovementDirection()
        {
            // 从主摄像机中获取一条与鼠标光标方向一致的射线.
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 当射线没有击中任何东西的时候停止移动.
            // 注意这个对象被设置为忽略Raycast层，这样Raycast就不会击中它.
            RaycastHit raycastHit;
            if (!Physics.Raycast(ray, out raycastHit))// 注意感叹号!,如果不是......
                return Vector3.zero;

            // 如果射线击中了什么东西，计算从这个物体到那个点的水平方向.
            var direction = raycastHit.point - transform.position;
            direction.y = 0;  // 将Y的高度设为0.

            // 如果我们抵达目的地，停止移动.
            // 我们可以使用像0.1这样的任意小的值，但是如果速度太快，那就行不通了.
            // 相反，我们可以计算它在单帧中以最大速度实际移动的距离来确定.
            // 它将在下一帧到达或经过目的地.
            var distance = direction.magnitude;
            if (distance < _MovementSpeed * _SprintMultiplier * Time.fixedDeltaTime)
            {
                return Vector3.zero;
            }
            else
            {
                // 否则，将方向归一化，这样我们就不会根据距离改变速度.
                // 调用direction.Normalize()会做同样的事情，但是会再次计算大小.
                return direction / distance;
            }
        }

        /************************************************************************************************************************/

        private void FixedUpdate()
        {
            // 使刚体朝预定的方向运动.
            _Body.velocity = _MovementDirection * _Move.State.Speed * _MovementSpeed;
        }

        /************************************************************************************************************************/
    }
}
