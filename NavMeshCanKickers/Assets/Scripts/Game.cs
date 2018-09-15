using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の処理。
/// </summary>
public class Game : MonoBehaviour, IStageSearcher
{
    public static string envSceneName = "";

    [SerializeField] private PlayerInfo playerInfo;
    [SerializeField] private PlayerInfo[] enemyInfo;
    [SerializeField] private GameCamera gameCamera;
    [SerializeField] private PointInput pointer;
    [SerializeField] private Can[] canPrefabs;
    [SerializeField] float canSpawnInitialWait;
    [SerializeField] float canSpawnInterval;
    [SerializeField] int canSpawnNum = 1;
    [SerializeField] private Box boxPrefab;
    [SerializeField] private ItemEffect[] boxItemEffects;
    [SerializeField] private Missile itemMissilePrefab;
    [SerializeField] Hud hud;
    [SerializeField] float gameTime = 120f;
    [SerializeField] TimerEvent onGameStart = new TimerEvent();
    [SerializeField] TimerEvent onTimeUpdate = new TimerEvent();

    [System.Serializable] public class TimerEvent : UnityEvent<int> { }

    [System.Serializable]
    public class PlayerInfo
    {
        [SerializeField] internal string name;
        [SerializeField] internal PlayerController prefab;
        [SerializeField] internal Color hudColor;
    }

    public float remainingGameTime { get; private set; }

    private PlayerController myPlayer;
    private List<PlayerController> allPlayers = new List<PlayerController>();
    private Spawner<Can> canSpawner = new Spawner<Can>();
    private Spawner<Box> boxSpawner = new Spawner<Box>();
    private List<Kickable> kickables = new List<Kickable>();

    IEnumerator Start()
    {
        // ロードしてキャラspawn
        yield return LoadAndSpawn();

        // ゲームタイマー初期化してゲーム画面出す
        InitGameTimer();
        Fade.Open();

        // ready - go!
        yield return ReadyGo();

        // 缶spawn開始
        StartCoroutine(CanSpawnRoutine());

        // タイムオーバーまで待つ
        yield return WaitUntilTimeOver();

        // ゲームオーバー表示
        yield return GameOver();

        // タイトル画面へ戻る
        yield return Fade.Close();
        yield return SceneManager.LoadSceneAsync("Title"); // タイトルへ戻る
    }

    /// <summary>
    /// ゲームフィールドのロードとオブジェクトのspawn
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadAndSpawn()
    {
        // 背景ロード。何かすでにロード済みならロードしない(テストのためにGameとGameEnv両方シーンがある状態等)
        if (EnvSetting.Instance == null) {
            var sceneName = string.IsNullOrEmpty(envSceneName) ? "GameEnv" : envSceneName;
            yield return EnvSetting.LoadEnvScene(sceneName);
        }
        var envSetting = EnvSetting.Instance;

        // プレーヤーspawn
        var posq = new Queue<Transform>(envSetting.playerPositions.OrderBy(e => Random.value).ToList());
        SpawnMyPlayer(posq);
        SpawnEnemies(posq);

        // 缶spawner初期化
        canSpawner.SetSpawnPositions(envSetting.canPositions);
        canSpawner.onSpawn.AddListener(OnCanSpawn);

        // 箱全部spawn
        boxSpawner.SetSpawnPositions(envSetting.boxPositions);
        boxSpawner.onSpawn.AddListener(OnBoxSpawn);
        boxSpawner.SpawnAll(boxPrefab); // 全位置にspawn
    }

    private void InitGameTimer()
    {
        remainingGameTime = gameTime;
        onTimeUpdate.Invoke((int) gameTime);
    }

    /// <summary>
    /// 操作プレーヤーspawn。
    /// </summary>
    /// <param name="posq">生成位置のキュー</param>
    private void SpawnMyPlayer(Queue<Transform> posq)
    {
        myPlayer = SpawnPlayer(playerInfo, posq.Dequeue());
        gameCamera.SetTarget(myPlayer.transform);
    }

    /// <summary>
    /// 敵プレーヤー全部spawn
    /// </summary>
    /// <param name="posq">生成位置のキュー</param>
    private void SpawnEnemies(Queue<Transform> posq)
    {
        foreach (var enInfo in enemyInfo) {
            var epl = SpawnPlayer(enInfo, posq.Dequeue());
            // EnemyController をつけて、ゲーム開始時に動作開始するように設定。
            var econ = epl.gameObject.AddComponent<EnemyController>();
            onGameStart.AddListener(t => econ.StartControl(this));
        }
    }

    /// <summary>
    /// Ready - Go サイン
    /// </summary>
    private IEnumerator ReadyGo()
    {
        hud.ShowLogo(Hud.LogoType.Ready, 2f);
        yield return new WaitForSeconds(2f);
        hud.ShowLogo(Hud.LogoType.Go, 1f);

        ConnectPlayerInput(myPlayer);
    }

    /// <summary>
    /// ゲーム時間処理
    /// </summary>
    private IEnumerator WaitUntilTimeOver()
    {
        onGameStart.Invoke((int) remainingGameTime);
        while (remainingGameTime > 0f) {
            var n = (int) remainingGameTime;
            remainingGameTime = Mathf.Max(0f, remainingGameTime - Time.deltaTime);
            if ((int) remainingGameTime != n) { // 残り時刻の整数部が変化した。
                onTimeUpdate.Invoke((int) remainingGameTime);
            }
            yield return null;
        }
        remainingGameTime = 0f;
    }

