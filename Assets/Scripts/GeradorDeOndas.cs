using UnityEngine;

public class GeradorDeOndas : MonoBehaviour
{
    string[] inimigos = {"Serpente", "Aranha", "Esqueleto"};
    int vidaInimigo = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 5; i < 6; i++)
        {
            Debug.Log($"Inimigo {inimigos[Random.Range(0, inimigos.Length - 1)]} criado em (Local, {i}, 0)");
        }

        while (vidaInimigo > 0)
        {
            vidaInimigo -= 10;
            Debug.Log("O inimigo levou 10 de dano!");
        }
        Debug.Log("O inimigo morreu!");

        foreach (string item in inimigos)
        {
            Debug.Log($"Inimigo encontrado: {item}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
