
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("Referências")]
    public EnemySpawner enemySpawner;
    public GameManager gameManager;
    public TMP_Text textoWave;

    [Header("Configurações das Waves")]
    [Min(1)]
    public int quantidadeMaximaWaves = 5;

    public float tempoEntreWaves = 3f;
    public float tempoTextoNaTela = 2f;

    [Header("Vitória")]
    public string nomeCenaVitoria = "Vitoria";

    private int waveAtual = 0;
    private bool iniciandoWave = false;
    private bool jogoFinalizado = false;

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

        yield return StartCoroutine(ControlarWaves());
    }

    IEnumerator ControlarWaves()
    {
        while (waveAtual < quantidadeMaximaWaves)
        {
            iniciandoWave = true;
            waveAtual++;

            Debug.Log($"Iniciando wave {waveAtual} de {quantidadeMaximaWaves}");

            MostrarTextoWave("WAVE " + waveAtual);

            yield return new WaitForSeconds(tempoTextoNaTela);

            // Gera os inimigos da wave atual.
            yield return StartCoroutine(
                enemySpawner.SpawnarWave(waveAtual)
            );

            iniciandoWave = false;

            // Espera todos os inimigos morrerem.
            while (!enemySpawner.WaveTerminou())
            {
                yield return new WaitForSeconds(0.5f);
            }

            MostrarTextoWave("WAVE " + waveAtual + " CONCLUÍDA!");

            // Se esta foi a última wave, termina o jogo.
            if (waveAtual >= quantidadeMaximaWaves)
            {
                jogoFinalizado = true;

                Debug.Log("Todas as waves foram concluídas! Vitória!");

                yield return new WaitForSeconds(tempoTextoNaTela);

                SceneManager.LoadScene(nomeCenaVitoria);
                yield break;
            }

            // Aguarda antes de iniciar a próxima wave.
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
