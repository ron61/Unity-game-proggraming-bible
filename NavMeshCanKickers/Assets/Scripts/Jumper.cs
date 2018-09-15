using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Jumper : MonoBehaviour
{
    [SerializeField] private float minimumJumpTime = 0.3f;
    [SerializeField] private float jumpGravity = -10f;
    [SerializeField] private float jumpXzSpeed = 6f;
    [SerializeField] private float preJumpTime = 0.2f;
    [SerializeField] private float jumpEndTime = 0.2f;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private PlayerAnimatorController animatorCtrl;

    public float deltaTimeScale = 1f;

    public JumpEvent onJumpStart = new JumpEvent();
    public JumpEvent onJumpEnd = new JumpEvent();

    public class JumpEvent : UnityEvent { }

    private Transform mTrans;
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
    }

    IEnumerator Start()
    {
        while (true) {
            // ladderではないオフメッシュリンクにくるまで待つ
            yield return new WaitUntil(() => IsOnJumpArea());
            // リンクの到達先までジャンプする
            yield return Jump();
        }
    }

    // ジャンプ可能なエリアにいるかどうか。
    private bool IsOnJumpArea()
    {
        if (!agent.isOnOffMeshLink) {
            return false; // オフメッシュリンク上にいないならjumpできるエリアではない 
        }
        const int jumpArea = 2; // jump エリアの番号は Area タブで2
        var link = agent.currentOffMeshLinkData;
        if (link.linkType == OffMeshLinkType.LinkTypeJumpAcross || link.linkType == OffMeshLinkType.LinkTypeDropDown) {
            return true; // 自動生成の水平方向ジャンプか、降りるジャンプエリアの場合。
        }
        if (link.offMeshLink != null && link.offMeshLink.area == jumpArea) {
            return true; // 手動生成の jump エリアの場合。
        }
        return false;
    }

    // agent の offmeshlink データに基づいてジャンプ
    public IEnumerator Jump()
    {
        var link = agent.currentOffMeshLinkData;
        var v = link.endPos - mTrans.position;
        v.y = 0f;
        var jumpTime = Mathf.Max(minimumJumpTime, v.magnitude / jumpXzSpeed); // ジャンプ滞空時間
        yield return Jump(link.endPos, v, jumpTime, jumpGravity);
    }

    // 現在位置からendPosまでをジャンプ処理
    public IEnumerator Jump(Vector3 endPos, Vector3 forward, float jumpTime, float gravity)
    {
        // 書籍本文とすこしちがって、引数でジャンプ先やキャラの向きを受けて動作します。
        // Climber でも使用するのでこのようになっています。
        onJumpStart.Invoke();
        agent.isStopped = true; // エージェント処理停止

        var p0 = mTrans.position; // 初期位置
        var v = endPos - p0; // ジャンプ方向
        var v0 = v.y - gravity * 0.5f; // 放物運動y初速(時間0~1.0正規化前提)

        // アニメーション開始+初動待ち
        StartCoroutine(JumpAnimation(jumpTime)); // キャラをジャンプ方向に向けてアニメーション
        yield return new WaitForSeconds(preJumpTime); // 地面から足が離れるまで

        // ジャンプ移動+放物運動。t = 正規化ジャンプ時間(0~1)
        for (var t = 0f; t < 1f; t += deltaTime / jumpTime) { // t = 正規化ジャンプ時間(0~1)
            var p = Vector3.Lerp(p0, endPos, t);
            p.y = p0.y + v0 * t + 0.5f * gravity * t * t; // 放物運動位置y
            mTrans.position = p;
            mTrans.forward = forward;
            yield return null;
        }

        // オフメッシュリンク移動終了+エージェント処理再開。
        mTrans.position = endPos;
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        onJumpEnd.Invoke();
    }

    // アニメーション再生コルーチン。
    // ジャンプ中の着地アニメーション開始を調整して、
    // time に合わせた時間だけジャンプアニメーションを再生する。
    private IEnumerator JumpAnimation(float time)
    {
        animatorCtrl.Jump(true);
        yield return new WaitForSeconds(time + preJumpTime - jumpEndTime); // 着地アニメ開始まで待つ
        animatorCtrl.Jump(false);
        yield return new WaitForSeconds(jumpEndTime);
    }
}
