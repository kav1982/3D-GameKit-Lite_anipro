// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0618 // �������ͻ��Ա��ʱ(����Animancer Lite�е�MixerState)�ľ���.
#pragma warning disable CS0649 // �����ֶδ�δ����ֵ������ʼ�վ�����Ĭ��ֵ�ľ���.

using Animancer.Examples.FineControl;
using UnityEngine;

namespace Animancer.Examples.Locomotion
{
    /// <summary>
    /// һ��<see cref="SpiderBot"/> �� <see cref="MixerState.Transition2D"/> ��һ��<see cref="Rigidbody"/> ������������κη����ƶ�.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Locomotion - Spider Bot Advanced")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Locomotion/SpiderBotAdvanced")]
    public sealed class SpiderBotAdvanced : SpiderBot
    {
        /************************************************************************************************************************/

        [SerializeField] private Rigidbody _Body; //����
        [SerializeField] private float _TurnSpeed = 90; //ת���ٶ�
        [SerializeField] private float _MovementSpeed = 1.5f; //�ƶ��ٶ�
        [SerializeField] private float _SprintMultiplier = 2;  //��̳���

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

            // �����ƶ�״̬������������.
            // ��ȷ�����ǿ��Է���_MovementAnimation.��ʵ�ʲ�����֮ǰ������������������.
            Animancer.States.GetOrCreate(_Move);
        }

        /************************************************************************************************************************/

        protected override void Update()
        {
            // �����˶����򣬱�Ҫʱ���û����������ѻ����˯��״̬.
            _MovementDirection = GetMovementDirection();
            base.Update();

            // ����˶�״̬���ڲ�����δ����:
            if (_Move.State.IsActive)
            {
                // ��ת���������ͬ��Y�Ƕ�.
                var eulerAngles = transform.eulerAngles;
                var targetEulerY = Camera.main.transform.eulerAngles.y;
                eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, targetEulerY, _TurnSpeed * Time.deltaTime);
                transform.eulerAngles = eulerAngles;

                // �˶��ķ�����������ռ䣬����������Ҫ����ת���ɾֲ��ռ�����Ӧ.
                // ���ڵ�ǰ����ת��ͨ��ʹ�õ����ȷ���÷����ϵļн�.
                // ����ÿ����,������Ǹ�������ת�����û�б�Ҫ��.
                _Move.State.Parameter = new Vector2(
                    Vector3.Dot(transform.right, _MovementDirection),
                    Vector3.Dot(transform.forward, _MovementDirection));

                // �����ٶ�ȡ�������Ƿ��ڳ��.
                var isSprinting = Input.GetMouseButton(0);
                _Move.State.Speed = isSprinting ? _SprintMultiplier : 1;
            }
            else// �������ȫֹͣ.
            {
                _Move.State.Parameter = Vector2.zero;
                _Move.State.Speed = 0;
            }
        }

        /************************************************************************************************************************/

        private Vector3 GetMovementDirection()
        {
            // ����������л�ȡһ��������귽��һ�µ�����.
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // ������û�л����κζ�����ʱ��ֹͣ�ƶ�.
            // ע�������������Ϊ����Raycast�㣬����Raycast�Ͳ��������.
            RaycastHit raycastHit;
            if (!Physics.Raycast(ray, out raycastHit))// ע���̾��!,�������......
                return Vector3.zero;

            // ������߻�����ʲô�����������������嵽�Ǹ����ˮƽ����.
            var direction = raycastHit.point - transform.position;
            direction.y = 0;  // ��Y�ĸ߶���Ϊ0.

            // ������ǵִ�Ŀ�ĵأ�ֹͣ�ƶ�.
            // ���ǿ���ʹ����0.1����������С��ֵ����������ٶ�̫�죬�Ǿ��в�ͨ��.
            // �෴�����ǿ��Լ������ڵ�֡��������ٶ�ʵ���ƶ��ľ�����ȷ��.
            // ��������һ֡����򾭹�Ŀ�ĵ�.
            var distance = direction.magnitude;
            if (distance < _MovementSpeed * _SprintMultiplier * Time.fixedDeltaTime)
            {
                return Vector3.zero;
            }
            else
            {
                // ���򣬽������һ�����������ǾͲ�����ݾ���ı��ٶ�.
                // ����direction.Normalize()����ͬ�������飬���ǻ��ٴμ����С.
                return direction / distance;
            }
        }

        /************************************************************************************************************************/

        private void FixedUpdate()
        {
            // ʹ���峯Ԥ���ķ����˶�.
            _Body.velocity = _MovementDirection * _Move.State.Speed * _MovementSpeed;
        }

        /************************************************************************************************************************/
    }
}
