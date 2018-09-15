using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// NavMeshAgent の移動と走りアニメーションの処理。
/// </summary>
public class Runner : MonoBehaviour
{
    [SerializeField] private float runAnimationSpeed = 2f;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private PlayerAnimatorController animatorCtrl;

    public Vector3 destination
    {
        get { return agent.destination; }
        set
        {
            var pos = GetNavMeshSamplePoint(value);
            isNotReached = pos != agent.destination;
            agent.destination = pos;
        }
    }

    /// <summary>目的地についたら呼ばれるevent。</summary>
    public RunEvent onReach = new RunEvent();

    /// <summary>
    /// 移動可能=destination設定できる状態かどうか。
    /// ジャンプしたり梯子に登っていると agent は isStopped で navmeshにも乗っていないので false になる。
    /// </summary>
    public bool canSetDestination { get { return agent.isOnNavMesh && !agent.isStopped; } }

    public class RunEvent : UnityEvent { }

    private float initialAgentSpeed;
    private Transform mTrans;
    private bool isNotReached = true;

    void Awake()
    {
        OnValidate();
        mTrans = transform;
    }

    void OnValidate()
    {
        if (agent == null) {
            agent = GetComponent<NavMeshAgent>();
        }
        if (animatorCtrl == null) {
            animatorCtrl = GetComponentInChildren<PlayerAnimatorController>();
        }
    }

    void Start()
    {
        initialAgentSpeed = agent.speed;
    }

    /// <summary>
    /// NavMesh処理開始。ジャンプや梯子の上り下り終了時に使う。
    /// </summary>
    public void StartAgent()
    {
        agent.isStopped = false;
    }

    /// <summary>
    /// NavMesh処理停止。ジャンプや梯子の上り下り時に使う。
    /// </summary>
    public void StopAgent()
    {
        agent.isStopped = true;
    }

    void Update()
    {
        // jump やゲームオーバー後などでagentが停止していたり空中にいるなら走りアニメ処理しない。
        if (!agent.isOnNavMesh || agent.isStopped) {
            return;
        }

        // 一定以上の速さで移動しているなら走りアニメーションを出す。 
        var spd = agent.velocity.magnitude;
        var moving = agent.velocity.magnitude >= runAnimationSpeed;
        animatorCtrl.Run(moving); // Animatorを使って走り/立ちアニメーション切り替え

        // 目的地到着したら到着イベント投げる。
        if (isNotReached && IsReached()) {
            onReach.Invoke();
            isNotReached = false;
        }
    }

    private bool IsReached()
    {
        return Vector3.Distance(mTrans.position, agent.destination) <= agent.stoppingDistance;
    }

    internal void SetSpeedScale(float scale)
    {
        agent.speed = scale * initialAgentSpeed;
    }

    private static Vector3 GetNavMeshSamplePoint(Vector3 pos)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, 3.0f, 1 << 0)) {
            return hit.position;
        } else {
            return pos;
        }
    }

}
