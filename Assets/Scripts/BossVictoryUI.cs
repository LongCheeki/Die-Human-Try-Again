using UnityEngine;
using UnityEngine.SceneManagement;

public class BossVictoryUI : MonoBehaviour
{
    [Header("Boss")]
    public EnemyHealth bossHealth;

    [Header("UI")]
    public GameObject victoryPanel;

    [Header("Optional")]
    public AudioSource bossMusic;
    public AudioSource victoryMusic;

    private void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (bossHealth != null)
        {
            bossHealth.Died += ShowVictory;
        }
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.Died -= ShowVictory;
        }
    }

    private void ShowVictory()
    {
        if (bossMusic != null)
        {
            bossMusic.Stop();
        }

        if (victoryMusic != null)
        {
            victoryMusic.Play();
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

        Debug.Log("Quit Game");
    }
}