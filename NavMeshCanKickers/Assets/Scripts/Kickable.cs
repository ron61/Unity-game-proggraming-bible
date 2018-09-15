using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Kicker にキックされるモノ。缶や箱。
/// </summary>
public class Kickable : MonoBehaviour
{
    /// <summary>キックされた回数</summary>
    public int kickNum { get; private set; }
    public Vector3 position { get { return mTrans.position; } }

    /// <summary>キックされたときのイベント</summary>
    public KickableEvent onKicked = new KickableEvent();
    public class KickableEvent : UnityEvent<Kicker> { }

    private Transform mTrans;

    void Awake()
    {
        mTrans = transform;
    }

    /// <summary>
    /// collider 衝突。Kicker だったら「蹴られた」
    /// </summary>
    public void OnTriggerEnter(Collider other)
    {
        var otherRoot = other.transform.root;
        var kicker = otherRoot.GetComponent<Kicker>();
        if (kicker != null && kicker.TryHit(this)) {
            onKicked.Invoke(kicker);
            kicker.onKickHit.Invoke(this);
            ++kickNum;
        }
    }
}