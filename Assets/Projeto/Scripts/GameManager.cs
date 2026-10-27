
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Modo de jogo")]
    public bool tutorial = false;

    [Header("Sistema de pausa")]
    public GameObject painelPausa;

    private bool jogoPausado = false;

    [Tooltip("Nome da classe selecionada, por exemplo: Atirador, Mago ou Espadachim")]
    private string classeEscolhida = "Atirador";

    [Header("Countdown")]
    public float countdownTime = 5f;
    public TextMeshProUGUI timerLabel;
    public bool countdown = true;

    [Header("Tutorial")]
    public TextMeshProUGUI tutorialLabel;
    private int etapaTutorial = 0;
    private int killsTutorial = 0;
    private bool tutorialIniciado = false;
    private bool inimigosTutorialLiberados = false;
    private bool tutorialConcluido = false;
    private MapaGerador mapaGerador;

    [Header("Inimigos do Tutorial")]
    public GameObject[] prefabsInimigos;
    public float distanciaMinimaSpawn = 30f;
    public float distanciaMaximaSpawn = 60f;
    public int tentativasSpawn = 100;

    private readonly System.Collections.Generic.List<GameObject>
        inimigosTutorial = new();

    private bool spawnTutorialRealizado = false;

    [Header("Player Stats")]
    public string playerName;
    public int playerKills = 0;
    public int playerDeaths = 0;

    [Header("Pontuação da partida")]
    public TextMeshProUGUI textoPontuacao;
    public int playerScore = 0;

    public void AdicionarPontos(int quantidade)
{
    if (quantidade <= 0)
        return;

    playerScore += quantidade;
    EstatisticasPartida.pontuacao = playerScore;

    AtualizarTextoPontuacao();
}

    [Header("UI")]
    public RectTransform frags;
    public TextMeshProUGUI playerNameLabel;
    public TextMeshProUGUI playerKillsLabel;
    public TextMeshProUGUI playerDeathsLabel;

    void AtualizarTextoPontuacao()
{
    if (textoPontuacao != null)
    {
        textoPontuacao.text = $"Pontos:\n<size=40>{playerScore}</size>";
    }
}

    void Awake()
    {
        if (frags != null)
            frags = frags.GetComponent<RectTransform>();
    }

void Start()
{
    EstatisticasPartida.Resetar();
    AtualizarTextoPontuacao();

    if (painelPausa != null)
            painelPausa.SetActive(false);

        jogoPausado = false;
        Time.timeScale = 1f;

    AtualizarFrags();

    if (tutorial)
    {
        countdown = false;
        Time.timeScale = 1f;

        if (timerLabel != null)
            timerLabel.gameObject.SetActive(false);

        StartCoroutine(PrepararTutorial());
    }
    else
    {
        Time.timeScale = 0f;

        if (timerLabel != null)
        {
            timerLabel.gameObject.SetActive(true);
            timerLabel.text = countdownTime.ToString();
        }

        StartCoroutine(StartCountdown());
    }
}

void ControlarPausa()
{
    if (Keyboard.current == null)
        return;

    // ESC abre ou fecha o painel.
    if (Keyboard.current.escapeKey.wasPressedThisFrame)
    {
        if (jogoPausado)
            RetomarJogo();
        else
            PausarJogo();
    }

    // ENTER volta ao menu enquanto a pausa está aberta.
    if (jogoPausado &&
        (Keyboard.current.enterKey.wasPressedThisFrame ||
         Keyboard.current.numpadEnterKey.wasPressedThisFrame))
    {
        VoltarAoMenu();
    }
}

public void PausarJogo()
{
    jogoPausado = true;
    Time.timeScale = 0f;

    if (painelPausa != null)
        painelPausa.SetActive(true);

    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
}

public void RetomarJogo()
{
    jogoPausado = false;
    Time.timeScale = 1f;

    if (painelPausa != null)
        painelPausa.SetActive(false);

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
}

public void VoltarAoMenu()
{
    Time.timeScale = 1f;
    jogoPausado = false;

    SceneManager.LoadScene("Menu");
}

