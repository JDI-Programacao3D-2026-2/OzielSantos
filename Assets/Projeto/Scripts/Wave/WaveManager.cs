
using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("Referências")]
    public EnemySpawner enemySpawner;
    public GameManager gameManager;
    public TMP_Text textoWave;

    [Header("Configurações")]
    public float tempoEntreWaves = 3f;
    public float tempoTextoNaTela = 2f;

    private int waveAtual = 0;
    private bool iniciandoWave = false;

    
    IEnumerator Start()
{
    // Espera o GameManager existir.
    while (gameManager == null)
    {
        gameManager = FindAnyObjectByType<GameManager>();
        yield return null;
    }

    // Espera o EnemySpawner existir.
    while (enemySpawner == null)
    {
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        yield return null;
    }

    // Aguarda o countdown terminar.
    while (gameManager.countdown)
    {
        yield return null;
    }

    Debug.Log("Countdown terminou! Iniciando as waves.");

    StartCoroutine(ControlarWaves());
}


    IEnumerator ControlarWaves()
    {
        while (true)
        {
            iniciandoWave = true;
            waveAtual++;

            MostrarTextoWave("WAVE " + waveAtual);

            yield return new WaitForSeconds(tempoTextoNaTela);

            // Gera os inimigos da onda atual.
            yield return StartCoroutine(enemySpawner.SpawnarWave(waveAtual));

            iniciandoWave = false;

            // Espera todos os inimigos morrerem.
            while (!enemySpawner.WaveTerminou())
            {
                yield return new WaitForSeconds(0.5f);
            }

            MostrarTextoWave("WAVE " + waveAtual + " CONCLUÍDA!");

            yield return new WaitForSeconds(tempoEntreWaves);
        }
    }

    void MostrarTextoWave(string mensagem)
    {
        if (textoWave == null)
            return;

        textoWave.text = mensagem;
        textoWave.gameObject.SetActive(true);

        CancelInvoke(nameof(EsconderTextoWave));
        Invoke(nameof(EsconderTextoWave), tempoTextoNaTela);
    }

    void EsconderTextoWave()
    {
        if (textoWave != null)
            textoWave.gameObject.SetActive(false);
    }
}
