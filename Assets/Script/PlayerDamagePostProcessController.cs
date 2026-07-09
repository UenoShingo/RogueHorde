using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerDamagePostProcessController : MonoBehaviour
{
    [SerializeField] Shader damageShader;
    [SerializeField] Color damageColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField, Range(0f, 1f)] float damageIntensity = 0.18f;
    [SerializeField] float fadeInTime = 0.03f;
    [SerializeField] float holdTime = 0.04f;
    [SerializeField] float fadeOutTime = 0.25f;

    Material material;
    float timer;
    float totalTime;
    float currentIntensity;

    void Awake()
    {
        setupMaterial();
    }

    void OnDestroy()
    {
        if (null != material)
        {
            Destroy(material);
        }
    }

    void Update()
    {
        if (0 >= timer)
        {
            currentIntensity = 0;
            return;
        }

        timer -= Time.unscaledDeltaTime;
        float elapsed = totalTime - timer;

        if (0 < fadeInTime && elapsed < fadeInTime)
        {
            currentIntensity = Mathf.Lerp(0, damageIntensity, elapsed / fadeInTime);
            return;
        }

        elapsed -= fadeInTime;
        if (elapsed < holdTime)
        {
            currentIntensity = damageIntensity;
            return;
        }

        elapsed -= holdTime;
        if (0 < fadeOutTime && elapsed < fadeOutTime)
        {
            currentIntensity = Mathf.Lerp(damageIntensity, 0, elapsed / fadeOutTime);
            return;
        }

        currentIntensity = 0;
        timer = 0;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (null == material || 0 >= currentIntensity)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetColor("_Color", damageColor);
        material.SetFloat("_Intensity", currentIntensity);
        Graphics.Blit(source, destination, material);
    }

    public void Flash()
    {
        setupMaterial();

        totalTime = Mathf.Max(0, fadeInTime) + Mathf.Max(0, holdTime) + Mathf.Max(0, fadeOutTime);
        timer = totalTime;

        if (0 >= totalTime)
        {
            currentIntensity = damageIntensity;
        }
    }

    void setupMaterial()
    {
        if (null == damageShader)
        {
            damageShader = Resources.Load<Shader>("DamageRedPostProcess");
        }

        if (null == damageShader)
        {
            damageShader = Shader.Find("Hidden/RogueHorde/DamageRedPostProcess");
        }

        if (null != material || null == damageShader || !damageShader.isSupported) return;

        material = new Material(damageShader)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
    }
}
