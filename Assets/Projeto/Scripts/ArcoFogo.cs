
using System.Collections.Generic;
using UnityEngine;

public class ArcoFogo : MonoBehaviour
{
    [Header("Configuração")]
    public float velocidade = 12f;
    public float duracao = 5f;
    public int dano = 40;

    private Vector3 direcao;
    private bool veioDoPlayer = true;
    private Transform jogadorQueAtacou;

    private readonly HashSet<Knockback> alvosAtingidos =
        new HashSet<Knockback>();

    public void Inicializar(
    Vector3 direcaoDisparo,
    int danoAtaque,
    float tempoVida,
    Transform jogador)
{
    direcao = direcaoDisparo.normalized;
    dano = danoAtaque;
    duracao = tempoVida;
    jogadorQueAtacou = jogador;

    Destroy(gameObject, duracao);
}

    void Update()
    {
        transform.position +=
            direcao * velocidade * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
{
    // Impede que o arco acerte o próprio jogador.
    if (jogadorQueAtacou != null &&
        other.transform.root == jogadorQueAtacou.root)
        return;

    Knockback alvo =
        other.GetComponentInParent<Knockback>();

    if (alvo == null || alvosAtingidos.Contains(alvo))
        return;

    alvosAtingidos.Add(alvo);

    alvo.ReceberAtaque(
        dano, direcao, 0f, veioDoPlayer
    );
}
}
