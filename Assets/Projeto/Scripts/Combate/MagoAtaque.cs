
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AtaqueMago : MonoBehaviour
{
    [Header("Configuração do ataque")]
    public GameObject bolaDeFogoPrefab;
    public Transform pontoDisparo;

    [Header("UI do cooldown")]
    public Image imagemCooldown;

    public float cooldown = 2f;
    public int dano = 25;

    private float proximoDisparo;

    void Update()
    {
        // Atualiza o indicador visual do cooldown.
            // Atualiza o indicador visual do cooldown.
            if (imagemCooldown != null)
            {
                float restante = proximoDisparo - Time.time;

                imagemCooldown.fillAmount = cooldown > 0
                    ? 1f - Mathf.Clamp01(restante / cooldown)
                    : 1f;
            }

        // Clique esquerdo para atacar.
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= proximoDisparo)
        {
            Atirar();
        }
    }

    void Atirar()
    {
        if (bolaDeFogoPrefab == null || pontoDisparo == null)
        {
            Debug.LogWarning(
                "Configure a bola de fogo e o ponto de disparo!"
            );
            return;
        }

        GameObject objeto = Instantiate(
            bolaDeFogoPrefab,
            pontoDisparo.position,
            pontoDisparo.rotation
        );

        Projectile projetil = objeto.GetComponent<Projectile>();

        if (projetil != null)
        {
            projetil.dano = dano;
            projetil.StartProjectile(
                pontoDisparo.forward,
                null
            );
        }
        else
        {
            Debug.LogWarning(
                "O prefab precisa ter o componente Projectile!"
            );

            Destroy(objeto);
            return;
        }

        proximoDisparo = Time.time + cooldown;
    }
}
