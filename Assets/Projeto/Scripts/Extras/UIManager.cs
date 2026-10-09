using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Transform player;
    private ShootPool s;
    public TextMeshProUGUI label;
    public TextMeshProUGUI reloadLabel;
    private int SpoolSize; 
    private int ScurrentAmmo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s = player.GetComponent<ShootPool>();
    }

    // Update is called once per frame
    void Update()
    {
        SpoolSize = s.poolSize;
        ScurrentAmmo = s.currentAmmo;
        label.text = "Balas: " + ScurrentAmmo.ToString() + "/" + SpoolSize.ToString();

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
