
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

public class EnemyNavMesh : MonoBehaviour
{
    public enum TipoCombate
    {
        Distancia,
        CorpoACorpo
    }

    [Header("Tipo de combate")]
    public TipoCombate tipoCombate = TipoCombate.Distancia;

    [Header("Fuga por vida")]
    [SerializeField, Range(0f, 1f)]
    private float porcentagemVidaFuga = 0.3f;

    private Knockback vidaInimigo;

    [Header("Referências")]
    private Transform player;
    private float proximaBuscaPlayer;
    public NavMeshAgent agent;
    public Transform[] waypoints;

    [Header("Detecção")]
    public float distanciaVisao = 15f;
    [Range(0f, 360f)]
    public float anguloVisao = 120f;
    public float alturaOlhos = 1.5f;

    [Tooltip("Camadas que bloqueiam a visão, como paredes.")]
    public LayerMask obstaculos;

    [Header("Combate")]
    public float distanciaAtaque = 6f;
    public float intervaloTiro = 1.5f;
    private bool podeAtirar = true;
    private Coroutine rotinaDisparo;
    public UnityEvent aoAtirar;

    [Header("Patrulha")]
    public float raioPatrulha = 10f;
    public float distanciaDestino = 1f;
    public float tempoMinimoPatrulha = 2f;
    public float tempoMaximoPatrulha = 5f;

    [Header("Fuga")]
    public float distanciaFuga = 10f;
    public float distanciaEscolhaFuga = 8f;

    [Header("Estados")]
    public EnemyState currentState = EnemyState.RandomPatrol;

    public enum EnemyState
    {
        WayPatrol,
        RandomPatrol,
        Pursuit,
        Attack,
        Flee
    }

    private Vector3 origemPatrulha;
    private float proximaPatrulha;
    private float proximoTiro;
    private int waypointAtual = -1;
    private bool destinoDefinido;

[SerializeField] private float distanciaBuscaNavMesh = 2f;
[SerializeField] private float diferencaAlturaMaxima = 0.8f;

void RecuperarNavMesh()
{
    if (agent == null || !agent.isActiveAndEnabled)
        return;

    if (agent.isOnNavMesh)
        return;

    NavMeshHit hit;

    if (NavMesh.SamplePosition(
        transform.position,
        out hit,
        distanciaBuscaNavMesh,
        agent.areaMask))
    {
        // Evita escolher uma calçada muito acima ou abaixo.
        float diferencaAltura =
            Mathf.Abs(hit.position.y - transform.position.y);

        if (diferencaAltura > diferencaAlturaMaxima)
            return;

        if (agent.Warp(hit.position))
        {
            agent.isStopped = false;
        }
    }
}


    void BuscarPlayer()
{
    if (player != null)
        return;

    if (Time.time < proximaBuscaPlayer)
        return;

    proximaBuscaPlayer = Time.time + 0.5f;

    GameObject encontrado =
        GameObject.FindGameObjectWithTag("Player");

    if (encontrado != null)
    {
        player = encontrado.transform;

        Debug.Log(
            name + ": encontrou o jogador " + encontrado.name
        );
    }
}
    void Start()
    {
        vidaInimigo = GetComponent<Knockback>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject encontrado = GameObject.FindGameObjectWithTag("Player");

            if (encontrado != null)
                player = encontrado.transform;
        }

        if (agent == null)
        {
            Debug.LogError(name + ": NavMeshAgent não encontrado!");
            enabled = false;
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            Debug.LogError(
                name + ": agente desativado ou fora do NavMesh!"
            );
            enabled = false;
            return;
        }

        origemPatrulha = transform.position;

        if (waypoints != null && waypoints.Length > 0)
            ChangeState(EnemyState.WayPatrol);
        else
            ChangeState(EnemyState.RandomPatrol);
    }

    
