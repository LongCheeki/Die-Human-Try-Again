using UnityEngine;

[DisallowMultipleComponent]
public sealed class CastleBattleMusic : MonoBehaviour
{
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 0.10f;
    [Min(0f)] public float fadeInSeconds = 1.5f;

    private static CastleBattleMusic activeMusic;
    private AudioSource source;
    private float fade;

    public static void Ensure(GameObject player)
    {
        if (activeMusic == null && player.GetComponent<CastleBattleMusic>() == null)
            player.AddComponent<CastleBattleMusic>();
    }

    private void Awake()
    {
        if (activeMusic != null && activeMusic != this)
        {
            enabled = false;
            return;
        }
        activeMusic = this;
        if (musicClip == null) musicClip = Resources.Load<AudioClip>("Music/CastleBattle");
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = true;
        source.volume = 0f;
        source.clip = musicClip;
        source.priority = 64;
        if (musicClip != null) source.Play();
    }

    private void Update()
    {
        if (source == null) return;
        if (Time.timeScale == 0f)
        {
            source.Pause();
            return;
        }
        source.UnPause();
        fade = fadeInSeconds <= 0f ? 1f : Mathf.Min(1f, fade + Time.unscaledDeltaTime / fadeInSeconds);
        source.volume = volume * fade;
    }

    private void OnDisable()
    {
        if (source != null) source.Pause();
    }

    private void OnDestroy()
    {
        if (activeMusic == this) activeMusic = null;
        if (source != null) Destroy(source);
    }
}
