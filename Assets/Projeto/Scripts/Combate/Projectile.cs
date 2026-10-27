
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

    // Mantém compatibilidade com o ShootPool existente.
    public void StartProjectile(Vector3 direction, ShootPool shooter)
    {
        this.direction = direction.normalized;
        shootPool = shooter;

        veioDoPlayer = shooter != null && !shooter.isEnemy;

        retornando = false;
    }

    // Novo método para ataques criados diretamente, como o do Mago.
    public void StartProjectile(Vector3 direction, GameObject atacante)
    {
        this.direction = direction.normalized;
        shootPool = null;

        if (atacante != null)
        {
            veioDoPlayer = atacante.CompareTag("Player");
        }
        else
        {
            veioDoPlayer = false;
        }

        retornando = false;
    }

    void Update()
    {
        if (retornando)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (retornando)
            return;

        retornando = true;
        CancelInvoke(nameof(SpawnTime));

        Knockback alvo =
            collision.collider.GetComponentInParent<Knockback>();

        if (alvo != null)
        {
            alvo.ReceberAtaque(
                dano,
                direction,
                0f,
                veioDoPlayer
            );
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
