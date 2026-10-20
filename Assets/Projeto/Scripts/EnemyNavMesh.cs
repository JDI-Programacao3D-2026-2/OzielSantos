
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyNavMesh : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private int hp = 100;
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int hpFuga = 30;

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
        BuscarPlayer();

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        // Procura o jogador caso ainda não tenha referência.
        if (player == null)
        {
            BuscarPlayer();
        }
        else if (currentState != EnemyState.Flee)
        {
            float distanciaPlayer = Vector3.Distance(
                transform.position,
                player.position
            );

            if (hp <= hpFuga)
            {
                ChangeState(EnemyState.Flee);
            }
            else if (distanciaPlayer <= distanciaVisao)
            {
                if (distanciaPlayer <= distanciaAtaque)
                {
                    ChangeState(EnemyState.Attack);
                }
                else
                {
                    ChangeState(EnemyState.Pursuit);
                }
            }
        }

        // Foge automaticamente quando a vida fica baixa.
        if (hp <= hpFuga && currentState != EnemyState.Flee)
            ChangeState(EnemyState.Flee);

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
    {
        Debug.LogWarning(name + ": agente fora do NavMesh!");
        return;
    }

    float distancia = Vector3.Distance(
        transform.position,
        player.position
    );

    if (distancia <= distanciaAtaque)
    {
        ChangeState(EnemyState.Attack);
        return;
    }

    agent.isStopped = false;
    agent.stoppingDistance = distanciaAtaque * 0.8f;

    // Procura um ponto navegável perto do jogador.
    if (NavMesh.SamplePosition(
    player.position,
    out NavMeshHit hit,
    10f,
    NavMesh.AllAreas))
{
    Debug.Log(
        "NavMesh encontrado! Player: " + player.position +
        " | Ponto encontrado: " + hit.position
    );

    agent.SetDestination(hit.position);
}
else
{
    Debug.LogError(
        "Não existe NavMesh num raio de 10 metros do jogador! " +
        "Posição do jogador: " + player.position
    );
}

    Debug.DrawLine(
        transform.position,
        player.position,
        Color.yellow
    );
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

        if (Time.time >= proximoTiro)
        {
            aoAtirar?.Invoke();
            proximoTiro = Time.time + intervaloTiro;
        }
    }

    void Flee()
    {
        if (player == null)
        {
            VoltarPatrulha();
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        if (destinoDefinido &&
            (agent.pathPending ||
             agent.remainingDistance > distanciaDestino))
            return;

        Vector3 direcao = transform.position - player.position;
        direcao.y = 0f;

        if (direcao.sqrMagnitude < 0.01f)
            direcao = -transform.forward;

        Vector3 destino = transform.position +
            direcao.normalized * distanciaFuga;

        if (NavMesh.SamplePosition(
            destino,
            out NavMeshHit hit,
            distanciaEscolhaFuga,
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

        foreach (EnemyNavMesh aliado in aliados)
        {
            if (aliado == this || aliado.player == null)
                continue;

            float distancia = Vector3.Distance(
                transform.position,
                aliado.transform.position
            );

            if (distancia < 10f && aliado.hp > aliado.hpFuga)
                aliado.ChangeState(EnemyState.Pursuit);
        }
    }

    public void TakeDamage(int dano)
    {
        hp = Mathf.Clamp(hp - dano, 0, maxHp);

        Debug.Log(name + " recebeu " + dano +
                  " de dano. HP: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (hp <= hpFuga)
            ChangeState(EnemyState.Flee);
        else if (player != null)
        {
            ChangeState(EnemyState.Pursuit);
            CallBackup();
        }
    }
}
