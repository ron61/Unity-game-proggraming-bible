using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 箱。Kickable。
/// </summary>
public class Box : MonoBehaviour
{
    [SerializeField] private GameObject boxBodyRoot;
    [SerializeField] private GameObject boxPartsRoot;
    [SerializeField] private Rigidbody[] boxParts;
    [SerializeField] private int life = 5;
    [SerializeField] public Kickable kickable;

    public BoxEvent onCollapse = new BoxEvent();

    public ItemEffect effect { get; set; }

    public class BoxEvent : UnityEvent<Box, Kicker> { }

    private bool isCollapsed = false;
    private int remainingLife;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (kickable == null) {
            kickable = GetComponent<Kickable>();
        }
        if (boxParts.Length == 0) {
            boxParts = boxPartsRoot.GetComponentsInChildren<Rigidbody>();
        }
    }

    void Start()
    {
        boxBodyRoot.SetActive(true);
        boxPartsRoot.SetActive(false);
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
        if (isCollapsed) { // もう壊れている
            return;
        }
        if (kickable.kickNum >= life) {
            isCollapsed = true;
            CollapseModel(kicker);
            onCollapse.Invoke(this, kicker);
        }
    }

    private void CollapseModel(Kicker kicker)
    {
        foreach (var c in boxParts) {
            var v = Random.insideUnitCircle * Random.Range(3f, 5f);
            c.velocity = new Vector3(v.x, Random.Range(2f, 4f), v.y);
            c.angularVelocity = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        }
        boxBodyRoot.SetActive(false);
        boxPartsRoot.SetActive(true);
        Destroy(gameObject, 2f);
    }
}