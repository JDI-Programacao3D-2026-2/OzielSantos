using UnityEngine;

public class CalculadoraDano : MonoBehaviour
{
    float vida = 100;
    int ataque = 25;
    int defesa = 10;
    float multiplicador = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float danoReal = (ataque - defesa) * multiplicador;

        vida -= danoReal;
        Debug.Log("=== Turno 1 ===");
        Debug.Log($"Dano real : {danoReal} | Dano crítico! Vida restante: {vida}");

        vida -= danoReal;
        Debug.Log("=== Turno 2 ===");
        Debug.Log($"Dano real : {danoReal} | Dano crítico! Vida restante: {vida}");

        vida -= danoReal;
        Debug.Log("=== Turno 3 ===");
        Debug.Log($"Dano real : {danoReal} | Dano crítico! Vida restante: {vida}");  
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
