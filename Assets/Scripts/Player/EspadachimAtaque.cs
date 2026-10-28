using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class EspadachimAtaque : MonoBehaviour
{
    [Header("Configuração do ataque")]
    private GameManager gameManager;
    [SerializeField] private int dano = 25;
    [SerializeField] private float alcance = 2f;
    [SerializeField] private float anguloAtaque = 120f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 0.6f;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 8f;

    [Header("Animação da espada (opcional)")]
    [SerializeField] private Transform espada;
    [SerializeField] private float duracaoGiro = 0.25f;

    [Header("Equipe")]
    [SerializeField] private bool jogador = true;
    [SerializeField] private string tagAlvo = "Enemy";

    private bool podeAtacar = true;
    private bool atacando;
    private Quaternion rotacaoInicial;

    private readonly HashSet<GameObject> alvosAtingidos = new();

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        
        if (espada != null)
            rotacaoInicial = espada.localRotation;
    }

    void Update()
{
    // Apenas o jogador ataca com o clique esquerdo.
    if (jogador &&
        Mouse.current != null &&
        Mouse.current.leftButton.wasPressedThisFrame &&
        podeAtacar && Time.timeScale != 0f)
    {
        StartCoroutine(Atacar());
        gameManager.TutorialAtacar();
    }
}

    // Pode ser chamado pela IA dos inimigos.
    public void TentarAtacar()
    {
        if (!jogador && podeAtacar)
            StartCoroutine(Atacar());
    }

    
IEnumerator Atacar()
{
    if (!podeAtacar)
        yield break;

    podeAtacar = false;
    atacando = true;
    alvosAtingidos.Clear();

    if (espada != null)
        rotacaoInicial = espada.localRotation;

    float tempo = 0f;
    float duracao = Mathf.Max(0.01f, duracaoGiro);

    // A espada começa levantada.
    Quaternion rotacaoInicio = rotacaoInicial *
        Quaternion.Euler(-70f, 0f, 0f);

    // A espada termina apontada para baixo.
    Quaternion rotacaoFim = rotacaoInicial *
        Quaternion.Euler(70f, 0f, 0f);

    while (tempo < duracao)
    {
        tempo += Time.deltaTime;

        float progresso = Mathf.Clamp01(tempo / duracao);

        if (espada != null)
        {
            espada.localRotation = Quaternion.Slerp(
                rotacaoInicio,
                rotacaoFim,
                progresso
            );
        }

        DetectarAlvos();

        yield return null;
    }

    if (espada != null)
        espada.localRotation = rotacaoInicial;

    atacando = false;

    yield return new WaitForSeconds(cooldown);

    podeAtacar = true;
}


    
void DetectarAlvos()
{
    Vector3 centro = transform.position +
                     transform.forward * (alcance * 0.5f);

    Collider[] atingidos = Physics.OverlapSphere(centro, alcance);

    foreach (Collider col in atingidos)
    {
        Transform raiz = col.transform.root;

        // Não pode acertar a si mesmo.
        if (raiz == transform.root)
            continue;

        // O alvo precisa ter a tag configurada.
        if (!raiz.CompareTag(tagAlvo))
            continue;

        // Não acerta o mesmo alvo várias vezes no mesmo golpe.
        if (alvosAtingidos.Contains(raiz.gameObject))
            continue;

        Vector3 direcao = raiz.position - transform.position;
        direcao.y = 0f;

        if (direcao.sqrMagnitude < 0.001f)
            continue;

        if (Vector3.Angle(transform.forward, direcao) >
            anguloAtaque * 0.5f)
            continue;

        Knockback alvo = raiz.GetComponent<Knockback>();

        if (alvo == null)
            continue;

        alvosAtingidos.Add(raiz.gameObject);

        // Só registra como ataque do jogador se este
        // personagem estiver configurado como jogador.
        alvo.ReceberAtaque(
            dano,
            direcao.normalized,
            forcaKnockback,
            jogador
        );

        Debug.Log(
            name + " acertou " + raiz.name +
            " | Ataque do jogador: " + jogador
        );
    }
}


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 centro = transform.position +
                         transform.forward * (alcance * 0.5f);

        Gizmos.DrawWireSphere(centro, alcance);
    }
}
