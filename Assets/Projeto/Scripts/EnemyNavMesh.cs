
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMesh : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private int hp = 100;
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int hpFuga = 30;

    public Transform player;
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public LayerMask layerMask;

    [Header("Distâncias")]
    public float losePlayerDistance = 15f;
    public float distanciaFuga = 10f;
    public float distanciaEscolhaFuga = 8f;

    public enum EnemyState
    {
        WayPatrol,
        RandomPatrol,
        Pursuit,
        Flee
    }

    public EnemyState currentState = EnemyState.WayPatrol;

    void Update()
    {
        if (hp <= hpFuga && player != null)
        {
            ChangeState(EnemyState.Flee);
        }

        FiniteStateMachine();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    void FiniteStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Pursuit:
                Pursuit();
                break;

            case EnemyState.WayPatrol:
                WayPatrol();
                break;

            case EnemyState.RandomPatrol:
                WayPatrol();
                break;

            case EnemyState.Flee:
                Flee();
                break;
        }
    }

    void Pursuit()
    {
        if(player != null){
             agent.stoppingDistance = 6f;
             agent.SetDestination(player.position);
             transform.LookAt(player);
             } 
             
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > losePlayerDistance) 
        { 
            ChangeState(EnemyState.WayPatrol);
        }
    }

    void Flee()
    {
        if (player == null)
            return;

        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        Vector3 direction = transform.position - player.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            direction = -transform.forward;

        direction.Normalize();

        Vector3 destino = transform.position
                        + direction * distanciaFuga;

        if (NavMesh.SamplePosition(
            destino,
            out NavMeshHit hit,
            distanciaEscolhaFuga,
            NavMesh.AllAreas))
        {
            if (!agent.hasPath ||
                Vector3.Distance(agent.destination, hit.position) > 2f)
            {
                agent.SetDestination(hit.position);
            }
        }

        /* Olha para o jogador enquanto recua.
        Vector3 olhar = player.position - transform.position;
        olhar.y = 0f;

        if (olhar.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(olhar);
        }
        */

        // Olha para a direção em que está fugindo.
        Vector3 olhar = transform.position - player.position;
        olhar.y = 0f;

        if (olhar.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(olhar);
        }
    }

    void WayPatrol()
    {
        agent.stoppingDistance = 0f;

        if (waypoints != null &&
            waypoints.Length > 0 &&
            !agent.pathPending &&
            agent.remainingDistance < 0.5f)
        {
            int randomIndex = Random.Range(0, waypoints.Length);
            agent.SetDestination(waypoints[randomIndex].position);
        }

        SearchPlayer();
    }

    void SearchPlayer()
    {
        if (player == null)
            return;

        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out RaycastHit hit,
            20f,
            layerMask))
        {
            if (hit.transform == player ||
                hit.transform.IsChildOf(player))
            {
                if (hp <= hpFuga)
                    ChangeState(EnemyState.Flee);
                else
                    ChangeState(EnemyState.Pursuit);

                CallBackup();
            }
        }
    }

    void CallBackup()
    {
        EnemyNavMesh[] todosInimigos = FindObjectsByType<EnemyNavMesh>();

        foreach (EnemyNavMesh aliado in todosInimigos)
        {
            float distancia = Vector3.Distance(
                transform.position,
                aliado.transform.position);

            if (distancia < 10f &&
                aliado.hp > aliado.hpFuga)
            {
                aliado.ChangeState(EnemyState.Pursuit);
            }
        }
    }

    public void TakeDamage(int dano)
    {
        hp -= dano;
        hp = Mathf.Clamp(hp, 0, maxHp);

        Debug.Log(name + " recebeu " + dano +
                  " de dano. HP: " + hp);

        if (hp <= 0)
        {
            Die();
            return;
        }

        if (hp <= hpFuga && player != null)
        {
            ChangeState(EnemyState.Flee);
        }
    }

    void Die()
    {
        Debug.Log(name + " foi derrotado!");

        Destroy(gameObject);
    }
}