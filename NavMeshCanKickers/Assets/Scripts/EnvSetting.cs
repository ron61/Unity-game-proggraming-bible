using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームフィールドのロードと設定。
/// オブジェクトのスポーン位置情報を持つ。
/// </summary>
public class EnvSetting : MonoBehaviour
{
    [SerializeField] private Transform playerPositionRoot;
    [SerializeField] private Transform canPositionRoot;
    [SerializeField] private Transform boxPositionRoot;
    [SerializeField] private Transform[] traversalPoints;

    public static EnvSetting Instance { get; private set; }

    public IEnumerable<Transform> playerPositions { get { return GetChildren(playerPositionRoot); } }
    public IEnumerable<Transform> canPositions { get { return GetChildren(canPositionRoot); } }
    public IEnumerable<Transform> boxPositions { get { return GetChildren(boxPositionRoot); } }

    public Transform[] demoTraversalPoints { get { return traversalPoints; } }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (var mf in gameObject.GetComponentsInChildren<MeshFilter>()) {
            var c = mf.gameObject.AddComponent<MeshCollider>();
            c.sharedMesh = mf.sharedMesh;
        }
    }

    private static IEnumerable<Transform> GetChildren(Transform t)
    {
        return Enumerable.Range(0, t.childCount).Select(n => t.GetChild(n));
    }

    /// <summary>
    /// ゲーム背景のロード。ロードすると EnvSetting.Instance で背景の情報(スポーン位置)を取得できる。
    /// </summary>
    public static IEnumerator LoadEnvScene(string envSceneName)
    {
        var scn = SceneManager.GetSceneByName(envSceneName);
        if (!scn.isLoaded) {
            yield return SceneManager.LoadSceneAsync(envSceneName, LoadSceneMode.Additive);
        }
        Instance = EnvSetting.Instance;
    }
}