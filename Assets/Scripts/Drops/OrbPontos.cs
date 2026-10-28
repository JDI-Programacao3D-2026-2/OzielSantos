
using UnityEngine;

public class OrbPontos : MonoBehaviour
{
    public int pontos = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameManager gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogWarning("Orb: GameManager não encontrado!");
            return;
        }

        gm.AdicionarPontos(pontos);
        Destroy(gameObject);
    }
}
