
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs dos inimigos")]
    public GameObject[] inimigos;

    [Header("Referências")]
    private Transform jogador;

    [Header("Quantidade")]
    public int quantidadeInicial = 5;
    public int aumentoPorWave = 2;

    [Header("Posições")]
    [Tooltip("Distância mínima do jogador")]
    public float distanciaMinimaJogador = 15f;

    [Tooltip("Distância máxima do jogador para spawn")]
    public float distanciaMaximaJogador = 45f;

    [Tooltip("Distância mínima entre inimigos")]
    public float distanciaMinimaInimigos = 5f;

    [Tooltip("Quantidade de tentativas para encontrar cada posição")]
    public int tentativasPorInimigo = 50;

    [Tooltip("Distância máxima para encontrar o NavMesh próximo da posição")]
    public float distanciaBuscaNavMesh = 5f;

    [Header("Dificuldade")]
    public int hpInicial = 100;
    public int aumentoHPPorWave = 20;

    private readonly List<GameObject> inimigosDaWave = new();

    void Start()
    {
        EncontrarJogador();
    }

    void EncontrarJogador()
    {
        GameObject objetoJogador =
            GameObject.FindGameObjectWithTag("Player");

        if (objetoJogador != null)
        {
            jogador = objetoJogador.transform;
        }
        else
        {
            Debug.LogWarning(
                "EnemySpawner: jogador não encontrado na cena!"
            );
        }
    }

    public IEnumerator SpawnarWave(int wave)
    {
        if (jogador == null)
            EncontrarJogador();

        if (jogador == null)
        {
            Debug.LogError(
                "EnemySpawner: não foi possível encontrar o jogador."
            );
            yield break;
        }

        if (inimigos == null || inimigos.Length == 0)
        {
            Debug.LogError("Configure os prefabs dos inimigos!");
            yield break;
        }

        inimigosDaWave.Clear();

        int quantidade = quantidadeInicial
            + (wave - 1) * aumentoPorWave;

        int hp = hpInicial
            + (wave - 1) * aumentoHPPorWave;

        for (int i = 0; i < quantidade; i++)
        {
            if (!TentarEncontrarPosicao(out Vector3 posicao))
            {
                Debug.LogWarning(
                    "Não foi possível encontrar uma posição livre " +
                    "para o inimigo " + (i + 1) + "."
                );

                continue;
            }

            GameObject prefab = inimigos[
                Random.Range(0, inimigos.Length)
            ];

            GameObject inimigo = Instantiate(
                prefab,
                posicao,
                Quaternion.identity
            );

            inimigosDaWave.Add(inimigo);

            Knockback vida = inimigo.GetComponent<Knockback>();

            if (vida != null)
            {
                vida.DefinirVidaMaxima(hp);
            }

            yield return null;
        }
    }

    bool TentarEncontrarPosicao(out Vector3 posicao)
    {
        posicao = Vector3.zero;

        for (int tentativa = 0;
             tentativa < tentativasPorInimigo;
             tentativa++)
        {
            // Escolhe uma direção aleatória em 360 graus.
            Vector2 direcaoAleatoria = Random.insideUnitCircle.normalized;

            // Escolhe uma distância aleatória do jogador.
            float distancia = Random.Range(
                distanciaMinimaJogador,
                distanciaMaximaJogador
            );

            Vector3 pontoAleatorio = jogador.position + new Vector3(
                direcaoAleatoria.x * distancia,
                0f,
                direcaoAleatoria.y * distancia
            );

            // Procura uma posição válida no NavMesh.
            if (!NavMesh.SamplePosition(
                pontoAleatorio,
                out NavMeshHit hit,
                distanciaBuscaNavMesh,
                NavMesh.AllAreas))
            {
                continue;
            }

            // Garante que o inimigo não apareça perto demais do jogador.
            float distanciaJogador = Vector3.Distance(
                hit.position,
                jogador.position
            );

            if (distanciaJogador < distanciaMinimaJogador)
                continue;

            // Evita que os inimigos apareçam amontoados.
            bool muitoPerto = false;

            foreach (GameObject existente in inimigosDaWave)
            {
                if (existente == null)
                    continue;

                float distanciaInimigo = Vector3.Distance(
                    hit.position,
                    existente.transform.position
                );

                if (distanciaInimigo < distanciaMinimaInimigos)
                {
                    muitoPerto = true;
                    break;
                }
            }

            if (muitoPerto)
                continue;

            posicao = hit.position;
            return true;
        }

        return false;
    }

    public bool WaveTerminou()
    {
        inimigosDaWave.RemoveAll(inimigo => inimigo == null);
        return inimigosDaWave.Count == 0;
    }

    
public IEnumerator SpawnarTutorial(int quantidade)
{
    inimigosDaWave.Clear();

    if (inimigos == null || inimigos.Length == 0)
    {
        Debug.LogError("Configure os prefabs dos inimigos!");
        yield break;
    }

    for (int i = 0; i < quantidade; i++)
    {
        if (!TentarEncontrarPosicao(out Vector3 posicao))
        {
            Debug.LogWarning(
                "Não foi possível encontrar uma posição para o inimigo do tutorial."
            );
            continue;
        }

        GameObject prefab = inimigos[
            Random.Range(0, inimigos.Length)
        ];

        GameObject inimigo = Instantiate(
            prefab,
            posicao,
            Quaternion.identity
        );

        inimigosDaWave.Add(inimigo);

        Knockback vida = inimigo.GetComponent<Knockback>();

        if (vida != null)
            vida.DefinirVidaMaxima(hpInicial);

        yield return null;
    }
}

}
