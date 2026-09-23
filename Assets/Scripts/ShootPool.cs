using UnityEngine; 
using UnityEngine.Pool;
using UnityEngine.InputSystem;

public class ShootPool : MonoBehaviour
{
    public GameObject projectilePrefab; 
    public Transform firePoint; 
    public int poolSize = 20; 
    public int currentAmmo; 
    private int activeProjectiles = 0;
    private ObjectPool<GameObject> pool; 

    void Awake()
    {
        currentAmmo = poolSize;
        // Cria o Pool.
        pool = new ObjectPool<GameObject>(
            () => Instantiate(projectilePrefab), 
            projectile => projectile.SetActive(true), 
            projectile => projectile.SetActive(false), 
            projectile => Destroy(projectile),
            false, 
            poolSize, 
            poolSize 
        );
    }

    void Update()
    {      
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }
    void Shoot()
    {
    if (currentAmmo <= 0 || activeProjectiles >= poolSize)
            return;
        GameObject projectile = pool.Get();
        currentAmmo--;
        activeProjectiles++;
        projectile.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        projectile.GetComponent<Projectile>().StartProjectile(firePoint.forward, this);
    }

    public void ReturnProjectile(GameObject projectile)
    {
        activeProjectiles--;
        pool.Release(projectile);
    }
}