void Update()
{
    RecuperarNavMesh();
    BuscarPlayer();

    if (agent == null)
{
    Debug.LogWarning(name + ": agent está nulo!");
    return;
}

if (!agent.enabled || !agent.isOnNavMesh)
{
    Debug.LogWarning(
        $"{name}: agente inválido! " +
        $"Enabled={agent.enabled}, OnNavMesh={agent.isOnNavMesh}"
    );
    return;
}

if (agent != null && agent.isActiveAndEnabled)
{
    if (!agent.isOnNavMesh)
    {
        Debug.LogWarning($"{name}: não está sobre o NavMesh!");
    }
    else if (agent.isStopped)
    {
        Debug.LogWarning($"{name}: NavMesh está pausado!");
    }
    else if (!agent.hasPath)
    {
        Debug.LogWarning($"{name}: não possui caminho!");
    }
}

    // A fuga por vida tem prioridade sobre os outros estados.
    if (DeveFugir())
    {
        if (currentState != EnemyState.Flee)
            ChangeState(EnemyState.Flee);
    }
    else if (player != null && currentState != EnemyState.Flee)
    {
        float distanciaPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distanciaPlayer <= distanciaVisao)
        {
            float distanciaNecessaria =
                tipoCombate == TipoCombate.CorpoACorpo
                ? 2f
                : distanciaAtaque;

            if (distanciaPlayer <= distanciaNecessaria)
                ChangeState(EnemyState.Attack);
            else
                ChangeState(EnemyState.Pursuit);
        }
    }

    if (currentState == EnemyState.Flee)
{
    Debug.Log(
        $"{name}: estado Flee | " +
        $"isStopped={agent.isStopped} | " +
        $"hasPath={agent.hasPath} | " +
        $"velocity={agent.velocity.magnitude}"
    );
}

    switch (currentState)
    {
        case EnemyState.WayPatrol:
            WayPatrol();
            break;

        case EnemyState.RandomPatrol:
            RandomPatrol();
            break;

        case EnemyState.Pursuit:
            Pursuit();
            break;

        case EnemyState.Attack:
            Attack();
            break;

        case EnemyState.Flee:
            Flee();
            break;
    }
}


    bool DeveFugir()
{
    if (vidaInimigo == null)
        return false;

    return vidaInimigo.vida <=
        vidaInimigo.vidaMaxima * porcentagemVidaFuga;
}

    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        destinoDefinido = false;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;

        if (newState == EnemyState.Attack)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }


float ObterDistanciaDeAtaque()
{
    return tipoCombate == TipoCombate.CorpoACorpo
        ? 2f
        : distanciaAtaque;
}

bool DetectarPlayer()
{
    if (player == null)
        return false;

    Vector3 origem = transform.position + Vector3.up * alturaOlhos;
    Vector3 alvo = player.position + Vector3.up;
    Vector3 direcao = alvo - origem;

    float distancia = direcao.magnitude;

    if (distancia > distanciaVisao || distancia < 0.01f)
        return false;

    // Verifica o campo de visão horizontal.
    Vector3 direcaoHorizontal = direcao;
    direcaoHorizontal.y = 0f;

    if (direcaoHorizontal.sqrMagnitude > 0.01f)
    {
        float angulo = Vector3.Angle(
            transform.forward,
            direcaoHorizontal
        );

        if (angulo > anguloVisao / 2f)
            return false;
    }

    // Verifica se há obstáculos entre o inimigo e o jogador.
    if (Physics.Raycast(
        origem,
        direcao.normalized,
        out RaycastHit hit,
        distancia,
        ~obstaculos,
        QueryTriggerInteraction.Ignore))
    {
        // O objeto atingido precisa ser o jogador ou um filho dele.
        if (hit.transform == player ||
            hit.transform.IsChildOf(player))
        {
            Debug.DrawLine(origem, hit.point, Color.green);
            return true;
        }

        Debug.DrawLine(origem, hit.point, Color.red);
        return false;
    }

    return false;
}


    void WayPatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            ChangeState(EnemyState.RandomPatrol);
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        if (destinoDefinido &&
            (agent.pathPending ||
             agent.remainingDistance > distanciaDestino))
            return;

        // Escolhe um waypoint diferente do anterior.
        int novoIndice = waypointAtual;

        if (waypoints.Length > 1)
        {
            int tentativas = 0;

            while (novoIndice == waypointAtual && tentativas < 10)
            {
                novoIndice = Random.Range(0, waypoints.Length);
                tentativas++;
            }
        }
        else
        {
            novoIndice = 0;
        }

        if (waypoints[novoIndice] == null)
            return;

        waypointAtual = novoIndice;

        agent.SetDestination(waypoints[waypointAtual].position);
        destinoDefinido = true;
    }

    void RandomPatrol()
    {
        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        if (destinoDefinido &&
            (agent.pathPending ||
             agent.remainingDistance > distanciaDestino))
            return;

        if (Time.time < proximaPatrulha)
            return;

        Vector3 aleatorio = origemPatrulha +
            new Vector3(
                Random.Range(-raioPatrulha, raioPatrulha),
                0f,
                Random.Range(-raioPatrulha, raioPatrulha)
            );

        if (NavMesh.SamplePosition(
            aleatorio,
            out NavMeshHit hit,
            raioPatrulha,
            NavMesh.AllAreas))
        {
            NavMeshPath caminho = new NavMeshPath();

            if (agent.CalculatePath(hit.position, caminho) &&
                caminho.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(hit.position);
                destinoDefinido = true;
            }
        }

        proximaPatrulha = Time.time + Random.Range(
            tempoMinimoPatrulha,
            tempoMaximoPatrulha
        );
    }



