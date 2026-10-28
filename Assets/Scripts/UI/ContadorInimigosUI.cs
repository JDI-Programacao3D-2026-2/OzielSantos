
using UnityEngine;
using TMPro;

public class ContadorInimigosUI : MonoBehaviour
{
    [Header("Referências")]
    private EnemySpawner enemySpawner;
    public TextMeshProUGUI textoContador;

    private float proximaBusca;
    
    void Start()
    {
        if (textoContador == null)
            textoContador = GetComponent<TextMeshProUGUI>();

        EncontrarEnemySpawner();
    }

    void Update()
    {
        // Procura novamente caso o spawner ainda não tenha sido criado.
        if (enemySpawner == null && Time.time >= proximaBusca)
        {
            proximaBusca = Time.time + 1f;
            EncontrarEnemySpawner();
        }

        if (enemySpawner == null || textoContador == null)
            return;

        int restantes = enemySpawner.InimigosRestantes;

        textoContador.text = $"Inimigos restantes:\n<size=40>{restantes}</size>";
    }

    void EncontrarEnemySpawner()
    {
        enemySpawner = FindAnyObjectByType<EnemySpawner>();

        if (enemySpawner != null)
        {
            Debug.Log("Contador encontrou o EnemySpawner!");
        }
        else
        {
            Debug.LogWarning("Contador ainda não encontrou o EnemySpawner.");
        }
    }
}
