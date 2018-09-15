using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Example04 : MonoBehaviour
{
    [SerializeField] private uint version = 1; // アセットバンドルのバージョン

    IEnumerator Start()
    {
        // アセットバンドルキャッシュシステム準備待ち
        yield return new WaitUntil(() => Caching.ready);
        // Webサーバーからアセットバンドルをダウンロード
        using (var wreq = UnityWebRequest.GetAssetBundle("http://localhost:8080/Example01/cana02", version, 0)) {
            yield return wreq.SendWebRequest(); // ダウンロード終了を待つ
            Debug.Log("http response code:" + wreq.responseCode);
            if (wreq.isNetworkError || wreq.isHttpError) {
                Debug.LogError("エラー " + wreq.error);
                yield break;
            }
            // ダウンロードに成功したら、アセットバンドルを取り出してアセットをロードする
            var ab = DownloadHandlerAssetBundle.GetContent(wreq);
            var prefab = ab.LoadAsset<GameObject>("CanA02");
            Instantiate(prefab);
        }
    }
}
