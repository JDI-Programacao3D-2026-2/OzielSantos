using UnityEngine;
using UnityEngine.InputSystem;

public class MouseOrbit : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;

    [Header("Configurações")]
    public float MouseSensitivity = 0.2f;
    public float rotY = 45f;
    public float rotX = 0f;

    [Header("Limites verticais")]
    public float minAngle = 30f;
    public float maxAngle = 60f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (player != null)
        {
            rotX = player.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (player == null || Mouse.current == null)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue()
                        * MouseSensitivity;

        rotX += delta.x;
        rotY = Mathf.Clamp(
            rotY - delta.y,
            minAngle,
            maxAngle
        );

        // A rotação horizontal acompanha o jogador.
        player.rotation = Quaternion.Euler(0f, rotX, 0f);
    }
}
