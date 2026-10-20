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

    private Rigidbody rb;
    private NavMeshAgent agente;
    private Coroutine rotinaKnockback;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agente = GetComponent<NavMeshAgent>();
    }

    public void ReceberAtaque(
        int dano,
        Vector3 direcao,
        float forca
    )
    {
        vida -= dano;

        if (vida <= 0)
        {
            vida = 0;

            if (destruirAoMorrer)
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("O jogador morreu! Ative aqui sua lógica de morte.");
            }

            return;
        }

        if (rotinaKnockback != null)
            StopCoroutine(rotinaKnockback);

        rotinaKnockback = StartCoroutine(
            AplicarKnockback(direcao, forca)
        );
    }

    
IEnumerator AplicarKnockback(Vector3 direcao, float forca)
{
    CharacterController controller =
        GetComponent<CharacterController>();

    Rigidbody rb = GetComponent<Rigidbody>();
    NavMeshAgent agente = GetComponent<NavMeshAgent>();

    bool agenteEstavaAtivo = agente != null && agente.enabled;

    if (agenteEstavaAtivo)
    {
        agente.isStopped = true;
        agente.enabled = false;
    }

    // Opção 1: personagem com CharacterController.
    // Move respeita paredes e colisões.
    if (controller != null && controller.enabled)
    {
        float tempo = 0f;

        while (tempo < duracaoKnockback)
        {
            float passo = forca * Time.deltaTime;

            controller.Move(direcao * passo);

            tempo += Time.deltaTime;
            yield return null;
        }
    }
    // Opção 2: personagem com Rigidbody.
    // A física impede que atravesse paredes sólidas.
    else if (rb != null && !rb.isKinematic)
    {
        rb.AddForce(direcao * forca, ForceMode.Impulse);

        yield return new WaitForSeconds(duracaoKnockback);

        rb.linearVelocity = new Vector3(
            0f,
            rb.linearVelocity.y,
            0f
        );
    }
    // Opção 3: sem física, usa uma verificação de colisão
    // antes de cada pequeno deslocamento.
    else
    {
        float tempo = 0f;
        float velocidade = forca;

        Collider colisor = GetComponent<Collider>();

        while (tempo < duracaoKnockback)
        {
            float passo = velocidade * Time.deltaTime;
            Vector3 deslocamento = direcao * passo;

            bool bloqueado = false;

            if (colisor != null)
            {
                Bounds b = colisor.bounds;

                Vector3 pontoInferior =
                    new Vector3(b.center.x, b.min.y + 0.05f, b.center.z);

                Vector3 pontoSuperior =
                    new Vector3(b.center.x, b.max.y - 0.05f, b.center.z);

                float raio = Mathf.Max(
                    0.05f,
                    Mathf.Min(b.extents.x, b.extents.z) * 0.8f
                );

                bloqueado = Physics.CapsuleCast(
                    pontoInferior,
                    pontoSuperior,
                    raio,
                    direcao,
                    passo + 0.02f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore
                );
            }

            if (!bloqueado)
                transform.position += deslocamento;
            else
                break;

            tempo += Time.deltaTime;
            yield return null;
        }
    }

    // Reativa a perseguição do inimigo após o empurrão.
    if (agenteEstavaAtivo && agente != null)
    {
        agente.enabled = true;
        agente.isStopped = false;
        agente.ResetPath();
    }

    rotinaKnockback = null;
}

}
