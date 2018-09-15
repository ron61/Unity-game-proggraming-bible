using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example01 : MonoBehaviour
{
    IEnumerator Start()
    {
        // Example01/cana02 アセットバンドルファイルをロード
        var ab = AssetBundle.LoadFromFile("Example01/cana02");
        if (ab == null) {
            yield break;
        }
        // ロードしたファイルの中から CanA02 GameObject(プレハブ) をロードしてInstantiate
        var prefab = ab.LoadAsset<GameObject>("CanA02");
        Instantiate(prefab);
        // クリック/タッチ待ち
        yield return new WaitUntil(() => Input.GetMouseButton(0));
        // アセットバンドルをunload。true なので派生するものすべてのメモリ開放
        ab.Unload(true);
        Debug.Log("Unloaded");
    }
}