void Pursuit()
{
    if (player == null)
    {
        VoltarPatrulha();
        return;
    }

    if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        return;

    // Ignora a diferença de altura entre os pivôs.
    Vector3 posInimigo = transform.position;
    Vector3 posPlayer = player.position;
    posInimigo.y = 0f;
    posPlayer.y = 0f;

    float distancia = Vector3.Distance(posInimigo, posPlayer);

    float distanciaNecessaria =
        tipoCombate == TipoCombate.CorpoACorpo
        ? 1.2f
        : distanciaAtaque;

    if (distancia <= distanciaNecessaria)
    {
        ChangeState(EnemyState.Attack);
        return;
    }

    agent.isStopped = false;

    // Permite chegar bem perto do destino.
    agent.stoppingDistance = 0f;
    agent.autoBraking = true;

    // Atualiza o destino somente quando necessário.
    if (NavMesh.SamplePosition(
        player.position,
        out NavMeshHit hit,
        10f,
        NavMesh.AllAreas))
    {
        if (!agent.hasPath ||
            Vector3.Distance(agent.destination, hit.position) > 0.5f)
        {
            agent.SetDestination(hit.position);
        }
    }

    Debug.DrawLine(transform.position, player.position, Color.yellow);
}

    IEnumerator DispararComIntervalo()
    {
        podeAtirar = false;

        aoAtirar?.Invoke();

        Debug.Log(name + " disparou. Aguardando " + intervaloTiro + " segundos.");

        yield return new WaitForSeconds(intervaloTiro);

        podeAtirar = true;
    }

    void Attack()
    {
        if (player == null)
        {
            VoltarPatrulha();
            return;
        }

        float distancia = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distancia > distanciaAtaque * 1.2f)
        {
            ChangeState(EnemyState.Pursuit);
            return;
        }

        float distanciaNecessaria =
            tipoCombate == TipoCombate.CorpoACorpo
            ? 3f
            : distanciaAtaque;

            if (distancia > distanciaNecessaria)
            {
                ChangeState(EnemyState.Pursuit);
                return;
            }

        agent.isStopped = true;

        // Gira apenas no eixo horizontal.
        Vector3 olhar = player.position - transform.position;
        olhar.y = 0f;

        if (olhar.sqrMagnitude > 0.01f)
        {
            Quaternion rotacao = Quaternion.LookRotation(olhar);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotacao,
                360f * Time.deltaTime
            );
        }

        
        if (tipoCombate == TipoCombate.Distancia)
        {
            if (podeAtirar)
            {
                rotinaDisparo = StartCoroutine(DispararComIntervalo());
            }
        }
        else
        {
            EspadachimAtaque ataque = GetComponent<EspadachimAtaque>();

            if (ataque != null)
                ataque.TentarAtacar();
        }
    }

    
