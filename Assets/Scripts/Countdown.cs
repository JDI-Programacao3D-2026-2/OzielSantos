using UnityEngine;
using System.Collections;
using TMPro;

public class Countdown : MonoBehaviour
{
    public TextMeshProUGUI countdownLabel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        Time.timeScale = 0f;
    }
    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StartCountdown()
    {
        countdownLabel.text = "3";
        yield return new WaitForSecondsRealtime(1f);
        countdownLabel.text = "2";
        yield return new WaitForSecondsRealtime(1f);
        countdownLabel.text = "1";
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        countdownLabel.text = "VAI!";
        yield return new WaitForSecondsRealtime(1f);
        countdownLabel.gameObject.SetActive(false);
    }
}
