using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージから缶や箱などを探すinterface
/// </summary>
public interface IStageSearcher
{
    /// <summary>enemy用。一番高スコアな缶を探す。</summary>
    Kickable GetHighScoreCan();

    /// <summary>指定場所から一番近い蹴れるものを探す。</summary>
    Kickable GetKickableNearest(Vector3 position);

    /// <summary>全敵プレーヤー</summary>
    IEnumerable<PlayerController> GetEnemies(PlayerController me);
}
