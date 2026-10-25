using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CorekSayaci : MonoBehaviour
{
    public static CorekSayaci Instance;

    public int toplamCorek = 0;
    public TextMeshProUGUI tmpYazi; 
    public Text normalYazi;         

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        ArayuzuGuncelle();
    }

    public void CorekEkle(int miktar)
    {
        toplamCorek += miktar;
        ArayuzuGuncelle();
    }

    public bool CorekHarca()
    {
        if (toplamCorek > 0)
        {
            toplamCorek--;
            ArayuzuGuncelle();
            return true;
        }
        return false;
    }

   void ArayuzuGuncelle()
{
    if (tmpYazi != null) tmpYazi.text = "Score : " + toplamCorek;
    if (normalYazi != null) normalYazi.text = "Score : " + toplamCorek;
}
}