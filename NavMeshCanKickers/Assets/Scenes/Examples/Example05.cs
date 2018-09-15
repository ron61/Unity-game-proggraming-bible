using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Example05 : MonoBehaviour
{
    // マニフェストバンドルをロードするサンプル
    IEnumerator Start()
    {
        var urlBase = "http://localhost:8080/Example05/";
        var manifestUrl = Path.Combine(urlBase, "Example05");
        // マニフェストバンドルのロード
        using (var wreq = UnityWebRequest.GetAssetBundle(manifestUrl)) {
            yield return wreq.SendWebRequest(); // ダウンロード終了を待つ
            Debug.Log("http response code:" + wreq.responseCode);
            if (wreq.isNetworkError || wreq.isHttpError) {
                Debug.LogError("エラー " + wreq.error);
                yield break;
            }
            // バンドルからマニフェストオブジェクト AssetBundleManifest を取り出す。
            // マニフェストオブジェクトの名前は必ず "assetbundlemanifest"
            var ab = DownloadHandlerAssetBundle.GetContent(wreq);
            var manifest = ab.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
            // 全アセットバンドル名と依存するアセットバンドル名を表示する
            var sb = new StringBuilder();
            foreach (var bn in manifest.GetAllAssetBundles()) {
                // deps = バンドル bn の依存するバンドル名の配列
                var deps = manifest.GetAllDependencies(bn);
                sb.Append("bundle:").Append(bn)
                    .Append("   dependencies:").Append(string.Join("/", deps))
                    .AppendLine();
            }
            Debug.Log(sb.ToString());
        }
    }
}
