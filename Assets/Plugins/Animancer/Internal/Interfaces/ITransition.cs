// Animancer // Copyright 2020 Kybernetik //

namespace Animancer
{
    /// <summary>
    /// 一个对象，它可以创建一个 <see cref="AnimancerState"/> 并管理它应该如何播放的细节.
    /// <para></para>
    /// 转换通常当作 <see cref="AnimancerPlayable.Play(ITransition)"/> 的参数来使用.
    /// </summary>
    public interface ITransition : IHasKey
    {
        /************************************************************************************************************************/

        /// <summary>
        /// 创建并返回一个新的 <see cref="AnimancerState"/> 连接到 `layer`.
        /// </summary>
        /// <remarks>
        /// 第一次在对象上使用转换时，将调用此方法来创建状态并使用 <see cref="IHasKey.Key"/> 将其注册到内部字典中，以便以后可以重载它.
        /// </remarks>
        AnimancerState CreateState(AnimancerLayer layer);

        /// <summary>
        /// 当一个转换被传递到 <see cref="AnimancerPlayable.Play(ITransition)"/> 时，这个属性确定将使用哪个<see cref="Animancer.FadeMode"/> .
        /// </summary>
        FadeMode FadeMode { get; }

        /// <summary>转换所需的时间(以秒为单位).</summary>
        float FadeDuration { get; }

        /// <summary>
        /// 通过 <see cref="AnimancerPlayable.Play(ITransition)"/> 调用来应用对“状态”的任何修改.
        /// </summary>
        /// <remarks>
        /// 与 <see cref="CreateState"/>, 不同的是，每次使用转换时都会调用此方法，这样它就可以执行转换比如设置为 
        /// <see cref="AnimancerState.Events"/> 或 <see cref="AnimancerState.Time"/>.
        /// </remarks>
        void Apply(AnimancerState state);

        /************************************************************************************************************************/
    }

    /// <summary>
    /// <see cref="ITransition"/> 和一些关于Unity编辑器GUI的附加细节.
    /// </summary>
    public interface ITransitionDetailed : ITransition
    {
        /************************************************************************************************************************/

        /// <summary>Indicates what the value of <see cref="AnimancerState.IsLooping"/> will be for the created state.</summary>
        bool IsLooping { get; }

        /// <summary>Determines what <see cref="AnimancerState.NormalizedTime"/> to start the animation at.</summary>
        float NormalizedStartTime { get; set; }

        /// <summary>Determines how fast the animation plays (1x = normal speed).</summary>
        float Speed { get; set; }

        /// <summary>The maximum amount of time the animation is expected to take (in seconds).</summary>
        float MaximumDuration { get; }

#if UNITY_EDITOR
        /// <summary>[Editor-Only]为这个转换添加上下文菜单函数.</summary>
        void AddItemsToContextMenu(UnityEditor.GenericMenu menu, UnityEditor.SerializedProperty property,
            Editor.Serialization.PropertyAccessor accessor);
#endif

        /************************************************************************************************************************/
    }
}

