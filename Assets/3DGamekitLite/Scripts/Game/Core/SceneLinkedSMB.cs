using UnityEngine;
using UnityEngine.Animations;

namespace Gamekit3D
{
    /// <summary>
    /// 状态机管理器(总控) 以 Monobehaviour 为单位
    /// 如果想实现一个状态, 可以继承此类来实现
    /// 比如: public class ChomperSMBAttack : SceneLinkedSMB<ChomperBehavior>
    /// </summary>
    /// <typeparam name="TMonoBehaviour"></typeparam>
    public class SceneLinkedSMB<TMonoBehaviour> : SealedSMB where TMonoBehaviour : MonoBehaviour
    {
        protected TMonoBehaviour m_MonoBehaviour;// 行为控制器 挂在Entity上
    
        bool m_FirstFrameHappened;
        bool m_LastFrameHappened;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="animator"> 动画控制器 </param>
        /// <param name="monoBehaviour"> 当前的Mono脚本 </param>
        public static void Initialise (Animator animator, TMonoBehaviour monoBehaviour)
        {
            // 获取动画控制器上的所有行为
            SceneLinkedSMB<TMonoBehaviour>[] sceneLinkedSMBs = animator.GetBehaviours<SceneLinkedSMB<TMonoBehaviour>>();

            // 遍历 --> 调用其初始化函数
            for (int i = 0; i < sceneLinkedSMBs.Length; i++)
            {
                sceneLinkedSMBs[i].InternalInitialise(animator, monoBehaviour);
            }

        }

        // 初始化函数
        protected void InternalInitialise (Animator animator, TMonoBehaviour monoBehaviour)
        {
            m_MonoBehaviour = monoBehaviour;
            OnStart (animator);
        }

        /// <summary>
        /// 进入到A状态时
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        /// <param name="controller"></param>
        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            m_FirstFrameHappened = false;

            OnSLStateEnter(animator, stateInfo, layerIndex);
            OnSLStateEnter (animator, stateInfo, layerIndex, controller);
        }

        /// <summary>
        /// A状态持续更新
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo">动画状态信息</param>
        /// <param name="layerIndex"></param>
        /// <param name="controller"></param>
        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if(!animator.gameObject.activeSelf)//如果gameObject没有被激活， 则不会调用此函数
                return;
        
