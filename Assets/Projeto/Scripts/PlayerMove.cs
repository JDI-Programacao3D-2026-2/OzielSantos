
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidadeNormal = 5f;
    private float speed;

    public Rigidbody rb;
    private Vector3 direction;

    private Coroutine corridaCoroutine;

    void Start()
    {
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
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector3(
            direction.x * speed,
            rb.linearVelocity.y,
            direction.z * speed
        );
    }

    public void AtivarCorrida(float duracao, float multiplicador)
    {
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
        speed = velocidadeNormal * multiplicador;

        Debug.Log("Espadachim ativou a corrida!");

        yield return new WaitForSeconds(duracao);

        speed = velocidadeNormal;
        corridaCoroutine = null;

        Debug.Log("Velocidade normal restaurada!");
    }
}
