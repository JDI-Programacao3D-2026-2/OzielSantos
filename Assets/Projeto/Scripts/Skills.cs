
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Skills : MonoBehaviour
{
    public enum Classe
    {
        Atirador,
        Espadachim
    }

    [Header("Classe do personagem")]
    public Classe classe = Classe.Atirador;

    [Header("Interface")]
    public Image[] skill_Image;

    [Header("Cooldowns")]
    public float cd_1 = 8f;
    public float cd_2 = 12f;

    [Header("Referências do Atirador")]
    public Transform pontoDisparo;
    public GameObject projetilEspecialPrefab;
    public ShootPool arma;

    [Header("Skill 1 - Tiro Poderoso")]
    public int danoSkill1 = 60;

    [Header("Skill 2 - Metralhadora")]
    public float duracaoMetralhadora = 5f;
    [Range(0.1f, 1f)]
    public float multiplicadorIntervaloTiro = 0.4f;

    private bool skill1Disponivel = true;
    private bool skill2Disponivel = true;

    void Start()
    {
        if (skill_Image != null)
        {
            foreach (Image imagem in skill_Image)
            {
                if (imagem != null)
                    imagem.fillAmount = 1f;
            }
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        AtualizarCooldowns();

        if (Keyboard.current.digit1Key.wasPressedThisFrame &&
            skill1Disponivel)
        {
            UsarSkill1();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame &&
            skill2Disponivel)
        {
            UsarSkill2();
        }
    }

    void AtualizarCooldowns()
    {
        if (skill_Image == null || skill_Image.Length < 2)
            return;

        if (!skill1Disponivel)
        {
            skill_Image[0].fillAmount +=
                Time.deltaTime / cd_1;

            if (skill_Image[0].fillAmount >= 1f)
            {
                skill_Image[0].fillAmount = 1f;
                skill1Disponivel = true;
            }
        }

        if (!skill2Disponivel)
        {
            skill_Image[1].fillAmount +=
                Time.deltaTime / cd_2;

            if (skill_Image[1].fillAmount >= 1f)
            {
                skill_Image[1].fillAmount = 1f;
                skill2Disponivel = true;
            }
        }
    }

    void UsarSkill1()
    {
        if (classe == Classe.Atirador)
        {
            AtirarProjetilEspecial();
        }
        else if (classe == Classe.Espadachim)
        {
            // Adicione aqui a habilidade do espadachim.
            Debug.Log("Skill 1 do Espadachim!");
        }

        IniciarCooldown(0);
    }

    void UsarSkill2()
    {
        if (classe == Classe.Atirador)
        {
            AtivarMetralhadora();
        }
        else if (classe == Classe.Espadachim)
        {
            // Adicione aqui a habilidade do espadachim.
            Debug.Log("Skill 2 do Espadachim!");
        }

        IniciarCooldown(1);
    }

    void AtirarProjetilEspecial()
{
    if (projetilEspecialPrefab == null || pontoDisparo == null)
    {
        Debug.LogWarning("Configure o prefab especial e o ponto de disparo!");
        return;
    }

    GameObject objeto = Instantiate(
        projetilEspecialPrefab,
        pontoDisparo.position,
        pontoDisparo.rotation
    );

    Projectile projetil = objeto.GetComponent<Projectile>();

    if (projetil != null)
    {
        projetil.dano = danoSkill1;

        // Não coloque a bala forte no pool da arma normal.
        projetil.StartProjectile(pontoDisparo.forward, null);
    }
    else
    {
        Debug.LogWarning(
            "O prefab especial precisa ter o script Projectile!"
        );
    }
}

    void AtivarMetralhadora()
    {
        if (arma == null)
        {
            Debug.LogWarning(
                "Configure a arma no Inspector!"
            );
            return;
        }

        StartCoroutine(MetralhadoraTemporaria());
    }

    IEnumerator MetralhadoraTemporaria()
    {
        // A implementação do aumento da cadência fica na arma.
        arma.AtivarMetralhadora(
            duracaoMetralhadora,
            multiplicadorIntervaloTiro
        );

        yield return null;
    }

    void IniciarCooldown(int indice)
    {
        if (skill_Image == null ||
            skill_Image.Length <= indice ||
            skill_Image[indice] == null)
        {
            Debug.LogWarning(
                "Configure as duas imagens das skills no Inspector!"
            );
            return;
        }

        skill_Image[indice].fillAmount = 0f;

        if (indice == 0)
            skill1Disponivel = false;
        else
            skill2Disponivel = false;
    }
}
