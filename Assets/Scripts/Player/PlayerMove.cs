
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidadeNormal = 5f;
    private float speed;
    private Vector3 normalParede;

    public Rigidbody rb;
    private Vector3 direction;

    [Header("Pulo")]
    public float forcaPulo = 7f;
    public Transform groundCheck;
    public float raioChao = 0.2f;
    public LayerMask camadaChao;

    private bool estaNoChao;
    private bool podePular = true;
    private bool estavaNoChao;

    [Header("Dash")]
    public Key teclaDash = Key.LeftShift;
    public float velocidadeDash = 15f;
    public float duracaoDash = 0.2f;
    public float cooldownDash = 1f;

    private bool dashAtivo;
    private bool dashDisponivel = true;
    private bool corridaAtiva;

    private Coroutine dashCoroutine;
    private Coroutine corridaCoroutine;

    private GameManager gameManager;


    void VerificarChao()
{
    if (groundCheck == null || rb == null)
    {
        estaNoChao = false;
        return;
    }

    estaNoChao = Physics.CheckSphere(
        groundCheck.position,
        raioChao,
        camadaChao,
        QueryTriggerInteraction.Ignore
    );

    // Se estiver subindo ou descendo, não está aterrissado.
    if (rb.linearVelocity.y > 0.1f ||
        rb.linearVelocity.y < -0.1f)
    {
        estaNoChao = false;
    }

    // Só libera outro pulo quando tocar no chão novamente.
    if (estaNoChao)
    {
        podePular = true;
    }
}
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        speed = velocidadeNormal;
    }

    void Update()
    {
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.Euler(
                0,
                Camera.main.transform.eulerAngles.y,
                0
            );
        }

        direction = Vector3.zero;

        if (Time.timeScale == 0f)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed)
            direction += transform.forward;

        if (Keyboard.current.sKey.isPressed)
            direction -= transform.forward;

        if (Keyboard.current.dKey.isPressed)
            direction += transform.right;

        if (Keyboard.current.aKey.isPressed)
            direction -= transform.right;

        direction = Vector3.ClampMagnitude(direction, 1f);

        VerificarChao();

        if (Keyboard.current.spaceKey.wasPressedThisFrame
            && podePular
            && estaNoChao
            && !dashAtivo
            && rb != null)
        {
            podePular = false;

            rb.AddForce(
                Vector3.up * forcaPulo,
                ForceMode.Impulse
            );

            if (gameManager != null)
            {
                gameManager.TutorialPular();
            }

            Debug.Log("Pulo executado!");
        }

        // Ativa o dash somente se não estiver usando a corrida.
        if (Keyboard.current.spaceKey.wasPressedThisFrame
            && estaNoChao
            && !dashAtivo
            && rb != null)
        {
            rb.AddForce(
                Vector3.up * forcaPulo,
                ForceMode.Impulse
            );
        }
            
    if (Keyboard.current[teclaDash].wasPressedThisFrame
        && dashDisponivel
        && !dashAtivo
        && !corridaAtiva)
    {
        dashCoroutine = StartCoroutine(ExecutarDash());
        if (gameManager != null)
        {
            gameManager.TutorialDash();
        }
    }

    }

    void OnCollisionStay(Collision collision)
{
    foreach (ContactPoint contato in collision.contacts)
    {
        if (Vector3.Dot(contato.normal, Vector3.up) < 0.3f)
        {
            normalParede = contato.normal;
            return;
        }
    }

    normalParede = Vector3.zero;
}

void OnCollisionExit(Collision collision)
{
    normalParede = Vector3.zero;
}

    
void FixedUpdate()
{
    if (rb == null)
        return;

    // Não interfere na velocidade durante o dash.
    if (dashAtivo)
        return;

    Vector3 movimento = direction * speed;
    movimento.y = 0f;

    // Se estiver encostado em uma parede, tenta deslizar
    // na direção paralela à superfície.
    if (Physics.SphereCast(
        transform.position + Vector3.up * 0.5f,
        0.3f,
        movimento.normalized,
        out RaycastHit hit,
        0.6f,
        ~0,
        QueryTriggerInteraction.Ignore))
    {
        if (hit.collider.transform != transform &&
            !hit.collider.transform.IsChildOf(transform))
        {
            movimento = Vector3.ProjectOnPlane(
                movimento,
                hit.normal
            );
        }
    }

    rb.linearVelocity = new Vector3(
        movimento.x,
        rb.linearVelocity.y,
        movimento.z
    );
}


    private IEnumerator ExecutarDash()
    {
        dashAtivo = true;
        dashDisponivel = false;

        Vector3 direcaoDash = direction;

        if (direcaoDash.sqrMagnitude < 0.01f)
            direcaoDash = transform.forward;

        direcaoDash.y = 0f;
        direcaoDash.Normalize();

        float tempo = 0f;

        while (tempo < duracaoDash)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(
                    direcaoDash.x * velocidadeDash,
                    rb.linearVelocity.y,
                    direcaoDash.z * velocidadeDash
                );
            }

            tempo += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        dashAtivo = false;
        dashCoroutine = null;

        yield return new WaitForSeconds(cooldownDash);

        dashDisponivel = true;
    }

    public void AtivarCorrida(float duracao, float multiplicador)
    {
        // Impede a corrida de iniciar durante um dash.
        if (dashAtivo)
        {
            Debug.Log("Não é possível ativar a corrida durante o dash!");
            return;
        }

        if (corridaCoroutine != null)
            StopCoroutine(corridaCoroutine);

        corridaCoroutine = StartCoroutine(
            CorridaTemporaria(duracao, multiplicador)
        );
    }

    private IEnumerator CorridaTemporaria(
        float duracao,
        float multiplicador)
    {
        corridaAtiva = true;
        speed = velocidadeNormal * multiplicador;

        Debug.Log("Espadachim ativou a corrida!");

        yield return new WaitForSeconds(duracao);

        speed = velocidadeNormal;
        corridaAtiva = false;
        corridaCoroutine = null;

        Debug.Log("Velocidade normal restaurada!");
    }
}
