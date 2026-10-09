
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int dano = 20;

    private Vector3 direction;
    private ShootPool Shootpool;

    void OnEnable()
    {
        CancelInvoke(nameof(SpawnTime));
        Invoke(nameof(SpawnTime), 5f);
    }

    public void StartProjectile(Vector3 direction, ShootPool shooter)
    {
        this.direction = direction;
        this.Shootpool = shooter;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        CancelInvoke(nameof(SpawnTime));

        EnemyNavMesh inimigo =
            collision.collider.GetComponentInParent<EnemyNavMesh>();

        if (inimigo != null)
        {
            inimigo.TakeDamage(dano);
        }

        if (Shootpool != null)
        {
            Shootpool.ReturnProjectile(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void SpawnTime()
    {
        if (Shootpool != null)
        {
            Shootpool.ReturnProjectile(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}