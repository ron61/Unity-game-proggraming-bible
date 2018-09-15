using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// アイテムを運ぶミサイル。
/// ターゲットは確定していて、ターゲット以外にはヒットしない=効果が発生しない。
/// </summary>
public class Missile : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float speed = 12f;
    [SerializeField] private AnimationCurve lerpAnim;
    [SerializeField] private float lerpTime = 1f;
    [SerializeField] private Vector3 initialDirection;
    [SerializeField] private float hitRange = 1f;
    [SerializeField] private float noHitTime = 0.2f;

    public MissileEvent onHitTarget = new MissileEvent();
    public HitEvent onHitMissile = new HitEvent();
    public class MissileEvent : UnityEvent<ItemEffect, Transform> { }
    public class HitEvent : UnityEvent<Missile, Transform> { }

    private float currentTime;
    private ItemEffect itemEffect;
    private Transform mTrans;

    void Start()
    {
        mTrans = transform;
    }

    void OnEnable()
    {
        currentTime = 0f;
        transform.forward = initialDirection;
    }

    public void Shoot(Transform aimTarget, ItemEffect effect)
    {
        target = aimTarget;
        itemEffect = effect;
        gameObject.SetActive(true);
    }

    void Update()
    {
        mTrans.position += mTrans.forward * speed * Time.deltaTime;
        if (target == null) {
            return;
        }
        currentTime += Time.deltaTime;
        var coef = lerpAnim.Evaluate(currentTime / lerpTime);
        var dir = target.position - mTrans.position;
        mTrans.forward = Vector3.Slerp(mTrans.forward, dir.normalized, coef).normalized;
        if (currentTime >= noHitTime && dir.magnitude < hitRange) {
            onHitTarget.Invoke(itemEffect, target);
            onHitMissile.Invoke(this, target);
            Destroy(gameObject);
        }
    }

    public Missile Create(Vector3 position)
    {
        var mobj = Instantiate(transform, position, Quaternion.identity);
        return mobj.GetComponent<Missile>();
    }
}
