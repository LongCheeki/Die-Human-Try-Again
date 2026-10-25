using UnityEngine;

public sealed class RangerAnimationEvents : MonoBehaviour
{
    public SkinnedMeshRenderer bow;
    public GameObject heldArrow;
    private PlayerArcherCombat combat;
    private Animator animator;

    private void Awake()
    {
        combat = GetComponentInParent<PlayerArcherCombat>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (combat == null) combat = GetComponentInParent<PlayerArcherCombat>();
        if (animator == null) animator = GetComponent<Animator>();
        animator.Rebind();
        animator.Update(0f);
        if (heldArrow != null) heldArrow.SetActive(false);
        if (bow != null) bow.SetBlendShapeWeight(0, 0f);
    }

    public void ReleaseArrow()
    {
        if (combat != null) combat.ReleaseArrow();
        if (heldArrow != null) heldArrow.SetActive(false);
    }

    public void FinishShot()
    {
        if (combat != null) combat.FinishShot();
    }

    private void LateUpdate()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);
        float draw = 0f;
        if (state.IsName("Draw")) draw = Mathf.InverseLerp(0.3f, 0.65f, state.normalizedTime);
        else if (state.IsName("Release")) draw = 1f - Mathf.InverseLerp(0f, 0.06f, state.normalizedTime);
        if (bow != null) bow.SetBlendShapeWeight(0, draw * 100f);
        if (heldArrow != null)
            heldArrow.SetActive(combat != null && combat.HasPendingShot &&
                ((state.IsName("Draw") && state.normalizedTime >= 0.15f) || state.IsName("Release")));
    }

    private void OnDisable()
    {
        if (combat != null) combat.CancelShot();
        if (heldArrow != null) heldArrow.SetActive(false);
    }
}
