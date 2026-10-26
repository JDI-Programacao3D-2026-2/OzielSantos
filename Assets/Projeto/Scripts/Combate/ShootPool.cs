using UnityEngine; 
using UnityEngine.Pool;
using UnityEngine.InputSystem;
using System.Collections;

public class ShootPool : MonoBehaviour
{
    public GameObject projectilePrefab; 
    public Transform firePoint; 
    public int poolSize = 20; 
    public int currentAmmo; 
    private int activeProjectiles = 0;
    private ObjectPool<GameObject> pool; 
    public bool isEnemy = false;
    private bool canShoot = false;
    private float fireRate = 0.5f;
    private float nextFireTime = 0f;
    public LayerMask layerMask;
    public Transform player;

    private float intervaloOriginal;
    private Coroutine metralhadoraCoroutine;
    private bool municaoInfinita = false;

    [Header("Mira")]
    public float distanciaMira = 100f;
    public LayerMask camadaMira = ~0;

    [Header("Rotação da arma")]
    public Transform armaVisual;
    public float velocidadeRotacaoArma = 15f;

    

private Vector3 CalcularDirecaoTiro()
{
    // INIMIGOS: mira diretamente no jogador.
    if (isEnemy)
    {
        if (player == null)
        {
            GameObject objetoJogador =
                GameObject.FindGameObjectWithTag("Player");

            if (objetoJogador != null)
                player = objetoJogador.transform;
        }

        if (player == null || firePoint == null)
            return Vector3.zero;

        Vector3 alvo = player.position + Vector3.up;

        return (alvo - firePoint.position).normalized;
    }

    // JOGADOR: mira pelo centro da câmera.
    Camera cam = Camera.main;

    if (cam == null || firePoint == null)
        return Vector3.zero;

    Ray raio = cam.ViewportPointToRay(
        new Vector3(0.5f, 0.5f, 0f)
    );

    Vector3 pontoMiraCamera;

    if (Physics.Raycast(
        raio,
        out RaycastHit hit,
        distanciaMira,
        camadaMira,
        QueryTriggerInteraction.Ignore))
    {
        pontoMiraCamera = hit.point;
    }
    else
    {
        pontoMiraCamera =
            raio.origin + raio.direction * distanciaMira;
    }

    return (pontoMiraCamera - firePoint.position).normalized;
}

    void AtualizarRotacaoArma()
{
    if (armaVisual == null || firePoint == null)
        return;

    Vector3 direcao = CalcularDirecaoTiro();

    if (direcao.sqrMagnitude < 0.001f)
        return;

    Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);

    armaVisual.rotation = Quaternion.Slerp(
        armaVisual.rotation,
        rotacaoAlvo,
        velocidadeRotacaoArma * Time.deltaTime
    );
}

    void Awake()
    {
        intervaloOriginal = fireRate;
        currentAmmo = poolSize;
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
        if (isEnemy && canShoot)
        {
            // O EnemyNavMesh controla quando o inimigo pode atirar.
            // Não dispare automaticamente aqui.
        }

        if (isEnemy && currentAmmo <= 0 && !IsInvoking("Reload"))
        {
            Invoke("Reload", 2f);
        }

        if (!isEnemy &&
            Mouse.current != null &&
            Mouse.current.leftButton.isPressed &&
            Time.time >= nextFireTime)
        {
            Shoot();
}
        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame &&
            !isEnemy &&
            !IsInvoking(nameof(Reload)))
        {
            Invoke(nameof(Reload), 2f);
        }

        if (!isEnemy)
        AtualizarRotacaoArma();
    }
void FixedUpdate()
{
    canShoot = false;
    if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 7f, layerMask))
    {           
        canShoot = true;
         Debug.DrawLine(transform.position, hit.point, Color.green);
    }else
    {
      Debug.DrawLine(transform.position, transform.position + transform.forward * 100f, Color.red);
}
}

public void Shoot()
{
    if ((!municaoInfinita && currentAmmo <= 0) ||
        activeProjectiles >= poolSize ||
        firePoint == null)
        return;

    Vector3 direcao;

    if (isEnemy)
{
    GameObject objetoJogador = GameObject.FindGameObjectWithTag("Player");

    if (objetoJogador == null)
    {
        Debug.LogWarning($"{name}: não encontrou o Player!");
        return;
    }

    // Atualiza a referência para o jogador atual.
    player = objetoJogador.transform;

    // Mira na posição atual do jogador, aproximadamente no peito.
    Vector3 alvo = player.position + Vector3.up * 0.5f;

    direcao = (alvo - firePoint.position).normalized;

    Debug.DrawLine(firePoint.position, alvo, Color.magenta, 3f);

    Debug.Log($"{name} atirando. Posição atual do jogador: {player.position}");
}
else
{
    direcao = CalcularDirecaoTiro();
}

    if (direcao.sqrMagnitude < 0.001f)
        return;

    Debug.DrawRay(
        firePoint.position,
        direcao * 15f,
        Color.blue,
        3f
    );

    GameObject projectile = pool.Get();

    if (!municaoInfinita)
        currentAmmo--;

    activeProjectiles++;

    projectile.transform.SetPositionAndRotation(
        firePoint.position,
        Quaternion.LookRotation(direcao)
    );

    Projectile script = projectile.GetComponent<Projectile>();

    if (script != null)
    {
        script.StartProjectile(direcao, this);
        nextFireTime = Time.time + fireRate;
    }
    else
    {
        activeProjectiles--;
        pool.Release(projectile);
    }
}

    public void ReturnProjectile(GameObject projectile)
    {
        
        activeProjectiles--;
        pool.Release(projectile);        
    }

    private void Reload()
    {
        currentAmmo = poolSize;
    }

    public void AtivarMetralhadora(
    float duracao,
    float multiplicadorIntervalo)
{
    if (metralhadoraCoroutine != null)
        StopCoroutine(metralhadoraCoroutine);

    metralhadoraCoroutine =
        StartCoroutine(MetralhadoraTemporaria(
            duracao,
            multiplicadorIntervalo
        ));
}

private IEnumerator MetralhadoraTemporaria(
    float duracao,
    float multiplicadorIntervalo)
{
    municaoInfinita = true;

    fireRate = intervaloOriginal *
        Mathf.Clamp(multiplicadorIntervalo, 0.1f, 1f);

    Debug.Log("Metralhadora ativada: munição infinita!");

    yield return new WaitForSeconds(duracao);

    municaoInfinita = false;
    fireRate = intervaloOriginal;
    metralhadoraCoroutine = null;

    Debug.Log("Metralhadora desativada!");
}
}











