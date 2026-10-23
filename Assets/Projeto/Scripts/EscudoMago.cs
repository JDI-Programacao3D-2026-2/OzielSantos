
using UnityEngine;

public class EscudoFogo : MonoBehaviour
{
    [Header("Configuração")]
    public GameObject bolaPrefab;
    public int quantidadeBolas = 6;
    public int dano = 25;
    public float raio = 2f;
    public float velocidadeRotacao = 100f;
    public float duracao = 6f;

    private Transform jogador;
    private GameObject[] bolas;
    private float tempoDecorrido;
    private bool inicializado;
    private bool encerrado;

    public void Inicializar(
        Transform alvo,
        GameObject prefab,
        int quantidade,
        int danoAtaque,
        float distancia,
        float velocidade,
        float tempoVida)
    {
        if (alvo == null || prefab == null)
        {
            Debug.LogError(
                "EscudoFogo: jogador ou prefab das bolas não configurado.",
                this
            );

            // Este componente deve estar em um objeto separado do jogador.
            EncerrarEscudo();
            return;
        }

        jogador = alvo;
        bolaPrefab = prefab;
        quantidadeBolas = Mathf.Max(1, quantidade);
        dano = danoAtaque;
        raio = distancia;
        velocidadeRotacao = velocidade;
        duracao = tempoVida;

        bolas = new GameObject[quantidadeBolas];

        for (int i = 0; i < quantidadeBolas; i++)
        {
            float angulo = i * Mathf.PI * 2f / quantidadeBolas;

            Vector3 posicao = jogador.position + new Vector3(
                Mathf.Cos(angulo) * raio,
                1f,
                Mathf.Sin(angulo) * raio
            );

            GameObject bola = Instantiate(
                bolaPrefab,
                posicao,
                Quaternion.identity
            );

            BolaEscudoFogo componente =
                bola.GetComponent<BolaEscudoFogo>();

            if (componente != null)
                componente.Configurar(dano, jogador);

            bolas[i] = bola;
        }

        inicializado = true;
    }

    void Update()
    {
        if (!inicializado || encerrado)
            return;

        if (jogador == null)
        {
            EncerrarEscudo();
            return;
        }

        tempoDecorrido += Time.deltaTime;

        if (tempoDecorrido >= duracao)
        {
            EncerrarEscudo();
            return;
        }

        for (int i = 0; i < bolas.Length; i++)
        {
            if (bolas[i] == null)
                continue;

            float angulo =
                i * Mathf.PI * 2f / quantidadeBolas +
                tempoDecorrido * velocidadeRotacao * Mathf.Deg2Rad;

            Vector3 posicao = jogador.position + new Vector3(
                Mathf.Cos(angulo) * raio,
                1f,
                Mathf.Sin(angulo) * raio
            );

            bolas[i].transform.position = posicao;
        }
    }

    void EncerrarEscudo()
    {
        if (encerrado)
            return;

        encerrado = true;

        if (bolas != null)
        {
            foreach (GameObject bola in bolas)
            {
                if (bola != null)
                    Destroy(bola);
            }
        }

        // Destrói somente o objeto que contém este componente.
        Destroy(gameObject);
    }
}
