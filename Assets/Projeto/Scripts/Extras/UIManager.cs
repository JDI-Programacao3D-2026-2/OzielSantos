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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s = player.GetComponent<ShootPool>();
        k = player.GetComponent<Knockback>();
    }

    // Update is called once per frame
    void Update()
    {
        hp = k.vida;
        hpLabel.text = hp.ToString();

        if (bulletsLabel != null || reloadLabel != null)
        {
            SpoolSize = s.poolSize;
            ScurrentAmmo = s.currentAmmo;

            bulletsLabel.text = "Balas: " + ScurrentAmmo.ToString() + "/" + SpoolSize.ToString();

            if (ScurrentAmmo <= 0)
            {
                reloadLabel.gameObject.SetActive(true);
            }
            else
            {
                reloadLabel.gameObject.SetActive(false);
            }
        }
    }
}
