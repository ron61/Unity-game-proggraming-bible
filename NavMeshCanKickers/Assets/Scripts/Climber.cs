using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// 梯子の上り下り。ジャンプで降りたり飛びついたりするところは Jumperを使う。
/// </summary>
public class Climber : MonoBehaviour
{
    [SerializeField] private float climbSpeed = 1.5f;
    [SerializeField] private float climbJumpGravity = -5f;
    [SerializeField] private Jumper jumper;
    [SerializeField] private PlayerAnimatorController animatorCtrl;
    [SerializeField] private NavMeshAgent agent;

    public ClimbEvent onClimbStart = new ClimbEvent();
    public ClimbEvent onClimbEnd = new ClimbEvent();

    public class ClimbEvent : UnityEvent { }

    private Transform mTrans;
    [NonSerialized] public float deltaTimeScale = 1f;
    private float deltaTime { get { return Time.deltaTime * deltaTimeScale; } }

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
        if (jumper == null) {
            jumper = GetComponent<Jumper>();
        }
    }

    IEnumerator Start()
    {
        while (true) {
            // ladder エリア上にくるまで待つ
            yield return new WaitUntil(() => IsOnLadderOffMeshLink(agent));
            // 上り下り処理
            yield return Climb();
        }
    }

    // ladder エリアにいるかどうかの判定
    public static bool IsOnLadderOffMeshLink(NavMeshAgent agent)
    {
        const int ladderArea = 2; // ladder エリアは Areaタブで2番になっている
        if (!agent.isOnOffMeshLink) {
            return false; // オフメッシュリンク上にいない
        }
        // offMeshLink が手動で作ったリンクで ladder エリア 
        var link = agent.currentOffMeshLinkData;
        return link.offMeshLink != null && link.offMeshLink.area != ladderArea;
    }

    private IEnumerator Climb()
    {
        onClimbStart.Invoke();

        var link = agent.currentOffMeshLinkData;
        var fwd = link.offMeshLink.endTransform.forward;
        var btm = link.offMeshLink.endTransform.position;
        var p1 = new Vector3(btm.x, link.startPos.y, btm.z);
        var p2 = new Vector3(btm.x, link.endPos.y, btm.z);

        if (p2.y - p1.y < 0f) { // 降りる時: 近寄る→ジャンプで飛びつかみ→降りる
            yield return CloseTo(link.startPos, 0.4f);
            yield return JumpTo(p1, fwd);
            agent.isStopped = true; // エージェント処理停止
            yield return ClimbOrDescend(p1, p2, fwd);
        } else { // 登る時:近寄る→登る→ジャンプで飛び降り
            agent.isStopped = true; // エージェント処理停止
            yield return CloseTo(p1, 0.3f);
            yield return ClimbOrDescend(p1, p2, fwd);
            yield return JumpTo(link.endPos, fwd);
        }

        // オフメッシュリンク移動終了+エージェント処理再開。
        animatorCtrl.Climb(false);
        mTrans.position = link.endPos;
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        onClimbEnd.Invoke();
    }

    private IEnumerator ClimbOrDescend(Vector3 p1, Vector3 p2, Vector3 forward)
    {
        animatorCtrl.Climb(true);
        var climbTime = Mathf.Abs(p2.y - p1.y) / climbSpeed;
        for (var t = 0f; t < 1f; t += deltaTime / climbTime) {
            mTrans.position = Vector3.Lerp(p1, p2, t);
            mTrans.forward = forward;
            yield return null;
        }
        mTrans.position = p2;
    }

    private IEnumerator CloseTo(Vector3 targetPos, float time)
    {
        var p0 = mTrans.position;
        for (var t = 0f; t < 1f; t += deltaTime * deltaTimeScale / time) {
            mTrans.position = Vector3.Lerp(p0, targetPos, t);
            yield return null;
        }
        mTrans.position = targetPos;
    }

    private IEnumerator JumpTo(Vector3 endPos, Vector3 forward)
    {
        yield return jumper.Jump(endPos, forward, 0.3f, climbJumpGravity);
    }
}
