using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public sealed class BossShield : MonoBehaviour
{
    public float rechargeDelay = 9f;
    public GameObject effectPrefab;
    public bool HasShield { get; private set; }
    private EnemyHealth health;
    private GameObject barrier;
    private ParticleSystem sparks;
    private float rechargeRemaining;
    private Rigidbody body;
    private RigidbodyConstraints originalConstraints;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        body = GetComponent<Rigidbody>();
        if (body != null) originalConstraints = body.constraints;
        if (effectPrefab == null) return;
        var effect = Instantiate(effectPrefab, transform, false);
        barrier = effect.transform.Find("Barrier").gameObject;
        sparks = effect.GetComponentInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        HasShield = true;
        rechargeRemaining = 0f;
        LockPosition(true);
        if (barrier != null) barrier.SetActive(true);
        if (sparks != null) sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (HasShield || health.GetCurrentHealth() <= 0f) return;
        rechargeRemaining -= Time.deltaTime;
        if (rechargeRemaining > 0f) return;
        HasShield = true;
        LockPosition(true);
        if (barrier != null) barrier.SetActive(true);
    }

    public bool AbsorbHit(PlayerMimic attacker)
    {
        if (!HasShield) return false;
        if (attacker != null && attacker.IsWarrior())
        {
            HasShield = false;
            LockPosition(false);
            rechargeRemaining = rechargeDelay;
            if (barrier != null) barrier.SetActive(false);
            if (sparks != null) sparks.Play(true);
        }
        return true;
    }

    private void LockPosition(bool locked)
    {
        if (body == null) return;
        body.constraints = locked ? originalConstraints |
            RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ : originalConstraints;
        if (locked) body.linearVelocity = new Vector3(0f, body.linearVelocity.y, 0f);
    }

    private void OnDisable() { LockPosition(false); }
}
