using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 缶
/// </summary>
public class Can : MonoBehaviour
{
    [SerializeField] private float flyTime = 2f;
    [SerializeField] private float verticalSpeed = 6f;
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private Rigidbody mRigidbody;
    [SerializeField] private Transform meshChild;
    [SerializeField] public Kickable kickable;

    public int score = 10;

    public CanKickEvent onCanKicked = new CanKickEvent();
    public class CanKickEvent : UnityEvent<Kicker, Can> { }

    public Vector3 position { get { return mTrans.position; } }

    private Transform mTrans;

    void Awake()
    {
        mTrans = transform;
        OnValidate();
    }

    void OnValidate()
    {
        if (kickable == null) {
            kickable = GetComponent<Kickable>();
        }
        if (mRigidbody == null) {
            mRigidbody = GetComponent<Rigidbody>();
        }
        if (meshChild == null) {
            var mesh = GetComponentInChildren<MeshFilter>();
            meshChild = mesh.transform;
        }
    }

    void Start()
    {
        mRigidbody.isKinematic = true;
    }

    void OnEnable()
    {
        kickable.onKicked.AddListener(OnKicked);
    }

    void OnDisable()
    {
        kickable.onKicked.RemoveListener(OnKicked);
    }

    private void OnKicked(Kicker kicker)
    {
        onCanKicked.Invoke(kicker, this);
        Fly(kicker);
        Destroy(gameObject, flyTime);
    }

    private void Fly(Kicker kicker)
    {
        mRigidbody.isKinematic = false;
        var dir = mTrans.position - kicker.transform.position;
        dir = new Vector3(dir.x, 0f, dir.z).normalized * horizontalSpeed;
        dir.y = verticalSpeed;
        mRigidbody.velocity = dir;
        mRigidbody.angularVelocity = new Vector3(Random.Range(180f, 720f), 0f, 720f);
    }
}