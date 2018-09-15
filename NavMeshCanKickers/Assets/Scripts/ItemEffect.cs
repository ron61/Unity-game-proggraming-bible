using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムの効果を実装するクラス。
/// </summary>
[Serializable]
public class ItemEffect
{
    public bool isActive { get { return remainingTime >= 0f; } }

    [SerializeField, Header("効果種類")]
    private EffectType type;
    [SerializeField, Header("効果対象が敵")]
    private bool effectOnEnemies = false;
    [SerializeField, Header("効果値。倍率とか")]
    private float value;
    [SerializeField, Header("効果時間(秒)")]
    private float time;
    [SerializeField, Header("文字列表示。type={0}/value={1}/time={2}")]
    private string format = "{0}/{1}/{2}";

    private float remainingTime;

    public enum EffectType
    {
        None = 0,
        Score = 1,
        SpeedUp = 2,
        Stun = 3,
    }

    /// <summary>
    /// アイテム効果開始
    /// </summary>
    /// <param name="getter">効果発生元キャラクタ</param>
    /// <param name="searcher">プレーヤーキャラ検索用</param>
    public void StartEffect(PlayerController getter, IStageSearcher searcher)
    {
        if (effectOnEnemies) {
            foreach (var pc in searcher.GetEnemies(getter)) {
                var e = Clone();
                e.EffectOnPlayer(pc);
            }
        } else {
            EffectOnPlayer(getter);
        }
    }

    private void EffectOnPlayer(PlayerController pc)
    {
        remainingTime = time;
        pc.AddEffect(this);
        switch (type) {
            case EffectType.Score:
                pc.score += (int) value;
                break;
            case EffectType.Stun:
                pc.Pause(true);
                break;
        }
    }

    public void Update(PlayerController pc, float dt)
    {
        if (!isActive) {
            return;
        }
        remainingTime -= dt;
        switch (type) {
            case EffectType.SpeedUp:
                pc.runner.SetSpeedScale(isActive ? value : 1f);
                break;
            case EffectType.Stun:
                if (!isActive) {
                    pc.Pause(false);
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 複製。
    /// 複数のキャラに同時に効果発動させるときに1つの効果を複製して適用する。
    /// </summary>
    private ItemEffect Clone()
    {
        return (ItemEffect) MemberwiseClone();
    }

    public string PopString()
    {
        return string.Format(format, type, value, time);
    }
}