    /// <summary>
    /// ゲーム終了表示
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameOver()
    {
        DisconnectPlayerInput(myPlayer);
        allPlayers.ForEach(pl => pl.enabled = false);
        var maxsc = allPlayers.Max(pc => pc.score);
        foreach (var pl in allPlayers) {
            StartCoroutine(StopPlayer(pl, pl.score == maxsc));
        }
        hud.ShowLogo(myPlayer.score == maxsc ? Hud.LogoType.Win : Hud.LogoType.GameOver);
        yield return new WaitForSeconds(5f);
    }

    private IEnumerator StopPlayer(PlayerController pl, bool isWinner)
    {
        yield return new WaitUntil(() => pl.canMove);
        pl.Stop();
        if (isWinner) {
            pl.Win();
        } else {
            pl.Lose();
        }
    }

    /// <summary>
    /// プレーヤー1体spawn。hud(スコア)との関連付けする。
    /// </summary>
    /// <param name="pinfo"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private PlayerController SpawnPlayer(PlayerInfo pinfo, Transform pos)
    {
        var pobj = Instantiate(pinfo.prefab.transform, pos.position, pos.rotation);
        var pc = pobj.GetComponent<PlayerController>();
        var hudsc = hud.AddHudScore(pinfo.name);
        hudsc.SetColor(pinfo.hudColor);
        pc.onScoreUpdate.AddListener(hudsc.OnUpdateScore);
        pc.stageSearcher = this;
        allPlayers.Add(pc);
        return pc;
    }

    /// <summary>
    /// プレーヤー入力受付開始。
    /// </summary>
    private void ConnectPlayerInput(PlayerController ply)
    {
        pointer.onPoint.AddListener(ply.OnPointInput);
        pointer.onDrag.AddListener(ply.OnPointInput);
        ply.runner.onReach.AddListener(pointer.HideTarget);
    }

    /// <summary>
    /// プレーヤー入力受付終了
    /// </summary>
    private void DisconnectPlayerInput(PlayerController ply)
    {
        pointer.onPoint.RemoveListener(ply.OnPointInput);
        pointer.onDrag.RemoveListener(ply.OnPointInput);
        ply.runner.onReach.RemoveListener(pointer.HideTarget);
    }

    /// <summary>
    /// 缶を一定時間おきにspawn
    /// </summary>
    private IEnumerator CanSpawnRoutine()
    {
        yield return new WaitForSeconds(canSpawnInitialWait);
        while (remainingGameTime > 0f) {
            for (var i = 0; i < canSpawnNum && canSpawner.hasSpawnPos; ++i) {
                var prefab = canPrefabs[Random.Range(0, canPrefabs.Length)];
                canSpawner.Spawn(prefab);
            }
            yield return new WaitUntil(() => canSpawner.hasSpawnPos);
            yield return new WaitForSeconds(canSpawnInterval);
        }
    }

    /// <summary>
    /// 缶spawn時初期化
    /// </summary>
    /// <param name="can"></param>
    private void OnCanSpawn(Can can)
    {
        can.onCanKicked.AddListener(OnCanKicked);
        kickables.Add(can.kickable);
    }

    /// <summary>
    /// 缶蹴り処理。スコア加算してspawn位置を再spawn可能にする。
    /// </summary>
    /// <param name="can"></param>
    /// <param name="hitter"></param>
    private void OnCanKicked(Kicker kicker, Can can)
    {
        // score update
        var pc = kicker.GetComponent<PlayerController>();
        if (pc != null) {
            pc.score += can.score;
        }

        // restore can spawn position
        canSpawner.Delete(can);
    }

    /// <summary>
    /// 箱spawn時初期化
    /// </summary>
    /// <param name="box"></param>
    private void OnBoxSpawn(Box box)
    {
        box.effect = boxItemEffects[Random.Range(0, boxItemEffects.Length)];
        box.onCollapse.AddListener(OnBoxCollapse); // 箱が壊れた時
        kickables.Add(box.kickable);
    }

    /// <summary>
    /// 箱が壊れたときの処理。アイテムミサイルを射出。
    /// </summary>
    /// <param name="box"></param>
    /// <param name="kicker"></param>
    private void OnBoxCollapse(Box box, Kicker kicker)
    {
        var missile = itemMissilePrefab.Create(box.transform.position);
        missile.Shoot(kicker.transform, box.effect);
        missile.onHitTarget.AddListener(OnHitMissile); // ミサイルが当たった
    }

    /// <summary>
    /// アイテムミサイルが当たった
    /// </summary>
    /// <param name="eff">発動するアイテム効果</param>
    /// <param name="t">アイテムの当たったオブジェクト(キャラ)</param>
    private void OnHitMissile(ItemEffect eff, Transform t)
    {
        var player = t.GetComponent<PlayerController>();
        if (player != null) {
            hud.popTextPool.Pop(eff.PopString(), t.position);
            eff.StartEffect(player, this);
        }
    }

    #region IStageSearcher インターフェース

    Kickable IStageSearcher.GetHighScoreCan()
    {
        // あまり速い方法ではない
        return canSpawner.allObjects.OrderByDescending(c => c.score).Select(c => c.kickable).FirstOrDefault();
    }

    Kickable IStageSearcher.GetKickableNearest(Vector3 position)
    {
        // あまり速い方法ではない
        kickables.RemoveAll(obj => obj == null);
        return kickables.OrderBy(obj => Vector3.Distance(obj.position, position)).FirstOrDefault();
    }

    IEnumerable<PlayerController> IStageSearcher.GetEnemies(PlayerController me)
    {
        return allPlayers.Where(pc => pc != me);
    }

    #endregion
}
