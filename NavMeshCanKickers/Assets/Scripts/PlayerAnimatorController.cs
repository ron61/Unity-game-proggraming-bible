using UnityEngine;
using UnityEngine.Events;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator mAnimator;

    public AnimationEvent onKickHit = new AnimationEvent();
    public class AnimationEvent : UnityEvent<int> { }

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (mAnimator == null) {
            mAnimator = GetComponent<Animator>();
        }
    }

    internal void Stop()
    {
        mAnimator.speed = 0f;
    }

    internal void Play()
    {
        mAnimator.speed = 1f;
    }

    internal void Run(bool moving)
    {
        mAnimator.SetBool("run", moving);
    }

    internal void Jump(bool v)
    {
        mAnimator.SetBool("jump", v);
    }

    internal void Kick()
    {
        mAnimator.SetTrigger("kick");
    }

    internal void Climb(bool v)
    {
        mAnimator.SetBool("climb", v);
        if (v) {
            Jump(false);
            Run(false);
        }
    }

    // AnimationEvent として clip に設定されて Animator から呼ばれる
    public void OnAnimationKickHit(int sw)
    {
        onKickHit.Invoke(sw);
    }

    public void Win()
    {
        mAnimator.SetTrigger("win");
    }

    public void Lose()
    {
        mAnimator.SetTrigger("lose");
    }
}