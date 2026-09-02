using UnityEngine;

public class PersonagemRPG : MonoBehaviour
{
    string nome;
    int vida;
    float velocidade;
    int nivel;
    bool estaVivo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        const int VIDA_MAXIMA = 100;
        const float GRAVIDADE = 10;

        nome = "Oziel";
        vida = 50;
        velocidade = 5.0f;
        nivel = 10;
        estaVivo = true;

        Debug.Log($"=== Ficha do Personagem === Nome: {nome} | Nível: {nivel} Vida: {vida}/{VIDA_MAXIMA} | Velocidade: {velocidade} Status: Vivo");
    }
}
