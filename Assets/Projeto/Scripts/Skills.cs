
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Skills : MonoBehaviour
{
    public enum Classe
    {
        Atirador,
        Espadachim,
        Mago
    }

    [Header("Controle da partida")]
    private GameManager gameManager;

    [Header("Classe do personagem")]
    public Classe classe = Classe.Atirador;

    [Header("Interface")]
    public Image[] skill_Image;

    [Header("Cooldowns")]
    public float cd_1 = 8f;
    public float cd_2 = 12f;

    // Atirador

    [Header("Referências do Atirador")]
    public Transform pontoDisparo;
    public GameObject projetilEspecialPrefab;
    public ShootPool arma;

    [Header("Skill 1 - Tiro Poderoso")]
    public int danoSkill1 = 60;

    [Header("Skill 2 - Metralhadora")]
    public float duracaoMetralhadora = 5f;
    [Range(0.1f, 1f)]
    public float multiplicadorIntervaloTiro = 0.4f;

    // Espadachim

    [Header("Skill 1 - Arco de Fogo")]
    public GameObject arcoFogoPrefab;
    public Transform pontoDisparoEspadachim;
    public int danoArcoFogo = 40;
    public float duracaoArcoFogo = 5f;

    [Header("Skill 2 - Corrida do Espadachim")]
    public PlayerMove movimentoPlayer;
    public float duracaoCorrida = 4f;
    public float multiplicadorVelocidade = 1.6f;

    // Mago

    [Header("Referências do Mago")]
    public Knockback vidaPlayer;
    public Transform centroEscudo;

    [Header("Skill 1 - Cura Arcana")]
    public int curaMago = 20;

    [Header("Skill 2 - Escudo Ígneo")]
    public GameObject bolaFogoEscudoPrefab;
    public int quantidadeBolasFogo = 6;
    public int danoBolasFogo = 25;
    public float raioEscudo = 2f;
    public float velocidadeRotacaoEscudo = 100f;
    public float duracaoEscudo = 6f;

    private bool skill1Disponivel = true;
    private bool skill2Disponivel = true;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if (skill_Image != null)
        {
            foreach (Image imagem in skill_Image)
            {
                if (imagem != null)
                    imagem.fillAmount = 1f;
            }
        }
    }

    void Update()
{
    if (Keyboard.current == null)
        return;

    // Bloqueia as habilidades durante a contagem regressiva.
    if (gameManager != null && gameManager.countdownTime > 0)
        return;

    AtualizarCooldowns();

    if (Keyboard.current.digit1Key.wasPressedThisFrame &&
        skill1Disponivel)
    {
        UsarSkill1();
    }

    if (Keyboard.current.digit2Key.wasPressedThisFrame &&
        skill2Disponivel)
    {
        UsarSkill2();
    }
}

    void AtualizarCooldowns()
    {
        if (skill_Image == null || skill_Image.Length < 2)
            return;

        if (!skill1Disponivel)
        {
            skill_Image[0].fillAmount +=
                Time.deltaTime / cd_1;

            if (skill_Image[0].fillAmount >= 1f)
            {
                skill_Image[0].fillAmount = 1f;
                skill1Disponivel = true;
            }
        }

        if (!skill2Disponivel)
        {
            skill_Image[1].fillAmount +=
                Time.deltaTime / cd_2;

            if (skill_Image[1].fillAmount >= 1f)
            {
                skill_Image[1].fillAmount = 1f;
                skill2Disponivel = true;
            }
        }
    }

    void UsarSkill1()
    {
        if (classe == Classe.Atirador)
        {
            AtirarProjetilEspecial();
        }
        else if (classe == Classe.Espadachim)
        {
            AtirarArcoFogo();
        }
        else if (classe == Classe.Mago)
        {
            CurarMago();
        }

        IniciarCooldown(0);
    }

    void UsarSkill2()
    {
        if (classe == Classe.Atirador)
        {
            AtivarMetralhadora();
        }
        else if (classe == Classe.Espadachim)
        {
            if (movimentoPlayer != null)
            {
                movimentoPlayer.AtivarCorrida(
                    duracaoCorrida,
                    multiplicadorVelocidade
                );
            }
            else
            {
                Debug.LogWarning(
                    "Configure o PlayerMove no Inspector!"
                );
            }
        }
        else if (classe == Classe.Mago)
        {
            AtivarEscudoFogo();
        }

        IniciarCooldown(1);
    }

    void AtirarProjetilEspecial()
{
    if (projetilEspecialPrefab == null || pontoDisparo == null)
    {
        Debug.LogWarning("Configure o prefab especial e o ponto de disparo!");
        return;
    }

    Camera cam = Camera.main;

    if (cam == null)
    {
        Debug.LogWarning("Nenhuma câmera com a tag MainCamera foi encontrada!");
        return;
    }

    float distanciaMira = 100f;

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

    Vector3 direcao = (pontoMira - pontoDisparo.position).normalized;

    GameObject objeto = Instantiate(
        projetilEspecialPrefab,
        pontoDisparo.position,
        Quaternion.LookRotation(direcao)
    );

    Projectile projetil = objeto.GetComponent<Projectile>();

    if (projetil != null)
    {
        projetil.dano = danoSkill1;
        projetil.StartProjectile(direcao, null);
    }
    else
    {
        Debug.LogWarning(
            "O prefab especial precisa ter o script Projectile!"
        );
    }
}

    void AtivarMetralhadora()
    {
        if (arma == null)
        {
            Debug.LogWarning(
                "Configure a arma no Inspector!"
            );
            return;
        }

        StartCoroutine(MetralhadoraTemporaria());
    }

    IEnumerator MetralhadoraTemporaria()
    {
        // A implementação do aumento da cadência fica na arma.
        arma.AtivarMetralhadora(
            duracaoMetralhadora,
            multiplicadorIntervaloTiro
        );

        yield return null;
    }

    void AtirarArcoFogo()
{
    if (arcoFogoPrefab == null ||
        pontoDisparoEspadachim == null)
    {
        Debug.LogWarning(
            "Configure o prefab e o ponto de disparo do arco de fogo!"
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

    Vector3 direcao =
        (pontoMira - pontoDisparoEspadachim.position).normalized;

    GameObject objeto = Instantiate(
        arcoFogoPrefab,
        pontoDisparoEspadachim.position,
        Quaternion.LookRotation(direcao)
    );

    ArcoFogo arco = objeto.GetComponent<ArcoFogo>();

    if (arco != null)
    {
        arco.Inicializar(
            direcao,
            danoArcoFogo,
            duracaoArcoFogo,
            transform
        );
    }
    else
    {
        Debug.LogWarning(
            "O prefab precisa ter o script ArcoFogo!"
        );

        Destroy(objeto);
    }
}

void CurarMago()
{
    if (vidaPlayer == null)
    {
        Debug.LogWarning("Configure o Knockback do jogador!");
        return;
    }

    vidaPlayer.vida = Mathf.Min(
        vidaPlayer.vida + curaMago,
        vidaPlayer.vidaMaxima
    );

    Debug.Log("Mago curou 20 pontos de vida!");
}

void AtivarEscudoFogo()
{
    if (bolaFogoEscudoPrefab == null)
    {
        Debug.LogWarning("Configure o prefab das bolas de fogo!");
        return;
    }

    Transform centro = centroEscudo != null
        ? centroEscudo
        : transform;

    GameObject objeto = new GameObject("EscudoFogo");

    EscudoFogo escudo = objeto.AddComponent<EscudoFogo>();

    escudo.Inicializar(
        centro,
        bolaFogoEscudoPrefab,
        quantidadeBolasFogo,
        danoBolasFogo,
        raioEscudo,
        velocidadeRotacaoEscudo,
        duracaoEscudo
    );
}

    void IniciarCooldown(int indice)
    {
        if (skill_Image == null ||
            skill_Image.Length <= indice ||
            skill_Image[indice] == null)
        {
            Debug.LogWarning(
                "Configure as duas imagens das skills no Inspector!"
            );
            return;
        }

        skill_Image[indice].fillAmount = 0f;

        if (indice == 0)
            skill1Disponivel = false;
        else
            skill2Disponivel = false;
    }
}
