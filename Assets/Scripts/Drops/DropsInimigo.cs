
using UnityEngine;

public class DropsInimigo : MonoBehaviour
{
    [Header("Prefabs dos drops")]
    public GameObject orbPontosPrefab;
    public GameObject orbCuraPrefab;

    [Header("Chances de drop (%)")]
    [Range(0, 100)]
    public float chanceOrb = 75f;

    [Range(0, 100)]
    public float chanceCura = 35f;

    [Header("Valores")]
    public int pontosPorOrb = 10;

    private bool jaDropou = false;

    [Header("Quantidade e espaçamento das orbs")]
    [Range(1, 10)]
    public int quantidadeOrbs = 3;

    public float distanciaMinimaEntreOrbs = 1.5f;
    public float raioDistribuicaoOrbs = 3f;

    [Header("Verificação do terreno")]
    public LayerMask camadaChao;
    public LayerMask camadasObstaculos;
    public float alturaInicialRaycast = 10f;
    public float raioVerificacao = 0.3f;
    public float distanciaMinimaDoChao = 0.05f;
    public int tentativasPorOrb = 30;

    
public void SoltarDrops()
{
    if (jaDropou)
        return;

    jaDropou = true;

    // DROP DE PONTOS
    if (orbPontosPrefab != null &&
        Random.Range(0f, 100f) < chanceOrb)
    {
        Vector3[] posicoes = new Vector3[quantidadeOrbs];
        int orbsCriadas = 0;

        for (int i = 0; i < quantidadeOrbs; i++)
        {
            bool posicaoValida = false;
            Vector3 posicao = Vector3.zero;

            for (int tentativa = 0; tentativa < tentativasPorOrb; tentativa++)
            {
                Vector2 aleatorio =
                    Random.insideUnitCircle * raioDistribuicaoOrbs;

                Vector3 origem = transform.position +
                    new Vector3(
                        aleatorio.x,
                        alturaInicialRaycast,
                        aleatorio.y
                    );

                if (!Physics.Raycast(
                    origem,
                    Vector3.down,
                    out RaycastHit hit,
                    alturaInicialRaycast * 2f,
                    camadaChao,
                    QueryTriggerInteraction.Ignore))
                    continue;

                posicao = hit.point +
                    Vector3.up * distanciaMinimaDoChao;

                bool muitoPerto = false;

                for (int j = 0; j < orbsCriadas; j++)
                {
                    Vector3 a = posicao;
                    Vector3 b = posicoes[j];

                    a.y = 0f;
                    b.y = 0f;

                    if (Vector3.Distance(a, b) <
                        distanciaMinimaEntreOrbs)
                    {
                        muitoPerto = true;
                        break;
                    }
                }

                if (muitoPerto)
                    continue;

                Vector3 centro = posicao +
                    Vector3.up * raioVerificacao;

                if (Physics.CheckSphere(
                    centro,
                    raioVerificacao,
                    camadasObstaculos,
                    QueryTriggerInteraction.Ignore))
                    continue;

                posicaoValida = true;
                break;
            }

            if (!posicaoValida)
                continue;

            GameObject orb = Instantiate(
                orbPontosPrefab,
                posicao,
                Quaternion.identity
            );

            OrbPontos scriptOrb = orb.GetComponent<OrbPontos>();

            if (scriptOrb != null)
                scriptOrb.pontos = pontosPorOrb;

            posicoes[orbsCriadas] = posicao;
            orbsCriadas++;
        }
    }

    // DROP DE CURA — independente do drop de pontos
    if (orbCuraPrefab != null &&
        Random.Range(0f, 100f) < chanceCura)
    {
        bool encontrouPosicao = false;

        for (int tentativa = 0;
             tentativa < tentativasPorOrb;
             tentativa++)
        {
            Vector2 aleatorio =
                Random.insideUnitCircle * raioDistribuicaoOrbs;

            Vector3 origem = transform.position +
                new Vector3(
                    aleatorio.x,
                    alturaInicialRaycast,
                    aleatorio.y
                );

            if (!Physics.Raycast(
                origem,
                Vector3.down,
                out RaycastHit hit,
                alturaInicialRaycast * 2f,
                camadaChao,
                QueryTriggerInteraction.Ignore))
                continue;

            Vector3 posicao = hit.point +
                Vector3.up * distanciaMinimaDoChao;

            Vector3 centro = posicao +
                Vector3.up * raioVerificacao;

            if (Physics.CheckSphere(
                centro,
                raioVerificacao,
                camadasObstaculos,
                QueryTriggerInteraction.Ignore))
                continue;

            Instantiate(
                orbCuraPrefab,
                posicao,
                Quaternion.identity
            );

            Debug.Log("Orb de cura criada em: " + posicao);

            encontrouPosicao = true;
            break;
        }

        if (!encontrouPosicao)
            Debug.LogWarning(
                "Drop de cura: nenhuma posição válida encontrada."
            );
    }
}

}
