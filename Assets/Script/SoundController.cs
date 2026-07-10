using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    // シングルトン
    public static SoundController Instance;
    // BGM再生装置
    AudioSource bgmAudioSource;
    // SE再生装置
    AudioSource seAudioSource;
    // ダメージSE再生装置
    AudioSource damageAudioSource;
    // リザルトBGM再生後にタイトルBGMへ戻す処理
    Coroutine bgmCoroutine;

    // SEの最後の再生時間
    Dictionary<int, float> lastSeTimes = new Dictionary<int, float>();
    float lastUISelectSETime = -999f;
    float lastEnemyDamageSETime = -999f;
    float lastPlayerDamageSETime = -999f;

    void Awake()
    {
        // もし無ければセットする
        if (null == Instance)
        {
            // サウンドの設定
            setupAudioSources();
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

    // BGM音源
    [SerializeField] List<AudioClip> audioClipsBGM;
    [SerializeField] AudioClip audioClipGameOverBGM;
    [SerializeField] AudioClip audioClipGameClearBGM;
    [SerializeField] int gameOverBGMIndex = 2;
    [SerializeField] int gameClearBGMIndex = 3;
    [SerializeField] int titleBGMIndex = 1;
    [SerializeField, Range(0f, 1f)] float bgmVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] float seVolume = 1f;
    // SE音源
    [SerializeField] List<AudioClip> audioClipsSE;
    // UI選択専用SE音源
    [SerializeField] AudioClip audioClipUISelect;
    [SerializeField] float uiSelectSEInterval = 0.05f;
    // ダメージ専用SE音源
    [SerializeField] AudioClip audioClipEnemyDamage;
    [SerializeField] AudioClip audioClipPlayerDamage;
    [SerializeField] float enemyDamageSEInterval = 0.08f;
    [SerializeField] float playerDamageSEInterval = 0.08f;

    // BGM再生
    public void PlayBGM(int index)
    {
        AudioClip clip = getBGMClip(index);
        if (null == clip) return;

        PlayBGM(clip);
    }

    // BGM再生
    public void PlayBGM(AudioClip clip)
    {
        if (null == bgmAudioSource) return;
        if (null == clip) return;

        stopBGMCoroutine();
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }

    // ゲームオーバーBGM再生
    public void PlayGameOverBGM()
    {
        playResultBGM(audioClipGameOverBGM, gameOverBGMIndex);
    }

    // ゲームクリアBGM再生
    public void PlayGameClearBGM()
    {
        playResultBGM(audioClipGameClearBGM, gameClearBGMIndex);
    }

    void playResultBGM(AudioClip clip, int fallbackIndex)
    {
        AudioClip resultClip = clip;
        if (null == resultClip)
        {
            resultClip = getBGMClip(fallbackIndex);
        }

        if (null == bgmAudioSource) return;
        if (null == resultClip) return;

        stopBGMCoroutine();
        bgmCoroutine = StartCoroutine(playResultBGMThenTitle(resultClip));
    }

    // SE再生
    public void PlaySE(int index)
    {
        if (null == seAudioSource) return;
        if (null == audioClipsSE) return;
        if (0 > index || audioClipsSE.Count <= index) return;
        if (null == audioClipsSE[index]) return;

        seAudioSource.volume = seVolume;
        seAudioSource.PlayOneShot(audioClipsSE[index]);
    }

    // SE再生（連続再生制限付き）
    public void PlaySE(int index, float interval)
    {
        if (0 < interval)
        {
            if (lastSeTimes.TryGetValue(index, out float lastTime)
                && Time.unscaledTime - lastTime < interval)
            {
                return;
            }

            lastSeTimes[index] = Time.unscaledTime;
        }

        PlaySE(index);
    }

    // UI選択SE再生
    public void PlayUISelectSE()
    {
        PlayUISelectSE(false);
    }

    public void PlayUISelectSE(bool force)
    {
        if (null == seAudioSource) return;
        if (!force
            && 0 < uiSelectSEInterval
            && Time.unscaledTime - lastUISelectSETime < uiSelectSEInterval)
        {
            return;
        }

        lastUISelectSETime = Time.unscaledTime;

        if (null != audioClipUISelect)
        {
            seAudioSource.volume = seVolume;
            seAudioSource.PlayOneShot(audioClipUISelect);
        }
    }

    // 敵ダメージSE再生
    public void PlayEnemyDamageSE()
    {
        PlayDamageSE(audioClipEnemyDamage, ref lastEnemyDamageSETime, enemyDamageSEInterval);
    }

    // プレイヤーダメージSE再生
    public void PlayPlayerDamageSE()
    {
        PlayDamageSE(audioClipPlayerDamage, ref lastPlayerDamageSETime, playerDamageSEInterval, true, true);
    }

    // ダメージSE再生
    void PlayDamageSE(AudioClip clip, ref float lastTime, float interval, bool preventOverlap = false, bool interruptPlayingDamage = false)
    {
        if (null == damageAudioSource) return;

        if (null == clip)
        {
            clip = getSEClip(1);
        }

        if (null == clip) return;

        float effectiveInterval = interval;
        if (preventOverlap)
        {
            effectiveInterval = Mathf.Max(effectiveInterval, clip.length);
        }

        if (0 < effectiveInterval && Time.unscaledTime - lastTime < effectiveInterval) return;

        if (damageAudioSource.isPlaying)
        {
            if (!interruptPlayingDamage) return;

            damageAudioSource.Stop();
        }

        lastTime = Time.unscaledTime;
        damageAudioSource.volume = seVolume;
        damageAudioSource.clip = clip;
        damageAudioSource.Play();
    }

    void setupAudioSources()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();

        if (0 < audioSources.Length)
        {
            bgmAudioSource = audioSources[0];
        }
        else
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (1 < audioSources.Length)
        {
            seAudioSource = audioSources[1];
        }
        else
        {
            seAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (2 < audioSources.Length)
        {
            damageAudioSource = audioSources[2];
        }
        else
        {
            damageAudioSource = gameObject.AddComponent<AudioSource>();
        }

        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = bgmVolume;

        seAudioSource.playOnAwake = false;
        seAudioSource.loop = false;
        seAudioSource.volume = seVolume;
        seAudioSource.spatialBlend = 0f;
        seAudioSource.outputAudioMixerGroup = bgmAudioSource.outputAudioMixerGroup;

        damageAudioSource.playOnAwake = false;
        damageAudioSource.loop = false;
        damageAudioSource.volume = seVolume;
        damageAudioSource.spatialBlend = 0f;
        damageAudioSource.outputAudioMixerGroup = bgmAudioSource.outputAudioMixerGroup;
    }

    IEnumerator playResultBGMThenTitle(AudioClip resultClip)
    {
        bgmAudioSource.loop = false;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.clip = resultClip;
        bgmAudioSource.Play();

        yield return new WaitForSecondsRealtime(resultClip.length);

        bgmCoroutine = null;
        PlayBGM(titleBGMIndex);
    }

    AudioClip getBGMClip(int index)
    {
        if (null == audioClipsBGM) return null;
        if (0 > index || audioClipsBGM.Count <= index) return null;

        return audioClipsBGM[index];
    }

    AudioClip getSEClip(int index)
    {
        if (null == audioClipsSE) return null;
        if (0 > index || audioClipsSE.Count <= index) return null;

        return audioClipsSE[index];
    }

    void stopBGMCoroutine()
    {
        if (null == bgmCoroutine) return;

        StopCoroutine(bgmCoroutine);
        bgmCoroutine = null;
    }
}
