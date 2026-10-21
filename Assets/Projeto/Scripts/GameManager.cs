using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Countdown")]
    public float countdownTime = 5f;
    public TextMeshProUGUI timerLabel;
    private bool countdown = true;

    [Header("Player Stats")]
    public string playerName;
    public int playerKills = 0;
    public int playerDeaths = 0;

    [Header("UI")]
    public RectTransform frags;
    public TextMeshProUGUI playerNameLabel;
    public TextMeshProUGUI playerKillsLabel;
    public TextMeshProUGUI playerDeathsLabel;

    void Awake()
    {
        frags = frags.GetComponent<RectTransform>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AtualizarFrags();
        Time.timeScale = 0f;
        timerLabel.text = countdownTime.ToString();
        StartCoroutine(StartCountdown());
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[Key.Tab].isPressed)
        {
            frags.anchoredPosition = new Vector2(0, 0);
        }
        else
        {
            frags.anchoredPosition = new Vector2(0, 400);
        }
    }

    IEnumerator StartCountdown()
    {
        while (countdown)
        {
            if (countdownTime > 0f)
            {
                yield return new WaitForSecondsRealtime(1f);
                countdownTime -= 1f;
                timerLabel.text = countdownTime.ToString();
            }
            else
            {
                timerLabel.text = "Vai!";
                countdown = false;
                Time.timeScale = 1f;
                yield return new WaitForSecondsRealtime(1f);
                timerLabel.gameObject.SetActive(false);
                StopCoroutine(StartCountdown());
            }
        }
    }

    public void RegistrarKill()
    {
        playerKills++;
        Debug.Log(playerName + " eliminou um inimigo! Kills: " + playerKills);
        AtualizarFrags();
    }

    public void RegistrarMorte()
    {
        playerDeaths++;
        Debug.Log(playerName + " morreu! Mortes: " + playerDeaths);
        AtualizarFrags();
    }

    void AtualizarFrags()
    {
        playerNameLabel.text = playerName;
        playerKillsLabel.text = playerKills.ToString();
        playerDeathsLabel.text = playerDeaths.ToString();
    }
}
