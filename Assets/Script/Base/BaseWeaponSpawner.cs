using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWeaponSpawner : MonoBehaviour
{
    // 武器のプレハブ
    [SerializeField] GameObject PrefabWeapon;

    // 武器データ
    public WeaponSpawnerStats Stats;
    // 与えた総ダメージ
    public float TotalDamage;
    // 稼働タイマー
    public float TotalTimer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
