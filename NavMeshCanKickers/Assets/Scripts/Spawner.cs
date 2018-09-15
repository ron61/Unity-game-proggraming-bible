using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// キャラクタspawn。SetSpawnPosition() でspawn予定位置を設定して、spawn時はその中からランダム選択する。
/// 一度 spawnすると、Delete() するまでその場所にはspawnしないようになっている。
/// </summary>
public class Spawner<T> where T : MonoBehaviour
{
    public SpawnEvent onSpawn = new SpawnEvent();

    public class SpawnEvent : UnityEvent<T> { }

    private List<Transform> spawnPositions = new List<Transform>();
    private Dictionary<T, Transform> usedPos = new Dictionary<T, Transform>();

    public bool hasSpawnPos { get { return spawnPositions.Count > 0; } }
    public IEnumerable<T> allObjects { get { return usedPos.Keys; } }

    public void SetSpawnPositions(IEnumerable<Transform> poslist)
    {
        spawnPositions = poslist.ToList();
    }

    public T Spawn(T prefab)
    {
        var obj = Object.Instantiate(prefab.transform);
        var c = obj.GetComponent<T>();
        SetPosition(c);
        onSpawn.Invoke(c);
        return c;
    }

    public void SpawnAll(T prefab)
    {
        while (hasSpawnPos) {
            Spawn(prefab);
        }
    }

    public void Delete(T obj)
    {
        var t = obj.transform;
        if (usedPos.ContainsKey(obj)) {
            spawnPositions.Add(usedPos[obj]);
            usedPos.Remove(obj);
        }
    }

    private void SetPosition(T obj)
    {
        var i = Random.Range(0, spawnPositions.Count);
        var pos = spawnPositions[i];
        spawnPositions.RemoveAt(i);
        obj.transform.position = pos.position;
        obj.transform.rotation = pos.rotation;
        usedPos[obj] = pos;
    }
}