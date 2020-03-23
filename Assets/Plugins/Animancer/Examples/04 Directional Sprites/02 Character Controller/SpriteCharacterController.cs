// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.DirectionalSprites
{
    /// <summary>
    /// 一个更复杂的版本 <see cref="SpriteMovementController"/> 增加了运行和推动动画以及实际移动的能力.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Directional Sprites - Sprite Character Controller")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.DirectionalSprites/SpriteCharacterController")]
    public sealed class SpriteCharacterController : MonoBehaviour
    {
        /************************************************************************************************************************/

        [Header("Physics")]
        [SerializeField] private CapsuleCollider2D _Collider;
        [SerializeField] private Rigidbody2D _Rigidbody;
        [SerializeField] private float _WalkSpeed = 1;
        [SerializeField] private float _RunSpeed = 2;

        [Header("Animations")]
        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private DirectionalAnimationSet _Idle;
        [SerializeField] private DirectionalAnimationSet _Walk;
        [SerializeField] private DirectionalAnimationSet _Run;
        [SerializeField] private DirectionalAnimationSet _Push;
        [SerializeField] private Vector2 _Facing = Vector2.down;

        private Vector2 _Movement;
        private DirectionalAnimationSet _CurrentAnimationSet;

        /************************************************************************************************************************/

        private void Awake()
        {
            Play(_Idle);
        }

        /************************************************************************************************************************/

        private void Update()
        {   //根据水平或垂直的朝向来判断移动方向
            _Movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_Movement != Vector2.zero)
            {
                _Facing = _Movement;
                UpdateMovementState();

                // 捕捉动画移动的确切距离.
                // 当使用DirectionalAnimationSets时,这意味着字符将只向上/向右/向下/向左移动.
                // 但是DirectionalAnimationSets也允许对角移动.
                _Movement = _CurrentAnimationSet.Snap(_Movement);
                _Movement = Vector2.ClampMagnitude(_Movement, 1);
            }
            else
            {
                Play(_Idle);
            }
        }

        /************************************************************************************************************************/

        private void Play(DirectionalAnimationSet animations)
        {
            _CurrentAnimationSet = animations;
            _Animancer.Play(animations.GetClip(_Facing));
        }

        /************************************************************************************************************************/

        // 预先分配一个接触点的数组,这样Unity就不需要在我们每次调用的时候分配一个新的.
        // _Collider.GetContacts. 这个例子不会有超过4个接触点,但是你可以考虑在一个真正的游戏中有一个更高的数字.
        // 即使是64这样的大数字也比每次都制造新数字要好.
        private static readonly ContactPoint2D[] Contacts = new ContactPoint2D[4];

        private void UpdateMovementState()
        {
            var contactCount = _Collider.GetContacts(Contacts);
            for (int i = 0; i < contactCount; i++)
            {
                // 如果我们直接向一个物体移动(或在30度范围内),我们就是在推它.
                if (Vector2.Angle(Contacts[i].normal, _Movement) > 180 - 30)
                {
                    Play(_Push);
                    return;
                }
            }

            var isRunning = Input.GetButton("Fire3");// 默认左Shift
            Play(isRunning ? _Run : _Walk);
        }

        /************************************************************************************************************************/

        private void FixedUpdate()
        {
            // 根据当前动画确定所需的速度.
            var speed = _CurrentAnimationSet == _Run ? _RunSpeed : _WalkSpeed;
            _Rigidbody.velocity = _Movement * speed;
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// 当在检查器中加载此脚本的实例或更改值时,由Unity编辑器在编辑模式中调用.
        /// <para></para>
        /// 在编辑模式下设置角色的初始脚本,这样你可以在场景中工作时看到它.
        /// </summary>
        private void OnValidate()
        {
            if (_Idle == null)
                return;

            AnimancerUtilities.EditModePlay(_Animancer, _Idle.GetClip(_Facing), false);
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
