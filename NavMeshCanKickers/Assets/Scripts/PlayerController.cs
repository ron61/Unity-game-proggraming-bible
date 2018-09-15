using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// プレーヤーキャラクタ処理。
/// 目的地到達で缶があればキックする、といったコンポネントの連動の必要な処理や、
/// Pause() で動作の停止(stun効果)のような、多数のコンポネントの設定が必要な処理を行う。
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerAnimatorController animatorCtrl;
    [SerializeField] internal Runner runner;
    [SerializeField] private Kicker kicker;
    [SerializeField] private Jumper jumper;
    [SerializeField] private Climber climber;
    [SerializeField, Header("この距離以下でキックする")] private float kickRange = 2f;

    public ScoreEvent onScoreUpdate = new ScoreEvent();
    public PlayerEvent onPause = new PlayerEvent();

    public class PlayerEvent : UnityEvent { }
    public class ScoreEvent : UnityEvent<int, int> { }

    public bool canMove { get { return !kicker.isKicking && runner.canSetDestination && !isPaused; } }
    public IStageSearcher stageSearcher { get; set; }
    public Vector3 position { get { return mTrans.position; } }
    public bool isPaused { get; private set; }

    private Transform mTrans;

    public int score
    {
        get { return mScore; }
        set
        {
            onScoreUpdate.Invoke(value, value - score);
            mScore = value;
        }
    }

    private int mScore;

    /// <summary>効果中のアイテム</summary>
    private List<ItemEffect> effects = new List<ItemEffect>();

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
        if (runner == null) {
            runner = GetComponent<Runner>();
        }
        if (kicker == null) {
            kicker = GetComponent<Kicker>();
        }
        if (jumper == null) {
            jumper = GetComponent<Jumper>();
        }
        if (climber == null) {
            climber = GetComponent<Climber>();
        }
    }

    void OnEnable()
    {
        runner.onReach.AddListener(OnReach);
        jumper.onJumpStart.AddListener(runner.StopAgent);
        jumper.onJumpEnd.AddListener(runner.StartAgent);
        climber.onClimbStart.AddListener(runner.StopAgent);
        climber.onClimbEnd.AddListener(runner.StartAgent);
        kicker.onKickStart.AddListener(runner.StopAgent);
        kicker.onKickEnd.AddListener(runner.StartAgent);
    }

    void OnDisable()
    {
        runner.onReach.RemoveListener(OnReach);
        jumper.onJumpStart.RemoveListener(runner.StopAgent);
        jumper.onJumpEnd.RemoveListener(runner.StartAgent);
        kicker.onKickStart.RemoveListener(runner.StopAgent);
        kicker.onKickEnd.RemoveListener(runner.StartAgent);
    }

    void Update()
    {
        // アイテム効果の処理
        foreach (var eff in effects) {
            eff.Update(this, Time.deltaTime);
        }
        effects.RemoveAll(eff => !eff.isActive);
    }

    internal void AddEffect(ItemEffect itemEffect)
    {
        effects.Add(itemEffect);
    }

    public void Stop()
    {
        animatorCtrl.Run(false);
        animatorCtrl.Jump(false);
        runner.StopAgent();
    }


    /// <summary>
    /// 動作停止。
    /// </summary>
    /// <param name="isActive"></param>
    public void Pause(bool isActive)
    {
        isPaused = isActive;
        if (isActive) {
            climber.deltaTimeScale = 0f;
            jumper.deltaTimeScale = 0f;
            runner.StopAgent();
            animatorCtrl.Stop();
        } else {
            climber.deltaTimeScale = 1f;
            jumper.deltaTimeScale = 1f;
            runner.StartAgent();
            animatorCtrl.Play();
        }
    }

    public void OnPointInput(Vector3 point)
    {
        if (canMove) {
            runner.destination = point + new Vector3(0f, 1f, 0f);
        }
    }

    /// <summary>
    /// 指定ポイントに近ければキック、遠ければ移動する。
    /// </summary>
    public void KickOrMove(Vector3 point)
    {
        if (canMove) {
            if ((point - mTrans.position).magnitude > kickRange) {
                OnPointInput(point);
            } else {
                kicker.StartKick(point);
            }
        }
    }

    /// <summary>
    /// 目的地まで到着したときは、まわりに蹴れそうなものがあればキックする。
    /// </summary>
    private void OnReach()
    {
        if (stageSearcher == null) {
            return;
        }
        var can = stageSearcher.GetKickableNearest(position);
        if (can != null && Vector3.Distance(can.position, position) <= kickRange) {
            kicker.StartKick(can.position);
        }
    }

    public void Win()
    {
        animatorCtrl.Win();
    }

    public void Lose()
    {
        animatorCtrl.Lose();
    }
}