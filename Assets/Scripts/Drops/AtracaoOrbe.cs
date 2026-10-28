
using UnityEngine;
using System.Collections;

public class AtracaoOrbe : MonoBehaviour
{
    [Header("Atração")]
    [SerializeField] private float tempoEspera = 2f;
    [SerializeField] private float velocidadeAtracao = 8f;
    [SerializeField] private float distanciaColeta = 0.6f;

    private Transform jogador;
    private bool atraindo;
    private bool coletando;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("AtracaoOrbe: jogador não encontrado!");
            return;
        }

        jogador = player.transform;
        StartCoroutine(IniciarAtracao());
    }

    IEnumerator IniciarAtracao()
    {
        yield return new WaitForSeconds(tempoEspera);
        atraindo = true;
    }

    void Update()
    {
        if (!atraindo || coletando || jogador == null)
            return;

        // Mira aproximadamente no centro do jogador.
        Vector3 destino = jogador.position + Vector3.up * 0.5f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidadeAtracao * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, destino) <= distanciaColeta)
        {
            coletando = true;

            // Aplica a coleta usando os scripts existentes.
            if (TryGetComponent<OrbPontos>(out _))
            {
                GameManager gm = FindAnyObjectByType<GameManager>();

                if (gm != null)
                {
                    OrbPontos orb = GetComponent<OrbPontos>();
                    gm.AdicionarPontos(orb.pontos);
                }
            }
            else if (TryGetComponent<DropCura>(out _))
            {
                Knockback vida = jogador.GetComponentInParent<Knockback>();

                if (vida != null && vida.vida > 0)
                {
                    vida.vida = Mathf.Min(
                        vida.vida + GetComponent<DropCura>().quantidadeCura,
                        vida.vidaMaxima
                    );
                }
            }

            Destroy(gameObject);
        }
    }
}
