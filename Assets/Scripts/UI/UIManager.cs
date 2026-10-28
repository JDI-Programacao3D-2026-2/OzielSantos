
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Transform player;

    private ShootPool s;
    private Knockback k;

    public TextMeshProUGUI bulletsLabel;
    public TextMeshProUGUI hpLabel;
    public TextMeshProUGUI reloadLabel;

    private int SpoolSize;
    private int ScurrentAmmo;
    private int hp;

    private bool recarregando = false;

    void Start()
    {
        s = player.GetComponent<ShootPool>();
        k = player.GetComponent<Knockback>();

        if (reloadLabel != null)
            reloadLabel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (k != null && hpLabel != null)
        {
            hp = k.vida;
            hpLabel.text = hp.ToString();
        }

        if (s == null)
            return;

        SpoolSize = s.poolSize;
        ScurrentAmmo = s.currentAmmo;

        if (bulletsLabel != null)
            bulletsLabel.text = ScurrentAmmo + "/" + SpoolSize;

        if (reloadLabel != null && !recarregando)
        {
            // Mostra aviso quando a munição acaba.
            reloadLabel.gameObject.SetActive(ScurrentAmmo <= 0);

            if (ScurrentAmmo <= 0)
                reloadLabel.text = "Sem munição!";
        }
    }

    public void MostrarRecarregando()
    {
        recarregando = true;

        if (reloadLabel != null)
        {
            reloadLabel.text = "Recarregando...";
            reloadLabel.gameObject.SetActive(true);
        }
    }

    public void FinalizarRecarga()
    {
        recarregando = false;

        if (reloadLabel != null)
            reloadLabel.gameObject.SetActive(false);
    }
}
