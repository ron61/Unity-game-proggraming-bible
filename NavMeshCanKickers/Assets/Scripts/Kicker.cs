using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// キックアクションをするオブジェクト=プレーヤーキャラクタ。
/// 対応する蹴られるモノは Kickable。
/// </summary>
public class Kicker : MonoBehaviour
{
    public bool isKicking { get; private set; }

    [SerializeField] private PlayerAnimatorController animatorCtrl;
    [SerializeField] private GameObject kickCollider;

    [SerializeField] private float kickTime = 0.6f;

    public TimeEvent onKickStart = new TimeEvent();
    public TimeEvent onKickEnd = new TimeEvent();
    public HitEvent onKickHit = new HitEvent();

    public class TimeEvent : UnityEvent { }
    public class HitEvent : UnityEvent<Kickable> { }

    private Kickable hitKickable = null;
    private Transform mTrans;

    void Awake()
    {
        OnValidate();
        mTrans = transform;
    }

    void OnValidate()
    {
        if (animatorCtrl == null) {
            animatorCtrl = GetComponentInChildren<PlayerAnimatorController>();
        }
    }

    void Start()
    {
        kickCollider.SetActive(false);
    }

    void OnEnable()
    {
        animatorCtrl.onKickHit.AddListener(OnKickAnimEvent);
    }

    void OnDisable()
    {
        animatorCtrl.onKickHit.RemoveListener(OnKickAnimEvent);
    }

    private void OnKickAnimEvent(int sw)
    {
        kickCollider.SetActive(sw != 0);
    }

    public void StartKick(Vector3 kickPoint)
    {
        if (isKicking) {
            return;
        }
        StartCoroutine(Kick(kickPoint));
    }

    /// <summary>
    /// キックコルーチン。
    /// 向き変えて、アニメーション開始して、コリジョン発生させる。
    /// </summary>
    /// <param name="kickPoint"></param>
    /// <returns></returns>
    internal IEnumerator Kick(Vector3 kickPoint)
    {
        isKicking = true;
        hitKickable = null;

        var tdir = kickPoint - mTrans.position;
        StartCoroutine(LookAt(tdir, 0.3f));
        animatorCtrl.Kick();
        onKickStart.Invoke();
        yield return new WaitForSeconds(kickTime);
        kickCollider.SetActive(false);
        isKicking = false;
        onKickEnd.Invoke();
    }

    /// <summary>
    /// 指定方向を向く。
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    private IEnumerator LookAt(Vector3 forward, float tm)
    {
        var org = mTrans.forward;
        forward.y = org.y;
        for (var t = 0f; t < 1f; t += Time.deltaTime / tm) {
            mTrans.forward = Vector3.Lerp(org, forward, t);
            yield return null;
        }
    }

    /// <summary>
    /// Kickableをキックできるかどうか。
    /// 一回のキックは2つ以上の Kickable(缶や箱)をキックできないようにする。
    /// </summary>
    /// <param name="kickable"></param>
    /// <returns></returns>
    internal bool TryHit(Kickable kickable)
    {
        if (hitKickable == null) {
            hitKickable = kickable;
            return true;
        } else {
            return false;
        }
    }
}
