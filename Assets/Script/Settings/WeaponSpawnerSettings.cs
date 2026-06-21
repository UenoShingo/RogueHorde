using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 武器生成装置
[System.Serializable]
public class WeaponSpawnerStats : BaseStats
{
    // 生成装置のプレハブ
    public GameObject PrefabSpawner;
    // 武器のアイコン
    public Sprite Icon;
    // レベルアップ時に追加されるアイテムID
    public int LevelUpItemId;

    // 一度に生成する数
    public float SpawnCount;
    // 生成タイマー
    public float SpawnTimerMin;
    public float SpawnTimerMax;

    // 生成時間取得
    public float GetRandomSpawnTimer()
    {
        return Random.Range(SpawnTimerMin, SpawnTimerMax);
    }

    // TODO アイテムを追加

}