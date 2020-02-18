// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // 屏蔽Unity对于字段从未被赋值，并且始终具有其默认值的警告

using System.Collections;
using UnityEngine;

namespace Animancer.Examples.Basics
{
    /// <summary>
    ///使用协同程序按顺序播放一系列动画。
    /// </summary>
    [AddComponentMenu(Strings.MenuPrefix + "Examples/Basics - Sequence Coroutine")]
    [HelpURL(Strings.APIDocumentationURL + ".Examples.Basics/SequenceCoroutine")]
    //SequenceCoroutine协程序列
    public sealed class SequenceCoroutine : MonoBehaviour 
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;         //Animancer组件
        [SerializeField] private TextMesh _Text;                        //文本网格
        [SerializeField] private ClipState.Transition[] _Animations;    //移动状态机

        /************************************************************************************************************************/

        private void OnEnable()
        {
             
            //这个脚本会给出一个通用的IndexOutOfRangeException，如果我们没有在检查器中设置任何动画，如果我们只添加一个动画，
            //它会工作得很好，但只是简单地播放动画。因此，我们可以使用Debug.Assert来确保至少有两个动画，如果没有，则提示更多
            //的错误消息。

            // Debug.Assert 就像在说“(条件)必须为真，如果不是，则给出错误(消息)”

            Debug.Assert(_Animations.Length > 1, "Must have more than 1 animation assigned.");

            // 如果你检查 Debug.Assert的定义, 你可以看到它有一个 [Conditional("UNITY_ASSERTIONS")]//有条件的断言宏
            // 属性，这意味着任何对该方法的调用将自动删除，如果该条件编译符号没有定义，
            // 就好像我们在它前面放了“#if unity_assertion”，后面放了“#endif”

            //“unity_assertion”断言宏，它是在Unity编辑器和开发构建中定义的，而不是在发布时构建。
            //在不影响发布游戏的性能的情况下，允许我们验证数据以捕获开发过程中的问题。

            //由于我们可以访问动画的细节，我们也可以对它们施加限制。这个脚本在非循环动画中仍然可以很好地工作，
            //但是由于我们将它用作空闲动画，所以它仅仅播放一次并冻结在最后一帧可能是不合适的。

            Debug.Assert(_Animations[0].Clip.isLooping, "The first animation should be looping.");

            
            //现在，我们真正想做的只是播放第一个动画。
            Play(0);
        }

        /************************************************************************************************************************/

        // Called by a UI Button.
        public void PlaySequence()
        {
            //循环播放
            //同一个协同程序的多个实例可以同时运行，但我们不希望这样，所以我们首先要阻止它们。
            StopAllCoroutines();
            StartCoroutine(CoroutineAnimationSequence());

            // StartCoroutine返回一个Coroutine对象，您可以将其存储在一个字段中，以备以后想要停止那个特定的对象，
            //但是由于我们只有一个对象，所以我们可以停止一切。
        }

        private IEnumerator CoroutineAnimationSequence()
        {
            //对于第一个动画之后的每个动画(从1开始，而不是从0开始), yield会自动接续不会从新运行
            for (int i = 1; i < _Animations.Length; i++)
            {
                //播放动画，等待动画完成
                var state = Play(i);
                yield return state;

                // 我们可以写成一行:
                // yield return Play(i);
                // 但是在“yield”后面加上“Play”看起来不太合适，所以我们用了两行来明确我们想要什么。
            }

            // 回到第一个动作 (待机).
            Play(0);
        }

        /************************************************************************************************************************/

        private AnimancerState Play(int index)
        {
            //我们想要确保文本总是显示当前动画的名称，所以用这个方法包装常规的AnimancerComponent.Play组件，我们只需调用它

            //如果我们希望其他脚本能够播放它们自己的转换，我们可以将这个方法设为public，并给它一个ClipState。转换参数，
            //而不只是一个“索引”。但这种方式只允许我们调用' Play(0) '和' Play(i) '，而不是' Play(_animate[0]) '和' Play(_animate [i]) '

            var animation = _Animations[index];
            _Text.text = animation.Clip.name;
            return _Animancer.Play(animation);
        }

        /************************************************************************************************************************/
    }
}
