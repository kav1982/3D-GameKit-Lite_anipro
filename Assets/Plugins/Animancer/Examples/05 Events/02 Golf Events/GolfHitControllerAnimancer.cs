// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽字段从未被赋值，并且始终具有其默认值的警告.
using UnityEngine;

namespace Animancer.Examples.Events
{
    /// <summary>
    /// 一个 <see cref="GolfHitController"/> 它使用了完全在检查器中配置的Animancer事件.
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Golf Events - Animancer")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.AnimationEvents/GolfHitControllerAnimancer")]
    public sealed class GolfHitControllerAnimancer : GolfHitController
    {
        /************************************************************************************************************************/

        // Nothing here.
        // 这个脚本与基本的GolfHitController没有什么不同.
        // 它假设在检查器中已经完全配置了事件.

        /************************************************************************************************************************/
    }
}
