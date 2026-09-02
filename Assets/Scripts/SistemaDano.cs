using UnityEngine;

public class SistemaDano : MonoBehaviour
{
    string tipoAtaque = "Fogo";
    int danoBase = 20;
    float multiplicador;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (tipoAtaque)
        {
            case "Fogo":
            multiplicador = 2.0f;
            Debug.Log($"Dano final: {danoBase * multiplicador}");
            break;

            case "Gelo":
            multiplicador = 1.5f;
            Debug.Log($"Dano final: {danoBase * multiplicador}");
            break;

            case "Raio":
            multiplicador = 3.0f;
            Debug.Log($"Dano final: {danoBase * multiplicador}");
            break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
