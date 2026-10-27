
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SelecaoPersonagem : MonoBehaviour
{
    [Header("Personagens disponíveis")]
    [SerializeField] private PersonagemData atirador;
    [SerializeField] private PersonagemData espadachim;
    [SerializeField] private PersonagemData mago;

    [Header("Painéis do menu")]
    [SerializeField] private GameObject painelModos;
    [SerializeField] private GameObject painelPersonagens;

    [Header("Cenas")]
    [SerializeField] private string nomeCenaGameplay = "Gameplay";
    [SerializeField] private string nomeCenaTutorial = "Tutorial";

    [Header("Texto do modo escolhido")]
    [SerializeField] private TextMeshProUGUI textoModo;

    private enum ModoJogo
    {
        Nenhum,
        Tutorial,
        Gameplay
    }

    private ModoJogo modoEscolhido = ModoJogo.Nenhum;

    public static PersonagemData PersonagemEscolhido { get; private set; }

    void Start()
    {
        // Começa mostrando a escolha do modo.
        MostrarPainelModos();
    }

    // BOTÕES DO PRIMEIRO PAINEL

        public void EscolherModoTutorial()
    {
        modoEscolhido = ModoJogo.Tutorial;

        if (textoModo != null)
            textoModo.text = "Modo Tutorial";

        MostrarSelecaoPersonagem();
    }

    public void EscolherModoGameplay()
    {
        modoEscolhido = ModoJogo.Gameplay;

        if (textoModo != null)
            textoModo.text = "Modo Gameplay Normal";

        MostrarSelecaoPersonagem();
    }

    // BOTÕES DE ESCOLHA DOS PERSONAGENS

    public void EscolherAtirador()
    {
        Escolher(atirador);
    }

    public void EscolherEspadachim()
    {
        Escolher(espadachim);
    }

    public void EscolherMago()
    {
        Escolher(mago);
    }

    private void Escolher(PersonagemData personagem)
    {
        if (modoEscolhido == ModoJogo.Nenhum)
        {
            Debug.LogWarning("Escolha primeiro o modo de jogo.");
            return;
        }

        if (personagem == null || personagem.prefab == null)
        {
            Debug.LogError(
                "O personagem selecionado ou seu prefab não foi configurado."
            );
            return;
        }

        PersonagemEscolhido = personagem;

        string cenaDestino = modoEscolhido == ModoJogo.Tutorial
            ? nomeCenaTutorial
            : nomeCenaGameplay;

        SceneManager.LoadScene(cenaDestino);
    }

    // BOTÕES DE VOLTAR

    public void VoltarParaModos()
    {
        modoEscolhido = ModoJogo.Nenhum;
        MostrarPainelModos();
    }

    private void MostrarPainelModos()
    {
        if (painelModos != null)
            painelModos.SetActive(true);

        if (painelPersonagens != null)
            painelPersonagens.SetActive(false);
    }

    private void MostrarSelecaoPersonagem()
    {
        if (painelModos != null)
            painelModos.SetActive(false);

        if (painelPersonagens != null)
            painelPersonagens.SetActive(true);
    }

    public void FecharJogo()
    {
        Application.Quit();
    }
}
