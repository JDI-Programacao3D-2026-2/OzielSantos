
using UnityEngine;

public class BolaEscudoFogo : MonoBehaviour
{
    private int dano;
    private Transform jogador;
    private bool atingiu;

    public void Configurar(int danoAtaque, Transform dono)
    {
        dano = danoAtaque;
        jogador = dono;
    }

    void OnTriggerEnter(Collider other)
    {
        if (atingiu)
            return;

        if (jogador != null &&
            other.transform.root == jogador.root)
            return;

        Knockback alvo =
            other.GetComponentInParent<Knockback>();

        if (alvo == null || alvo.CompareTag("Player"))
            return;

        atingiu = true;

        Vector3 direcao =
            (alvo.transform.position - transform.position).normalized;

        alvo.ReceberAtaque(dano, direcao, 0f, true);

        Destroy(gameObject);
    }
}
