using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトル画面。キャラにぐるぐるステージ上を走らせる。
/// </summary>
public class Title : MonoBehaviour
{
    [SerializeField] private Runner[] runners;
    [SerializeField] private GameObject logo;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private string envSceneName;

    IEnumerator Start()
    {
        logo.SetActive(false);

        // 背景ロード
        yield return EnvSetting.LoadEnvScene(envSceneName);
        var envSetting = EnvSetting.Instance;

        // 開幕カメラ
        Fade.Open();
        logo.SetActive(true);
        yield return DescendCamera();
        yield return new WaitForSeconds(1f);

        // キャラ走り出す
        foreach (var r in runners) {
            StartCoroutine(DemoRun(r, envSetting.demoTraversalPoints));
        }

        // ゲーム開始待ち
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Z) || Input.GetMouseButton(0) || Input.GetMouseButton(1));
        yield return Fade.Close();
        yield return SceneManager.LoadSceneAsync("Game");
    }

    private IEnumerator DescendCamera()
    {
        var orgCamPos = mainCamera.position;
        var height = 15f;
        var tm = 3f;
        for (var t = 1f; t >= 0f; t -= Time.deltaTime / tm) {
            mainCamera.position = orgCamPos + Vector3.up * height * t;
            yield return null;
        }
        mainCamera.position = orgCamPos;
    }

    private IEnumerator DemoRun(Runner runner, Transform[] traversalPoints)
    {
        runner.StartAgent();
        while (true) {
            var points = traversalPoints.OrderBy(t => Random.value).ToArray();
            foreach (var pnt in points) {
                runner.destination = pnt.position;
                yield return new WaitUntil(() => (pnt.position - runner.transform.position).magnitude < 1f);
            }
        }
    }
}
