using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    // シングルトン
    public static SoundController Instance;
    // SEの最後の再生時間
    Dictionary<int, float> lastSeTimes = new Dictionary<int, float>();
    float lastUISelectSETime = -999f;
    float lastEnemyDamageSETime = -999f;
    float lastPlayerDamageSETime = -999f;
    Coroutine bgmCoroutine;

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
    [SerializeField] AudioClip audioClipGameOverBGM;
    [SerializeField] AudioClip audioClipGameClearBGM;
    [SerializeField] int gameOverBGMIndex = 2;
    [SerializeField] int gameClearBGMIndex = 3;
    [SerializeField] int titleBGMIndex = 1;
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
        PlayBGM(getBGMClip(index));
    }

    // BGM再生
    public void PlayBGM(AudioClip clip)
    {
        if (null == audioSource) return;
        if (null == clip) return;

        stopBGMCoroutine();
        audioSource.loop = true;
        audioSource.clip = clip;
        audioSource.Play();
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
        if (null == audioSource) return;

        AudioClip resultClip = clip;
        if (null == resultClip)
        {
            resultClip = getBGMClip(fallbackIndex);
        }

        if (null == resultClip)
        {
            PlayBGM(titleBGMIndex);
            return;
        }

        stopBGMCoroutine();
        bgmCoroutine = StartCoroutine(playResultBGMThenTitle(resultClip));
    }

    IEnumerator playResultBGMThenTitle(AudioClip resultClip)
    {
        audioSource.loop = false;
        audioSource.clip = resultClip;
        audioSource.Play();

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

    void stopBGMCoroutine()
    {
        if (null == bgmCoroutine) return;

        StopCoroutine(bgmCoroutine);
        bgmCoroutine = null;
    }

    // SE再生
    public void PlaySE(int index)
    {
        if (null == audioSource) return;
        if (null == audioClipsSE) return;
        if (0 > index || audioClipsSE.Count <= index) return;
        if (null == audioClipsSE[index]) return;

        audioSource.PlayOneShot(audioClipsSE[index]);
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
        if (null == audioSource) return;
        if (!force
            && 0 < uiSelectSEInterval
            && Time.unscaledTime - lastUISelectSETime < uiSelectSEInterval)
        {
            return;
        }

        lastUISelectSETime = Time.unscaledTime;

        if (null != audioClipUISelect)
        {
            audioSource.PlayOneShot(audioClipUISelect);
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
        PlayDamageSE(audioClipPlayerDamage, ref lastPlayerDamageSETime, playerDamageSEInterval);
    }

    // ダメージSE再生
    void PlayDamageSE(AudioClip clip, ref float lastTime, float interval)
    {
        if (null == audioSource) return;
        if (0 < interval && Time.unscaledTime - lastTime < interval) return;

        lastTime = Time.unscaledTime;

        if (null != clip)
        {
            audioSource.PlayOneShot(clip);
            return;
        }

        PlaySE(1);
    }
}
