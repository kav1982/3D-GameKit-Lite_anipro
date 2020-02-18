// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽Unity对于字段从未被赋值，并且始终具有其默认值的警告

using UnityEngine;

namespace Animancer.Examples.FineControl
{
    /// <summary>
    /// 一个 <see cref="SpiderBot"/> 代表单一的运动演示
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Fine Control - Spider Bot Simple")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.FineControl/SpiderBotSimple")]
    //sealed 密封类,最终类.不可再被继承
    public sealed class SpiderBotSimple : SpiderBot
    {
        /************************************************************************************************************************/

        protected override bool IsMoving
        {
            get { return Input.GetKey(KeyCode.Space); }
        }

        /************************************************************************************************************************/

        [SerializeField] private ClipState.Transition _Move;

        protected override ITransition MovementAnimation
        {
            get { return _Move; }
        }

        /************************************************************************************************************************/
    }
}
