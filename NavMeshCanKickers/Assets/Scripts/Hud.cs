using System.Collections;
using System.Linq;
using UnityEngine;

public class Hud : MonoBehaviour
{
    [SerializeField] private HudScore hudScore;
    [SerializeField] private float hudScoreStepY;
    [SerializeField] internal PopTextPool popTextPool;
    [SerializeField] private LogoSetting[] logoes;

    [System.Serializable]
    public class LogoSetting
    {
        public LogoType type;
        public GameObject logo;
    }

    /// <summary>
    /// ゲーム画面ロゴ。
    /// </summary>
    public enum LogoType
    {
        Ready,
        Go,
        Win,
        GameOver,
    }

    private Vector3 hudScorePos;

    void Start()
    {
        hudScore.gameObject.SetActive(false);
        hudScorePos = hudScore.transform.localPosition;
        foreach (var ls in logoes) {
            ls.logo.SetActive(false);
        }
    }

    /// <summary>
    /// ロゴ表示。time 後に消去。time <= 0 なら消去しない。
    /// </summary>
    public void ShowLogo(LogoType type, float time = -1f)
    {
        StartCoroutine(InternalShowLogo(type, time));
    }

    private IEnumerator InternalShowLogo(LogoType type, float time)
    {
        var setting = logoes.FirstOrDefault(ls => ls.type == type);
        if (setting == null) {
            yield break;
        }
        setting.logo.SetActive(true);
        if (time > 0f) {
            yield return new WaitForSeconds(time);
            setting.logo.SetActive(false);
        }
    }

    public HudScore AddHudScore(string name)
    {
        var obj = Instantiate(hudScore.transform, hudScore.transform.parent, false);
        var hudsc = obj.GetComponent<HudScore>();
        obj.gameObject.SetActive(true);
        hudsc.Init(name);
        obj.localPosition = hudScorePos;
        hudScorePos.y += hudScoreStepY;
        return hudsc;
    }
}
