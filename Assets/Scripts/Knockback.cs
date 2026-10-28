using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Knockback : MonoBehaviour
{
    [Header("Vida")]
    public int vida = 100;
    public int vidaMaxima = 100;

    [Header("Knockback")]
    [SerializeField] private float duracaoKnockback = 0.2f;

    [Header("Configuração")]
    [SerializeField] private bool destruirAoMorrer = true;
    private bool morreu = false;

    [Header("Invencibilidade ao renascer")]
    [SerializeField] private float duracaoInvencibilidade = 3f;
    [SerializeField] private float intervaloPiscada = 0.15f;

    private bool invencivel = false;
    private Renderer[] renderizadores;
    private Coroutine rotinaInvencibilidade;

    private Rigidbody rb;
    private NavMeshAgent agente;
    private Coroutine rotinaKnockback;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agente = GetComponent<NavMeshAgent>();

        renderizadores = GetComponentsInChildren<Renderer>(true);
    }

public void DefinirVidaMaxima(int novaVida)
{
    vidaMaxima = novaVida;
    vida = novaVida;
}


IEnumerator InvencibilidadeRespawn()
{
    invencivel = true;

    float tempo = 0f;
    bool visivel = true;

    while (tempo < duracaoInvencibilidade)
    {
        visivel = !visivel;

        foreach (Renderer rend in renderizadores)
        {
            if (rend != null)
                rend.enabled = visivel;
        }

        yield return new WaitForSeconds(intervaloPiscada);
        tempo += intervaloPiscada;
    }

    // Garante que o jogador volte a aparecer normalmente.
    foreach (Renderer rend in renderizadores)
    {
        if (rend != null)
            rend.enabled = true;
    }

    invencivel = false;
    rotinaInvencibilidade = null;
}


public void ReceberAtaque(
    int dano,
    Vector3 direcao,
    float forca,
    bool causadoPeloPlayer
)
{
    // Impede que projéteis registrem a mesma morte novamente.
    if (morreu)
        return;

    if (CompareTag("Player") && invencivel)
        return;

    vida -= dano;

    Debug.Log($"{name} recebeu {dano} de dano. Vida: {vida}");

    if (vida <= 0)
    {
        vida = 0;
        morreu = true;

        bool ehJogador = CompareTag("Player");

        if (causadoPeloPlayer && !ehJogador)
        {
            GameManager gm = FindAnyObjectByType<GameManager>();

            if (gm != null)
                gm.RegistrarKill();
        }

        if (ehJogador)
        {
            GameManager gm = FindAnyObjectByType<GameManager>();

            if (gm != null)
                gm.RegistrarMorte();

            RespawnPlayer();
        }
        else if (destruirAoMorrer)
        {
            DropsInimigo drops = GetComponent<DropsInimigo>();

            if (drops != null)
                drops.SoltarDrops();

            Destroy(gameObject);
        }

        return;
    }

    if (rotinaKnockback != null)
        StopCoroutine(rotinaKnockback);

    rotinaKnockback = StartCoroutine(
        AplicarKnockback(direcao, forca)
    );
}


public void ReceberAtaque(
    int dano,
    Vector3 direcao,
    float forca
)
{
    ReceberAtaque(dano, direcao, forca, false);
}

    

IEnumerator AplicarKnockback(Vector3 direcao, float forca)
{
    CharacterController controller =
        GetComponent<CharacterController>();

    bool agenteEstavaAtivo =
        agente != null && agente.enabled;

    // O knockback será horizontal, sem lançar o inimigo para cima.
    direcao.y = 0f;

    if (direcao.sqrMagnitude > 0.001f)
        direcao.Normalize();
    else
    {
        rotinaKnockback = null;
        yield break;
    }

    if (agenteEstavaAtivo && agente.isOnNavMesh)
        agente.isStopped = true;

    // Para evitar conflitos, desativamos temporariamente o agente.
    if (agenteEstavaAtivo)
        agente.enabled = false;

    // Interrompe a física que poderia continuar lançando o inimigo.
    if (rb != null && !rb.isKinematic)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    float tempo = 0f;

    while (tempo < duracaoKnockback)
    {
        float passo = forca * Time.deltaTime;
        Vector3 deslocamento = direcao * passo;

        if (controller != null && controller.enabled)
        {
            controller.Move(deslocamento);
        }
        else
        {
            transform.position += deslocamento;
        }

        tempo += Time.deltaTime;
        yield return null;
    }

    // Limpa qualquer velocidade física residual.
    if (rb != null && !rb.isKinematic)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    if (agenteEstavaAtivo && agente != null)
    {
        NavMeshHit hit;

        // Busca uma superfície próxima, sem aceitar grandes
        // diferenças de altura entre a posição atual e o NavMesh.
        if (NavMesh.SamplePosition(
            transform.position,
            out hit,
            2f,
            agente.areaMask) &&
            Mathf.Abs(hit.position.y - transform.position.y) <= 0.8f)
        {
            transform.position = hit.position;
            agente.enabled = true;

            if (agente.isOnNavMesh)
            {
                agente.ResetPath();
                agente.isStopped = false;
            }
            else
            {
                Debug.LogWarning(
                    $"{name}: agente reativado fora do NavMesh."
                );
            }
        }
        else
        {
            // Não teletransporta para outra altura.
            agente.enabled = true;

            Debug.LogWarning(
                $"{name}: não encontrou NavMesh próximo e na altura correta."
            );
        }
    }

    rotinaKnockback = null;
}


void RespawnPlayer()
{
    StopAllCoroutines();
    rotinaKnockback = null;

    // Recupera a posição inicial salva pelo MapaGerador.
    Vector3 posicao = MapaGerador.PosicaoInicialPlayer;
    Quaternion rotacao = MapaGerador.RotacaoInicialPlayer;

    // Interrompe temporariamente o movimento.
    if (agente != null)
    {
        agente.enabled = false;
    }

    CharacterController controller =
        GetComponent<CharacterController>();

    if (controller != null)
        controller.enabled = false;

    transform.SetPositionAndRotation(posicao, rotacao);

    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Recupera a vida.
    vida = vidaMaxima;

    if (controller != null)
        controller.enabled = true;

    if (agente != null)
    {
        agente.enabled = true;

        if (NavMesh.SamplePosition(
            posicao,
            out NavMeshHit hit,
            3f,
            NavMesh.AllAreas))
        {
            agente.Warp(hit.position);
            agente.isStopped = false;
            agente.ResetPath();
        }
    }

    morreu = false;

    if (rotinaInvencibilidade != null)
    StopCoroutine(rotinaInvencibilidade);

    rotinaInvencibilidade = StartCoroutine(
        InvencibilidadeRespawn()
    );
    Debug.Log("Jogador renasceu na posição inicial!");
}

}
