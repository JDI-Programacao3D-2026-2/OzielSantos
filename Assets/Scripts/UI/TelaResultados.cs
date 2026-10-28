
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TelaResultados : MonoBehaviour
{
    [Header("Textos")]
    public TextMeshProUGUI textoKills;
    public TextMeshProUGUI textoMortes;
    public TextMeshProUGUI textoPontuacao;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (textoPontuacao != null)
        {
            textoPontuacao.text =
                $"Pontuação Final:\n<size=100>{EstatisticasPartida.pontuacao}</size>";
        }

        if (textoKills != null)
            textoKills.text = $"Kills:\n<size=75>{EstatisticasPartida.kills}</size>";

        if (textoMortes != null)
            textoMortes.text = $"Mortes:\n<size=75>{EstatisticasPartida.mortes}</size>";
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