            if (animator.IsInTransition(layerIndex) && animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
            {// 如果在Transition中
                OnSLTransitionToStateUpdate(animator, stateInfo, layerIndex);
                OnSLTransitionToStateUpdate(animator, stateInfo, layerIndex, controller);
            }

            if (!animator.IsInTransition(layerIndex) && m_FirstFrameHappened)
            {// 如果不在Transition中 && 第一帧已经过去
                OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);
                OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex, controller);
            }
        
            if (animator.IsInTransition(layerIndex) && !m_LastFrameHappened && m_FirstFrameHappened)
            {// 如果在Transition中 && 第一帧已经过去 && 最后一帧没有过去
                m_LastFrameHappened = true;
            
                OnSLStatePreExit(animator, stateInfo, layerIndex);
                OnSLStatePreExit(animator, stateInfo, layerIndex, controller);
            }

            if (!animator.IsInTransition(layerIndex) && !m_FirstFrameHappened)
            {// 不在Transition中 && 第一帧没有被调用 ---> 完全进入到B状态之后第一帧被调用
                m_FirstFrameHappened = true;

                OnSLStatePostEnter(animator, stateInfo, layerIndex);
                OnSLStatePostEnter(animator, stateInfo, layerIndex, controller);
            }

            if (animator.IsInTransition(layerIndex) && animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
            {// transition的每帧都会调用此函数
                OnSLTransitionFromStateUpdate(animator, stateInfo, layerIndex);
                OnSLTransitionFromStateUpdate(animator, stateInfo, layerIndex, controller);
            }
        }

        /// <summary>
        /// 从A状态退出
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        /// <param name="controller"></param>
        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            m_LastFrameHappened = false;

            OnSLStateExit(animator, stateInfo, layerIndex);
            OnSLStateExit(animator, stateInfo, layerIndex, controller);
        }

        /// <summary>
        /// Called by a MonoBehaviour in the scene during its Start function.
        /// 当挂载在Entity上的MonoBehaviour的OnEnable函数被调用时, 会调用其Animator下的所有状态的OnStart方法.
        /// 如果子类实现了这个OnStart方法的话.
        /// </summary>
        public virtual void OnStart(Animator animator) { }

        /// <summary>
        /// Called before Updates when execution of the state first starts (on transition to the state).
        /// 进入到此状态时会被调用(有一个函数重载)
        /// </summary>
        public virtual void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    
        /// <summary>
        /// Called after OnSLStateEnter every frame during transition to the state.
        /// A状态过渡到B状态的时候，会调用此函数(会调用很多次)
        /// </summary>
        public virtual void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called on the first frame after the transition to the state has finished.
        /// transition结束后的第一帧会调用此函数
        /// 在OnSLStateEnter之后调用
        /// </summary>
        public virtual void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called every frame after PostEnter when the state is not being transitioned to or from.
        /// 在PostEnter之后调用(会调用多次) 并且不处于transition状态
        /// </summary>
        public virtual void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called on the first frame after the transition from the state has started.  Note that if the transition has a duration of less than a frame, this will not be called.
        /// 进入到transition之后第一帧调用
        /// 注意：如果transition在一帧之内完成了， 则不会调用此函数
        /// </summary>
        public virtual void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called after OnSLStatePreExit every frame during transition to the state.
        /// 在transition的每帧都会调用此函数. 在OnSLStatePreExit之后调用.
        /// </summary>
        public virtual void OnSLTransitionFromStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called after Updates when execution of the state first finshes (after transition from the state).
        /// 在transition完成之后调用此函数. 在OnSLTransitionFromStateUpdate之后调用
        /// </summary>
        public virtual void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// Called before Updates when execution of the state first starts (on transition to the state).
        /// 进入到此状态时会被调用(有一个函数重载)
        /// </summary>
        public virtual void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called after OnSLStateEnter every frame during transition to the state.
        /// A状态过渡到B状态的时候，会调用此函数(会调用很多次)(有一个函数重载)
        /// </summary>
        public virtual void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called on the first frame after the transition to the state has finished.
        /// transition结束后的第一帧会调用此函数(有一个函数重载)
        /// 在OnSLStateEnter之后调用
        /// </summary>
        public virtual void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called every frame when the state is not being transitioned to or from.
        /// 在PostEnter之后调用(会调用多次) 并且不处于transition状态
        /// </summary>
        public virtual void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called on the first frame after the transition from the state has started.  Note that if the transition has a duration of less than a frame, this will not be called.
        /// 进入到transition之后第一帧调用
        /// 注意：如果transition在一帧之内完成了， 则不会调用此函数
        /// </summary>
        public virtual void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called after OnSLStatePreExit every frame during transition to the state.
        /// 在transition的每帧都会调用此函数. 在OnSLStatePreExit之后调用.
        /// </summary>
        public virtual void OnSLTransitionFromStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        /// <summary>
        /// Called after Updates when execution of the state first finshes (after transition from the state).
        /// 在transition完成之后调用此函数. 在OnSLTransitionFromStateUpdate之后调用
        /// </summary>
        public virtual void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }
    }

    //This class repalce normal StateMachineBehaviour. It add the possibility of having direct reference to the object
    //the state is running on, avoiding the cost of retrienving it through a GetComponent every time.
    //c.f. Documentation for more in depth explainations.
    /// <summary>
    /// 状态机类
    /// 用这个类来替代默认的StateMachinBehaviour. 如果此状态正在运行，使用它可以直接访问状态机对象， 避免了每次通过GetComponent
    /// 去获取
    /// </summary>
    public abstract class SealedSMB : StateMachineBehaviour
    {
        /// <summary>
        /// 进入到某一状态
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// 持续在某一状态
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        /// <summary>
        /// 某一状态结束时
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    }
}