using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    // シングルトン
    public static SoundController Instance;

    void Awake()
    {
        // もし無ければセットする
        if (null == Instance)
        {
            // サウンドの設定
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            // 最初に作られたオブジェクトをセットする
            Instance = this;
            // シーンをまたいでもオブジェクトを削除しない
            DontDestroyOnLoad(this.gameObject);
        }
        // 2回目以降に生成されたオブジェクトは削除する
        else
        {
            Destroy(this.gameObject);
        }
    }

    // 再生装置
    AudioSource audioSource;
    // BGM音源
    [SerializeField] List<AudioClip> audioClipsBGM;
    // SE音源
    [SerializeField] List<AudioClip> audioClipsSE;

    // BGM再生
    public void PlayBGM(int index)
    {
        audioSource.clip = audioClipsBGM[index];
        audioSource.Play();
    }

    // SE再生
    public void PlaySE(int index)
    {
        audioSource.PlayOneShot(audioClipsSE[index]);
    }
}
