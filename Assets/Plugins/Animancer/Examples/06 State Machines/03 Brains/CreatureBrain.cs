// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // �����ֶδ�δ����ֵ������ʼ�վ�����Ĭ��ֵ�ľ���.

using UnityEngine;

namespace Animancer.Examples.StateMachines.Brains
{
    /// <summary>
    /// ����һ�� <see cref="Brains.Creature"/> �����Ļ���.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Brains - Creature Brain")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Brains/CreatureBrain")]
    public abstract class CreatureBrain : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private Creature _Creature;
        public Creature Creature
        {
            get { return _Creature; }
            set
            {
                // ����Brainsʾ��ʹ�����������ʱ��Brains֮������л�.

                if (_Creature == value)
                    return;

                var oldCreature = _Creature;
                _Creature = value;

                // ȷ���Ǹ��ɵĽ�ɫ����ӳ�����brain.
                if (oldCreature != null)
                    oldCreature.Brain = null;

                // ��һ���µĽ�ɫ��ӳ����� brain.
                // ��Ҳֻϣ�� brains ���н�ɫ���Ƶ�ʱ��Żᱻ����.
                if (value != null)
                {
                    value.Brain = this;
                    enabled = true;
                }
                else
                {
                    enabled = false;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary> brain ��Ҫ�ƶ��ķ���. </summary>
        public Vector3 MovementDirection { get; protected set; }

        /// <summary> ��ʾ brain �Ƿ����ܵ�״̬.</summary>
        public bool IsRunning { get; protected set; }

        /************************************************************************************************************************/
    }
}