void Flee()
{
    if (player == null)
    {
        VoltarPatrulha();
        return;
    }

    if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        return;

    agent.isStopped = false;
    agent.stoppingDistance = 0f;

    // Continua fugindo enquanto estiver seguindo um caminho válido.
    if (agent.hasPath &&
        (agent.pathPending ||
         agent.remainingDistance > distanciaDestino))
    {
        return;
    }

    Vector3 direcao = transform.position - player.position;
    direcao.y = 0f;

    if (direcao.sqrMagnitude < 0.01f)
        direcao = -transform.forward;

    direcao.Normalize();

    // Tenta encontrar um ponto de fuga, começando mais perto.
    float[] distanciasTentativa = { distanciaFuga, 5f, 3f };

    foreach (float distancia in distanciasTentativa)
    {
        Vector3 destino = transform.position + direcao * distancia;

        if (!NavMesh.SamplePosition(
            destino,
            out NavMeshHit hit,
            distanciaEscolhaFuga,
            NavMesh.AllAreas))
        {
            continue;
        }

        NavMeshPath caminho = new NavMeshPath();

        if (agent.CalculatePath(hit.position, caminho) &&
            caminho.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(hit.position);
            destinoDefinido = true;

            Debug.Log(name + " está fugindo para " + hit.position);
            return;
        }
    }

    // Se não encontrou um ponto na direção escolhida,
    // tenta fugir para algum lado próximo.
    for (int i = 0; i < 8; i++)
    {
        Vector3 direcaoAlternativa =
            Quaternion.Euler(0f, i * 45f, 0f) * direcao;

        Vector3 destinoAlternativo =
            transform.position + direcaoAlternativa * 4f;

        if (!NavMesh.SamplePosition(
            destinoAlternativo,
            out NavMeshHit hit,
            distanciaEscolhaFuga,
            NavMesh.AllAreas))
        {
            continue;
        }

        NavMeshPath caminho = new NavMeshPath();

        if (agent.CalculatePath(hit.position, caminho) &&
            caminho.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(hit.position);
            destinoDefinido = true;

            Debug.Log(name + " está fugindo por uma rota alternativa.");
            return;
        }
    }

    Debug.LogWarning(name + ": não encontrou uma rota de fuga válida!");
}


    void VoltarPatrulha()
    {
        if (waypoints != null && waypoints.Length > 0)
            ChangeState(EnemyState.WayPatrol);
        else
            ChangeState(EnemyState.RandomPatrol);
    }

    
void CallBackup()
{
    EnemyNavMesh[] aliados =
    FindObjectsByType<EnemyNavMesh>();

    int aliadosChamados = 0;

    foreach (EnemyNavMesh aliado in aliados)
    {
        if (aliado == this)
            continue;

        // Ignora inimigos desativados.
        if (!aliado.isActiveAndEnabled)
            continue;

        // Atualiza a referência ao jogador, se necessário.
        if (aliado.player == null)
        {
            GameObject jogador = GameObject.FindGameObjectWithTag("Player");

            if (jogador != null)
                aliado.player = jogador.transform;
        }

        if (aliado.player == null)
            continue;

        // Verifica a distância horizontal entre os inimigos.
        Vector3 posicaoInimigo = transform.position;
        Vector3 posicaoAliado = aliado.transform.position;

        posicaoInimigo.y = 0f;
        posicaoAliado.y = 0f;

        float distancia = Vector3.Distance(
            posicaoInimigo,
            posicaoAliado
        );

        // Só chama inimigos próximos que não estejam fugindo.
        if (distancia <= 10f && !aliado.DeveFugir())
        {
            aliado.ChangeState(EnemyState.Pursuit);

            aliadosChamados++;

            Debug.Log(
                $"{name} chamou {aliado.name} para ajudar! " +
                $"Distância: {distancia:F1}m"
            );
        }
    }

    Debug.Log(
        $"{name}: chamados de reforço concluídos. " +
        $"Aliados chamados: {aliadosChamados}"
    );
}


    
public void TakeDamage(int dano, bool causadoPeloPlayer)
{
    if (vidaInimigo == null)
        vidaInimigo = GetComponent<Knockback>();

    if (vidaInimigo == null)
    {
        Debug.LogError(name + ": não possui Knockback!");
        return;
    }

    vidaInimigo.ReceberAtaque(
        dano,
        Vector3.zero,
        0f,
        causadoPeloPlayer
    );

    if (vidaInimigo.vida <= 0)
        return;

    if (DeveFugir())
    {
        ChangeState(EnemyState.Flee);
        Debug.Log(name + " está fugindo e não chamará reforços.");
    }
    else
    {
        ChangeState(EnemyState.Pursuit);

        Debug.Log(name + " foi atingido e está chamando reforços!");

        CallBackup();
    }
}
}
