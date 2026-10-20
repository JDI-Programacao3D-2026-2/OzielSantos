
using UnityEngine;
using UnityEngine.AI;

public class InimigoCombateCorpoaCorpo : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform jogador;
    [SerializeField] private NavMeshAgent agente;
    [SerializeField] private EspadachimAtaque ataque;

    [Header("Distâncias")]
    [SerializeField] private float distanciaAtaque = 2f;
    [SerializeField] private float distanciaParada = 1.2f;

    void Awake()
    {
        if (agente == null)
            agente = GetComponent<NavMeshAgent>();

        if (ataque == null)
            ataque = GetComponent<EspadachimAtaque>();
    }

    void Update()
    {
        if (jogador == null || agente == null)
            return;

        float distancia = Vector3.Distance(
            transform.position,
            jogador.position
        );

        // Aproxima-se do jogador até ficar perto o suficiente.
        agente.stoppingDistance = distanciaParada;

        if (distancia > distanciaAtaque)
        {
            agente.isStopped = false;
            agente.SetDestination(jogador.position);
        }
        else
        {
            agente.isStopped = true;

            // Vira o inimigo para o jogador.
            Vector3 direcao = jogador.position - transform.position;
            direcao.y = 0f;

            if (direcao.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direcao),
                    8f * Time.deltaTime
                );
            }

            // Inicia o ataque; o próprio script controla o cooldown.
            if (ataque != null)
                ataque.TentarAtacar();
        }
    }
}
