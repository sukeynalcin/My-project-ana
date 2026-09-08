
using UnityEngine;

public class SonsuzParalax : MonoBehaviour
{
    private float length, startpos;
    public GameObject cam;
    
    [Header("Paralaks Ayarı")]
    [Tooltip("1 = Kamera ile tamamen aynı hızda hareket eder (En uzak arka plan)\n0 = Hiç hareket etmez")]
    [Range(0f, 1f)]
    public float parallaxEffect;

    void Start()
    {
        // Resmin başlangıç konumunu ve genişliğini kaydediyoruz
        startpos = transform.position.x;
        
        if (GetComponent<SpriteRenderer>() != null)
        {
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        // Eğer kamerayı elle atmadıysan otomatik Main Camera'yı bulur
        if (cam == null)
        {
            cam = Camera.main.gameObject;
        }
    }

    void LateUpdate()
    {
        // Kameranın hareketine göre hızı ve mesafeyi hesapla
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        // Objenin yeni X konumunu güncelle
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // Kamera resmi geçtiğinde, resmi ileri/geri kaydırarak sonsuz döngü sağla
        if (temp > startpos + length)
        {
            startpos += length;
        }
        else if (temp < startpos - length)
        {
            startpos -= length;
        }
    }
}