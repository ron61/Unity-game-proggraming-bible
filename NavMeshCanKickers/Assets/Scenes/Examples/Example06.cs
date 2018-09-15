using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Example06 : MonoBehaviour
{
    // ロードしたマニフェストバンドル
    private AssetBundleManifest manifest;
    // ロードしたアセットバンドル全て
    private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

    // 依存関係から正しくアセットをロードするサンプル
    IEnumerator Start()
    {
        var urlBase = "http://localhost:8080/Example05/";
        // マニフェストバンドルのロード
        yield return LoadManifest(urlBase, "Example05");
        // players アセットバンドルのロード。依存するアセットバンドルもロードする。
        yield return LoadAssetBundle(urlBase, "players");
        // Player01 プレハブを players バンドルからロードして表示
        var prefab = loadedBundles["players"].LoadAsset<GameObject>("Player01");
        Instantiate(prefab);
    }

    // マニフェストバンドルのダウンロード
    private IEnumerator LoadManifest(string urlBase, string abname)
    {
        yield return Download(urlBase, abname);
        manifest = loadedBundles[abname].LoadAsset<AssetBundleManifest>("assetbundlemanifest");
    }

    // マニフェストバンドルを利用してアセットバンドルのダウンロードをする。
    private IEnumerator LoadAssetBundle(string urlBase, string abname)
    {
        // 依存するアセットバンドルを先に(ダウン)ロード
        // manifest は起動時に先にロード済みのマニフェストオブジェクト。
        foreach (var dep in manifest.GetAllDependencies(abname)) {
            yield return Download(urlBase, dep);
        }
        // 引数で指定された本体をロード
        yield return Download(urlBase, abname);
    }

    // 指定URLのアセットバンドルをダウンロード。結果は loadedBundle フィールドに設定。
    private IEnumerator Download(string urlBase, string abname)
    {
        if (loadedBundles.ContainsKey(abname)) { // ロード済みなら即終了
            yield break;
        }
        var url = Path.Combine(urlBase, abname);
        using (var wreq = UnityWebRequest.GetAssetBundle(url)) {
            yield return wreq.SendWebRequest(); // ダウンロード終了を待つ
            Debug.Log("http response code:" + wreq.responseCode);
            if (wreq.isNetworkError || wreq.isHttpError) {
                Debug.LogError("エラー " + wreq.error);
                yield break;
            }
            loadedBundles[abname] = DownloadHandlerAssetBundle.GetContent(wreq);
        }
    }
}
