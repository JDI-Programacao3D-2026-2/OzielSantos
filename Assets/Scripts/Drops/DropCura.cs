
using UnityEngine;

public class DropCura : MonoBehaviour
{
    public int quantidadeCura = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Knockback vida = other.GetComponentInParent<Knockback>();

        if (vida == null)
            return;

        if (vida.vida <= 0)
            return;

        vida.vida = Mathf.Min(
            vida.vida + quantidadeCura,
            vida.vidaMaxima
        );

        Destroy(gameObject);
    }
}
