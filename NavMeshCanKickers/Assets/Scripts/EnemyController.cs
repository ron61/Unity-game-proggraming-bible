using System.Collections;
using UnityEngine;


/// <summary>
/// 敵プレーヤーコントロール。
/// 缶を探して向かって蹴る、を繰り返す。
/// </summary>
public class EnemyController : MonoBehaviour
{
    public PlayerController playerController;

    private IStageSearcher stageSearcher;
    private Coroutine currentCo;
    private Kickable targetCan;
    private bool isTargetAvailable { get { return targetCan != null && targetCan.kickNum == 0; } }

    void Awake()
    {
        OnValidate();
    }

    void Start()
    {
    }

    void OnValidate()
    {
        if (playerController == null) {
            playerController = GetComponent<PlayerController>();
        }
    }

    public void StartControl(IStageSearcher es)
    {
        stageSearcher = es;
        ChangeState(StateSearch());
    }

    private void ChangeState(IEnumerator c)
    {
        if (currentCo != null) {
            StopCoroutine(currentCo);
            currentCo = null;
        }
        if (c != null) {
            currentCo = StartCoroutine(c);
        }
    }

    private IEnumerator StateSearch()
    {
        yield return new WaitUntil(() => playerController.canMove);
        while (!isTargetAvailable) {
            targetCan = stageSearcher.GetHighScoreCan();
            yield return null;
        }
        ChangeState(StateMove());
    }

    private IEnumerator StateMove()
    {
        yield return new WaitUntil(() => playerController.canMove);
        playerController.OnPointInput(targetCan.position);
        yield return new WaitUntil(() => !isTargetAvailable || 2f > (targetCan.position - playerController.position).magnitude);
        if (isTargetAvailable) {
            ChangeState(StateKick());
        } else {
            ChangeState(StateSearch());
        }
    }

    private IEnumerator StateKick()
    {
        yield return new WaitUntil(() => playerController.canMove);
        playerController.KickOrMove(targetCan.position);
        yield return new WaitForSeconds(1.5f);
        ChangeState(StateSearch());
    }
}