IEnumerator PrepararTutorial()
{
    // Espera o MapaGerador existir.
    while (mapaGerador == null)
    {
        mapaGerador = FindAnyObjectByType<MapaGerador>();
        yield return null;
    }

    // Espera o prefab escolhido ser instanciado de verdade.
    while (mapaGerador.PlayerInstanciado == null)
        yield return null;

    // Identifica a classe usando o nome do prefab instanciado.
    string nomePrefab = mapaGerador.PlayerInstanciado.name;
    nomePrefab = nomePrefab.Replace("(Clone)", "").Trim();

    if (nomePrefab.IndexOf(
        "Atirador",
        System.StringComparison.OrdinalIgnoreCase) >= 0)
    {
        classeEscolhida = "Atirador";
    }
    else if (nomePrefab.IndexOf(
        "Espadachim",
        System.StringComparison.OrdinalIgnoreCase) >= 0)
    {
        classeEscolhida = "Espadachim";
    }
    else if (nomePrefab.IndexOf(
        "Mago",
        System.StringComparison.OrdinalIgnoreCase) >= 0)
    {
        classeEscolhida = "Mago";
    }

    Debug.Log("Classe detectada para o tutorial: " + classeEscolhida);

    tutorialIniciado = true;
    etapaTutorial = 0;
    AtualizarTextoTutorial();
}


    void Update()
    {
        ControlarPausa();

        if (jogoPausado)
            return;
        
        if (Keyboard.current != null &&
            Keyboard.current.tabKey.isPressed &&
            frags != null)
        {
            frags.anchoredPosition = Vector2.zero;
        }
        else if (frags != null)
        {
            frags.anchoredPosition = new Vector2(0, 400);
        }

        // Detecta WASD para a primeira tarefa.
        if (tutorial && tutorialIniciado &&
            etapaTutorial == 0 &&
            Keyboard.current != null)
        {
            bool movimentou =
                Keyboard.current.wKey.isPressed ||
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.sKey.isPressed ||
                Keyboard.current.dKey.isPressed;

            if (movimentou)
                ConcluirEtapaTutorial();
        }
    }

    IEnumerator StartCountdown()
    {
        while (countdown)
        {
            if (countdownTime > 0f)
            {
                yield return new WaitForSecondsRealtime(1f);

                countdownTime -= 1f;

                if (timerLabel != null)
                    timerLabel.text = countdownTime.ToString();
            }
            else
            {
                if (timerLabel != null)
                    timerLabel.text = "Vai!";

                countdown = false;
                Time.timeScale = 1f;

                yield return new WaitForSecondsRealtime(1f);

                if (timerLabel != null)
                    timerLabel.gameObject.SetActive(false);

                yield break;
            }
        }
    }

    // Chame estes métodos nos scripts quando as ações realmente acontecerem.

    public void TutorialPular()
    {
        RegistrarAcaoTutorial(1);
    }

    public void TutorialDash()
    {
        RegistrarAcaoTutorial(2);
    }

    public void TutorialAtacar()
    {
        RegistrarAcaoTutorial(3);
    }

    public void TutorialRecarregar()
    {
        if (classeEscolhida.ToLower() == "atirador")
            RegistrarAcaoTutorial(4);
    }

    public void TutorialUsarSkill()
    {
        int etapaSkill = classeEscolhida.ToLower() == "atirador" ? 5 : 4;
        RegistrarAcaoTutorial(etapaSkill);
    }

    void RegistrarAcaoTutorial(int etapaEsperada)
    {
        if (!tutorial || !tutorialIniciado || tutorialConcluido)
            return;

        if (etapaTutorial == etapaEsperada)
            ConcluirEtapaTutorial();
    }

    void ConcluirEtapaTutorial()
    {
        if (!tutorial || tutorialConcluido)
            return;

        etapaTutorial++;

        int etapaFinalAntesDosInimigos =
        classeEscolhida.ToLower() == "atirador" ? 6 : 5;

    if (etapaTutorial == etapaFinalAntesDosInimigos)
    {
        StartCoroutine(IniciarEtapaDeCombate());
        return;
    }

        AtualizarTextoTutorial();
    }

    IEnumerator IniciarEtapaDeCombate()
{
    inimigosTutorialLiberados = true;

    if (tutorialLabel != null)
    {
        tutorialLabel.text =
            "TUTORIAL: Derrote 3 inimigos!\n" +
            "Inimigos derrotados: 0/3";
    }

    yield return StartCoroutine(SpawnarInimigosTutorial());
}

    void AtualizarTextoTutorial()
    {
        if (tutorialLabel == null)
            return;

        tutorialLabel.gameObject.SetActive(true);

        switch (etapaTutorial)
        {
            case 0:
                tutorialLabel.text = "TUTORIAL\n[W], [A], [S], [D] para mover";
                break;

            case 1:
                tutorialLabel.text = "TUTORIAL\n[Espaço] para pular";
                break;

            case 2:
                tutorialLabel.text = "TUTORIAL\n[Shift] para dash";
                break;

            case 3:
                tutorialLabel.text = "TUTORIAL\nClique esquerdo para atacar";
                break;

            case 4:
                if (classeEscolhida.ToLower() == "atirador")
                    tutorialLabel.text = "TUTORIAL\n[R] para recarregar";
                else
                    tutorialLabel.text = "TUTORIAL\nPressione [1] ou [2] para usar skills";
                break;

            case 5:
                tutorialLabel.text = "TUTORIAL\nPressione [1] ou [2] para usar skills";
                break;
        }
    }

    public void RegistrarKill()
    {
        playerKills++;
        EstatisticasPartida.kills = playerKills;

        Debug.Log(
            playerName + " eliminou um inimigo! Kills: " + playerKills
        );

        AtualizarFrags();

        if (tutorial && inimigosTutorialLiberados &&
            !tutorialConcluido)
        {
            killsTutorial++;

            if (tutorialLabel != null)
            {
                tutorialLabel.text =
                    "TUTORIAL: Derrote 3 inimigos!\n" +
                    "Inimigos derrotados: " +
                    Mathf.Min(killsTutorial, 3) + "/3";
            }

            if (killsTutorial >= 3)
                FinalizarTutorial();
        }
    }

    void FinalizarTutorial()
    {
        tutorialConcluido = true;

        if (tutorialLabel != null)
            tutorialLabel.text = "TUTORIAL CONCLUÍDO!";

        StartCoroutine(VoltarAoMenuDepoisDoTutorial());
    }

    private IEnumerator VoltarAoMenuDepoisDoTutorial()
    {
        yield return new WaitForSeconds(2f);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Menu");
    }

    public void RegistrarMorte()
{
    playerDeaths++;

    // Perde 10% da pontuação atual.
    // A conversão para int arredonda para baixo.
    playerScore = Mathf.FloorToInt(playerScore * 0.9f);

    EstatisticasPartida.mortes = playerDeaths;
    EstatisticasPartida.pontuacao = playerScore;

    Debug.Log(
        $"O jogador morreu! Pontuação atual: {playerScore}"
    );

    AtualizarTextoPontuacao();
}

    void AtualizarFrags()
    {
        if (playerNameLabel != null)
            playerNameLabel.text = playerName;

        if (playerKillsLabel != null)
            playerKillsLabel.text = playerKills.ToString();

        if (playerDeathsLabel != null)
            playerDeathsLabel.text = playerDeaths.ToString();
    }

    IEnumerator SpawnarInimigosTutorial()
{
    if (spawnTutorialRealizado)
        yield break;

    spawnTutorialRealizado = true;

    Transform jogador = GameObject.FindGameObjectWithTag("Player")?.transform;

    if (jogador == null)
    {
        Debug.LogError("Jogador não encontrado!");
        yield break;
    }

    if (prefabsInimigos == null || prefabsInimigos.Length == 0)
    {
        Debug.LogError("Configure os prefabs dos inimigos no GameManager!");
        yield break;
    }

    inimigosTutorial.Clear();

    for (int i = 0; i < 3; i++)
    {
        bool encontrouPosicao = false;

        for (int tentativa = 0; tentativa < tentativasSpawn; tentativa++)
        {
            Vector2 direcao = Random.insideUnitCircle.normalized;

            float distancia = Random.Range(
                distanciaMinimaSpawn,
                distanciaMaximaSpawn
            );

            Vector3 ponto = jogador.position + new Vector3(
                direcao.x * distancia,
                0f,
                direcao.y * distancia
            );

            if (!UnityEngine.AI.NavMesh.SamplePosition(
                ponto,
                out UnityEngine.AI.NavMeshHit hit,
                5f,
                UnityEngine.AI.NavMesh.AllAreas))
            {
                continue;
            }

            if (Vector3.Distance(hit.position, jogador.position)
                < distanciaMinimaSpawn)
            {
                continue;
            }

            bool pertoDeOutro = false;

            foreach (GameObject existente in inimigosTutorial)
            {
                if (existente != null &&
                    Vector3.Distance(
                        hit.position,
                        existente.transform.position
                    ) < 10f)
                {
                    pertoDeOutro = true;
                    break;
                }
            }

            if (pertoDeOutro)
                continue;

            GameObject prefab = prefabsInimigos[
                Random.Range(0, prefabsInimigos.Length)
            ];

            GameObject inimigo = Instantiate(
                prefab,
                hit.position,
                Quaternion.identity
            );

            inimigosTutorial.Add(inimigo);
            encontrouPosicao = true;
            break;
        }

        if (!encontrouPosicao)
        {
            Debug.LogWarning(
                "Não foi possível encontrar uma posição para o inimigo " +
                (i + 1) + " do tutorial."
            );
        }

        yield return null;
    }
}
}
