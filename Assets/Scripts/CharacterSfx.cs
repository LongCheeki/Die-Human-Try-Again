using UnityEngine;

[DisallowMultipleComponent]
public sealed class CharacterSfx : MonoBehaviour
{
    public AudioClip movementClip;
    [Tooltip("Movement sound for the player's slime form.")]
    public AudioClip slimeMovementClip;
    public AudioClip dashClip;
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip healClip;
    public AudioClip enemyAttackClip;
    [Range(0f, 1f)] public float volume = 0.65f;
    [Range(0f, 1f)] public float movementVolume = 0.35f;
    [Min(0.1f)] public float stepDistance = 1.5f;

    private AudioSource source;
    private Vector3 previousPosition;
    private float travelled;
    private PlayerMimic mimic;
    private bool isPlayer;
    private float nextSlimeStepTime;
    private static readonly AudioClip[] defaults = new AudioClip[5];

    public static CharacterSfx Get(GameObject actor)
    {
        var audio = actor.GetComponent<CharacterSfx>();
        return audio != null ? audio : actor.AddComponent<CharacterSfx>();
    }

    private void Awake()
    {
        LoadDefaultClips();
        mimic = GetComponent<PlayerMimic>();
        isPlayer = GetComponent<PlayerMovement>() != null;
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.dopplerLevel = 0f;
        source.spatialBlend = GetComponent<EnemyAI>() != null ? 1f : 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 3f;
        source.maxDistance = 22f;
        previousPosition = transform.position;
    }

    private void Reset()
    {
        LoadDefaultClips();
    }

    private void LoadDefaultClips()
    {
        if (movementClip == null) movementClip = Resources.Load<AudioClip>("CharacterAudio/Movement");
        if (slimeMovementClip == null) slimeMovementClip = Resources.Load<AudioClip>("CharacterAudio/SlimeMovement");
        if (dashClip == null) dashClip = Resources.Load<AudioClip>("CharacterAudio/Dash");
        if (attackClip == null) attackClip = Resources.Load<AudioClip>("CharacterAudio/AttackSwish");
        if (hurtClip == null) hurtClip = Resources.Load<AudioClip>("CharacterAudio/Hurt");
        if (healClip == null) healClip = Resources.Load<AudioClip>("CharacterAudio/HealPickup");
        if (enemyAttackClip == null) enemyAttackClip = Resources.Load<AudioClip>("CharacterAudio/EnemyAttack");
    }

    private void Update()
    {
        if (Time.timeScale == 0f) source.Pause();
        else source.UnPause();
    }

    public void Movement(bool walking)
    {
        Vector3 delta = transform.position - previousPosition;
        previousPosition = transform.position;
        delta.y = 0f;
        if (!walking || Time.timeScale == 0f || delta.magnitude > 2f)
        {
            travelled = 0f;
            return;
        }

        travelled += delta.magnitude;
        if (travelled >= Mathf.Max(0.1f, stepDistance))
        {
            travelled = 0f;
            bool slime = isPlayer && (mimic == null || !mimic.IsWarrior());
            if (slime && slimeMovementClip != null)
            {
                if (Time.time >= nextSlimeStepTime)
                {
                    Play(slimeMovementClip, 0, movementVolume);
                    nextSlimeStepTime = Time.time + slimeMovementClip.length;
                }
            }
            else Play(movementClip, 0, movementVolume);
        }
    }

    public void ResetMovement()
    {
        travelled = 0f;
        previousPosition = transform.position;
    }

    public void Dash() { Play(dashClip, 1, 0.8f); }
    public void Attack() { Play(attackClip, 2, 0.8f); }
    public void Hurt() { Play(hurtClip, 3, 1f); }
    public void HealPickup()
    {
        if (healClip != null) Play(healClip, 0, 0.9f);
    }
    public void EnemyAttack() { Play(enemyAttackClip, 4, 0.85f); }

    private void Play(AudioClip clip, int kind, float gain)
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f) return;
        if (clip == null)
        {
            if (defaults[kind] == null) defaults[kind] = Synthesize(kind);
            clip = defaults[kind];
        }
        source.PlayOneShot(clip, volume * gain);
    }

    private static AudioClip Synthesize(int kind)
    {
        const int rate = 22050;
        float[] durations = { 0.11f, 0.22f, 0.19f, 0.25f, 0.32f };
        string[] names = { "Soft footstep", "Dash rush", "Blade swish", "Damage impact", "Enemy heavy swing" };
        var samples = new float[Mathf.CeilToInt(rate * durations[kind])];
        var random = new System.Random(1709 + kind);
        float filteredNoise = 0f;
        float phase = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / rate;
            float u = (float)i / (samples.Length - 1);
            float noise = (float)random.NextDouble() * 2f - 1f;
            filteredNoise = Mathf.Lerp(filteredNoise, noise, kind == 2 ? 0.65f : 0.23f);
            float frequency = kind == 0 ? 125f : kind == 3 ? 190f : kind == 4 ? 130f : 420f;
            phase += 2f * Mathf.PI * frequency * (1f - 0.65f * u) / rate;
            float envelope = Mathf.Min(1f, t / 0.006f) * Mathf.Pow(1f - u, 2f);
            float tone = Mathf.Sin(phase);
            float value;
            if (kind == 0) value = tone * 0.55f + filteredNoise * 0.45f;
            else if (kind == 3) value = tone * 0.6f + filteredNoise * 0.4f;
            else value = filteredNoise * 0.85f + tone * (kind == 4 ? 0.3f : 0.08f);
            samples[i] = Mathf.Clamp(value * envelope * 0.8f, -1f, 1f);
        }
        var clip = AudioClip.Create(names[kind], samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
