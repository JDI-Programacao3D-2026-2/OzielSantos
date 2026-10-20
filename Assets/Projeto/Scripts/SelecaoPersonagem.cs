using UnityEngine;
using UnityEngine.SceneManagement;

public class SelecaoPersonagem : MonoBehaviour
{
    [Header("Personagens disponíveis")]
    [SerializeField] private PersonagemData atirador;
    [SerializeField] private PersonagemData espadachim;
    [SerializeField] private PersonagemData bombardeiro;

    [Header("Cena")]
    [SerializeField] private string nomeCenaGameplay = "Gameplay";

    // A escolha existe durante a sessão atual.
    public static PersonagemData PersonagemEscolhido { get; private set; }

    public void EscolherAtirador()
    {
        Escolher(atirador);
    }

    public void EscolherEspadachim()
    {
        Escolher(espadachim);
    }

    public void EscolherBombardeiro()
    {
        Escolher(bombardeiro);
    }

    private void Escolher(PersonagemData personagem)
    {
        if (personagem == null)
        {
            Debug.LogError("O personagem selecionado não foi configurado.");
            return;
        }

        if (personagem.prefab == null)
        {
            Debug.LogError(
                $"O personagem {personagem.nome} não possui um prefab."
            );
            return;
        }

        PersonagemEscolhido = personagem;
        SceneManager.LoadScene(nomeCenaGameplay);
    }
}
