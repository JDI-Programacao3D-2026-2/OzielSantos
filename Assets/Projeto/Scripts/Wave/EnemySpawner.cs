
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
    public float raioSpawn = 30f;
    public float distanciaMinimaJogador = 12f;
    public float distanciaMinimaInimigos = 3f;
    public int tentativasPorInimigo = 30;

    [Header("Dificuldade")]
    public int hpInicial = 100;
    public int aumentoHPPorWave = 20;

    private readonly List<GameObject> inimigosDaWave = new();

    void Start()
    {
        GameObject objetoJogador = GameObject.FindGameObjectWithTag("Player");

        if (objetoJogador != null)
        {
            jogador = objetoJogador.transform;
        }
        else
        {
            Debug.LogWarning("EnemySpawner: jogador não encontrado na cena!");
        }
    }

    public IEnumerator SpawnarWave(int wave)
    {
        inimigosDaWave.Clear();

        int quantidade = quantidadeInicial
            + (wave - 1) * aumentoPorWave;

        int hp = hpInicial
            + (wave - 1) * aumentoHPPorWave;

        for (int i = 0; i < quantidade; i++)
        {
            if (inimigos == null || inimigos.Length == 0)
            {
                Debug.LogError("Configure os prefabs dos inimigos!");
                yield break;
            }

            if (!TentarEncontrarPosicao(out Vector3 posicao))
            {
                Debug.LogWarning(
                    "Não foi possível encontrar uma posição livre para o inimigo."
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
            Vector3 aleatoria = transform.position + new Vector3(
                Random.Range(-raioSpawn, raioSpawn),
                0f,
                Random.Range(-raioSpawn, raioSpawn)
            );

            if (!NavMesh.SamplePosition(
                aleatoria,
                out NavMeshHit hit,
                3f,
                NavMesh.AllAreas))
            {
                continue;
            }

            if (jogador != null &&
                Vector3.Distance(hit.position, jogador.position)
                < distanciaMinimaJogador)
            {
                continue;
            }

            bool muitoPerto = false;

            foreach (GameObject existente in inimigosDaWave)
            {
                if (existente == null)
                    continue;

                if (Vector3.Distance(
                    hit.position,
                    existente.transform.position
                ) < distanciaMinimaInimigos)
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
}
