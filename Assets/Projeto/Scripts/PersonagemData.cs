using UnityEngine;

[CreateAssetMenu(
    fileName = "NovoPersonagem",
    menuName = "Personagens/Personagem"
)]
public class PersonagemData : ScriptableObject
{
    [Header("Informações")]
    public string id;
    public string nome;
    [TextArea] public string descricao;
    public Sprite retrato;

    [Header("Gameplay")]
    public GameObject prefab;
}
