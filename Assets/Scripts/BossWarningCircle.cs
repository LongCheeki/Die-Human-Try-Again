using UnityEngine;

public sealed class BossWarningCircle : MonoBehaviour
{
    public float radius = 1.7f;
    public float warningSeconds = 1.5f;
    public float lingerSeconds = 2f;
    public Renderer disc;
    public Color startColor = new Color(1f, 0.45f, 0.45f, 0.45f);
    public Color endColor = new Color(0.65f, 0f, 0f, 0.9f);
    public bool HasStruck { get; private set; }
    private PlayerHealth target;
    private BossEncounter encounter;
    private float elapsed;
    private MaterialPropertyBlock properties;

    public void Initialize(PlayerHealth player, BossEncounter owner)
    {
        target = player;
        encounter = owner;
        elapsed = 0;
        HasStruck = false;
        properties = new MaterialPropertyBlock();
        SetColor(startColor);
    }

    private void Update()
    {
        if (encounter == null || !encounter.IsFighting || target == null || target.GetCurrentHealth() <= 0)
        {
            Destroy(gameObject);
            return;
        }
        elapsed += Time.deltaTime;
        SetColor(Color.Lerp(startColor, endColor, Mathf.Clamp01(elapsed / warningSeconds)));
        if (!HasStruck && elapsed >= warningSeconds)
        {
            HasStruck = true;
            var delta = target.transform.position - transform.position;
            if (delta.x * delta.x + delta.z * delta.z <= radius * radius && Mathf.Abs(delta.y) < 2f)
                target.TakeTrapDamage(1f);
        }
        if (elapsed >= warningSeconds + lingerSeconds) Destroy(gameObject);
    }

    private void SetColor(Color color)
    {
        if (disc == null) return;
        if (properties == null) properties = new MaterialPropertyBlock();
        properties.SetColor("_BaseColor", color);
        properties.SetColor("_Color", color);
        disc.SetPropertyBlock(properties);
    }
}
