
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AtaqueMago : MonoBehaviour
{
    [Header("Configuração do ataque")]
    private GameManager gameManager;
    public GameObject bolaDeFogoPrefab;
    public Transform pontoDisparo;

    [Header("UI do cooldown")]
    public Image imagemCooldown;

    public float cooldown = 2f;
    public int dano = 25;

    private float proximoDisparo;
    private System.Collections.Generic.List<Collider> bolasDoDisparo =
    new System.Collections.Generic.List<Collider>();

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        // Atualiza o indicador visual do cooldown.
            // Atualiza o indicador visual do cooldown.
            if (imagemCooldown != null)
            {
                float restante = proximoDisparo - Time.time;

                imagemCooldown.fillAmount = cooldown > 0
                    ? 1f - Mathf.Clamp01(restante / cooldown)
                    : 1f;
            }

        // Clique esquerdo para atacar.
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= proximoDisparo &&
            Time.timeScale != 0f)
        {
            Atirar();
            gameManager.TutorialAtacar();
        }
    }

    void Atirar()
{
    if (bolaDeFogoPrefab == null || pontoDisparo == null)
    {
        Debug.LogWarning(
            "Configure a bola de fogo e o ponto de disparo!"
        );
        return;
    }

    Camera cam = Camera.main;

    if (cam == null)
    {
        Debug.LogWarning("Nenhuma câmera MainCamera encontrada!");
        return;
    }

    float distanciaMira = 100f;
    float anguloDispersao = 8f;

    Ray raio = cam.ViewportPointToRay(
        new Vector3(0.5f, 0.5f, 0f)
    );

    Vector3 pontoMira;

    if (Physics.Raycast(
        raio,
        out RaycastHit hit,
        distanciaMira,
        ~0,
        QueryTriggerInteraction.Ignore))
    {
        pontoMira = hit.point;
    }
    else
    {
        pontoMira = raio.origin + raio.direction * distanciaMira;
    }

    Vector3 direcaoCentral =
        (pontoMira - pontoDisparo.position).normalized;

    // Cria as três direções: esquerda, centro e direita.
    Vector3 direcaoEsquerda =
        Quaternion.AngleAxis(-anguloDispersao, cam.transform.up)
        * direcaoCentral;

    Vector3 direcaoDireita =
        Quaternion.AngleAxis(anguloDispersao, cam.transform.up)
        * direcaoCentral;

    CriarBolaDeFogo(direcaoEsquerda);
    CriarBolaDeFogo(direcaoCentral);
    CriarBolaDeFogo(direcaoDireita);

    proximoDisparo = Time.time + cooldown;
}

void CriarBolaDeFogo(Vector3 direcao)
{
    GameObject objeto = Instantiate(
        bolaDeFogoPrefab,
        pontoDisparo.position,
        Quaternion.LookRotation(direcao)
    );

    Collider[] collidersProjetil =
        objeto.GetComponentsInChildren<Collider>();

    // Ignora colisões com o jogador.
    Collider[] collidersJogador =
        transform.root.GetComponentsInChildren<Collider>();

    foreach (Collider jogadorCol in collidersJogador)
    {
        foreach (Collider projetilCol in collidersProjetil)
        {
            if (jogadorCol != null && projetilCol != null)
                Physics.IgnoreCollision(jogadorCol, projetilCol);
        }
    }

    // Ignora colisões com as outras bolas deste disparo.
    foreach (Collider novoCol in collidersProjetil)
    {
        foreach (Collider bolaAnterior in bolasDoDisparo)
        {
            if (novoCol != null && bolaAnterior != null)
                Physics.IgnoreCollision(novoCol, bolaAnterior);
        }
    }

    foreach (Collider col in collidersProjetil)
    {
        if (col != null)
            bolasDoDisparo.Add(col);
    }

    Projectile projetil = objeto.GetComponent<Projectile>();

    if (projetil != null)
    {
        projetil.dano = dano;
        projetil.StartProjectile(direcao, transform.root.gameObject);
    }
    else
    {
        Debug.LogWarning("O prefab precisa ter o componente Projectile!");
        Destroy(objeto);
    }
}
}
