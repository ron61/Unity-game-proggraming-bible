﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Example07 : MonoBehaviour
{
    // ロードしたマニフェストバンドル
    private AssetBundleManifest manifest;
    // ロード済みアセットバンドルの記録。アセットバンドル名からAssetBundleオブジェクトを引ける辞書。
    private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
    // アセットとアセットバンドルの対応付
    private Map assetBundleMap;

    // 依存関係から正しくアセットをロードするサンプル
    IEnumerator Start()
    {
        var urlBase = "http://localhost:8080/Example05/";
        // マニフェストバンドルのロード
        yield return LoadManifest(urlBase, "Example05");
        // アセット名とアセットバンドル名の対応付け AssetBundleMap.json のロード
        yield return LoadAssetBundleMap(urlBase, "AssetBundleMap.json");
        // アセットバンドルのロード。依存するアセットバンドルもロードする。
        // ロードするアセットバンドル名はアセット名から検索する。
        var abname = GetAssetBundleName("Player01");
        yield return LoadAssetBundle(urlBase, abname);
        // Player01 プレハブを players バンドルからロードして表示
        var prefab = loadedBundles[abname].LoadAsset<GameObject>("Player01");
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

    // 指定のurlの下のmapName を、Map クラスのjsonとみなしてダウンロード。
    // 結果は assetbundleMap フィールドに設定。
    private IEnumerator LoadAssetBundleMap(string urlBase, string mapName)
    {
        // UnityWebRequest.Get() でWebサーバーからテキストファイルをGet methodでダウンロードする。
        using (var wreq = UnityWebRequest.Get(Path.Combine(urlBase, mapName))) {
            yield return wreq.SendWebRequest(); // ダウンロード終了を待つ
            Debug.Log("http response code:" + wreq.responseCode);
            if (wreq.isNetworkError || wreq.isHttpError) {
                Debug.LogError("エラー " + wreq.error);
                yield break;
            }
            assetBundleMap = JsonUtility.FromJson<Map>(wreq.downloadHandler.text);
        }
    }

    // アセット名から梱包している assetName を最初にロードしている assetBundleMap から探す。
    private string GetAssetBundleName(string assetName)
    {
        var entry = assetBundleMap.data.FirstOrDefault(e => e.assetName == assetName);
        return entry != null ? entry.assetBundleName : null;
    }

    [System.Serializable]
    public class Map
    {
        public List<Entry> data = new List<Entry>();

        internal object FirstOrDefault(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }

    [System.Serializable]
    public class Entry
    {
        public string assetName;
        public string assetBundleName;
    }
}