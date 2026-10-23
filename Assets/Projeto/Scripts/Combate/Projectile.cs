
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int dano = 20;

    private Vector3 direction;
    private ShootPool shootPool;
    private bool retornando = false;

    void OnEnable()
    {
        CancelInvoke(nameof(SpawnTime));
        retornando = false;
        direction = Vector3.zero;

        Invoke(nameof(SpawnTime), 5f);
    }

    public void StartProjectile(Vector3 direction, ShootPool shooter)
    {
        this.direction = direction.normalized;
        this.shootPool = shooter;
        retornando = false;
    }

    void Update()
    {
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
            bool veioDoPlayer =
                shootPool != null && !shootPool.isEnemy;

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
            // Projétil criado diretamente pela skill.
            gameObject.SetActive(false);
        }
    }
}
