using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 direction;
    private ShootPool Shootpool;

    public void StartProjectile(Vector3 direction, ShootPool shooter)
    {
        this.direction = direction;
        this.Shootpool = shooter;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        Shootpool.ReturnProjectile(gameObject);
    }
}
