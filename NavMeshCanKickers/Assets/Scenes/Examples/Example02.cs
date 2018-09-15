using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Example02 : MonoBehaviour
{
    IEnumerator Start()
    {
        // Example01/testevn アセットバンドルファイルをロード
        var ab = AssetBundle.LoadFromFile("Example01/testenv");
        if (ab == null) {
            yield break;
        }
        // TestEnv シーンをロードする
        yield return SceneManager.LoadSceneAsync("TestEnv", LoadSceneMode.Additive);
        // クリック/タッチ待ち
        yield return new WaitUntil(() => Input.GetMouseButton(0));
        // アセットバンドルをunload。true なので派生するものすべてのメモリ開放
        ab.Unload(true);
        Debug.Log("Unloaded");
    }
}
