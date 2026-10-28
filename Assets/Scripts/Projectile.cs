
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int dano = 20;

    private Vector3 direction;
    private ShootPool shootPool;
    private bool retornando = false;
    private bool veioDoPlayer = false;

    void OnEnable()
    {
        CancelInvoke(nameof(SpawnTime));

        retornando = false;
        direction = Vector3.zero;
        shootPool = null;
        veioDoPlayer = false;

        Invoke(nameof(SpawnTime), 5f);
    }

    // Compatibilidade com o ShootPool.
    public void StartProjectile(Vector3 direction, ShootPool shooter)
    {
        this.direction = direction.normalized;
        shootPool = shooter;

        veioDoPlayer = shooter != null && !shooter.isEnemy;
        retornando = false;
    }

    // Para projéteis criados diretamente, como os do Mago.
    public void StartProjectile(Vector3 direction, GameObject atacante)
    {
        this.direction = direction.normalized;
        shootPool = null;

        veioDoPlayer = atacante != null &&
                       atacante.CompareTag("Player");

        retornando = false;
    }

    void Update()
    {
        if (retornando)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
{
    if (retornando)
        return;

    retornando = true;
    CancelInvoke(nameof(SpawnTime));

    Knockback alvo = other.GetComponentInParent<Knockback>();

    if (alvo != null)
    {
        EnemyNavMesh inimigo =
            alvo.GetComponent<EnemyNavMesh>();

        if (inimigo == null)
            inimigo = alvo.GetComponentInParent<EnemyNavMesh>();

        if (inimigo != null && veioDoPlayer)
            inimigo.TakeDamage(dano, true);
        else
            alvo.ReceberAtaque(dano, direction, 0f, veioDoPlayer);
    }

    FinalizarProjetil();
}

    void SpawnTime()
    {
        if (retornando)
            return;

        retornando = true;
        FinalizarProjetil();
    }

    void FinalizarProjetil()
    {
        if (shootPool != null)
        {
            shootPool.ReturnProjectile(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
