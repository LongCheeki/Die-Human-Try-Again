using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Player")]
    public PlayerHealth playerHealth;

    [Header("Health Bar")]
    public Image healthFill;
    public TMP_Text healthText;

    [Header("Damage Feedback")]
    public Image damageFlash;

    [Header("Settings")]
    public float flashDuration = 0.15f;

    private Coroutine flashCoroutine;

    void Start()
    {
        UpdateHealthUI();

        if (damageFlash != null)
        {
            Color color = damageFlash.color;
            color.a = 0f;
            damageFlash.color = color;
        }
    }

    public void UpdateHealthUI()
    {
        if (playerHealth == null)
            return;

        float current = playerHealth.GetCurrentHealth();
        float max = playerHealth.GetMaxHealth();

        if (healthFill != null)
        {
            healthFill.fillAmount = current / max;
        }

        if (healthText != null)
        {
            healthText.text =
                "HP  " +
                current.ToString("0.#") +
                " / " +
                max.ToString("0.#");
        }
    }

    public void PlayDamageFeedback()
    {
        UpdateHealthUI();

        if (damageFlash != null)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine =
                StartCoroutine(DamageFlashCoroutine());
        }
    }

    private IEnumerator DamageFlashCoroutine()
    {
        Color color = damageFlash.color;

        color.a = 0.35f;
        damageFlash.color = color;

        float timer = 0f;

        while (timer < flashDuration)
        {
            timer += Time.unscaledDeltaTime;

            color.a =
                Mathf.Lerp(
                    0.35f,
                    0f,
                    timer / flashDuration
                );

            damageFlash.color = color;

            yield return null;
        }

        color.a = 0f;
        damageFlash.color = color;

        flashCoroutine = null;
    }
}