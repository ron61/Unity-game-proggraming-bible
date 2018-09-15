using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Example03 : MonoBehaviour
{
    IEnumerator Start()
    {
        // Webサーバーからアセットバンドルをダウンロード
        using (var wreq = UnityWebRequest.GetAssetBundle("http://localhost:8080/Example01/cana02")) {
            yield return wreq.SendWebRequest(); // ダウンロード終了を待つ
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
