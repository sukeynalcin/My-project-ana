using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KarakterCani : MonoBehaviour
{
    [Header("Can Değerleri")]
    public int maxCan = 3;
    public int mevcutCan;

    [Header("Hasar Efekti")]
    public float dokunulmazlikSuresi = 1f; // Hasar aldıktan sonra koruma süresi
    private bool hasarAlabilirMi = true;

    private SpriteRenderer sr;
    private Color orjinalRenk;

    void Start()
    {
        mevcutCan = maxCan;

        // SpriteRenderer karakterin kendisinde yoksa alt objesinde arar
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        if (sr != null)
        {
            orjinalRenk = sr.color;
        }
    }

    public void HasarAl(int miktar)
    {
        // Karakter koruma anındaysa tekrar hasar almaz
        if (!hasarAlabilirMi) return;

        mevcutCan -= miktar;
        Debug.Log("Hasar alındı! Kalan Can: " + mevcutCan);

        // 3 canın tamamı bittiğinde sahne en baştan yüklenir
        if (mevcutCan <= 0)
        {
            Debug.Log("3 Can bitti, bölüm baştan başlıyor!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            StartCoroutine(HasarEfekti());
        }
    }

    IEnumerator HasarEfekti()
    {
        hasarAlabilirMi = false;

        // 1. Kıpkırmızı flaş
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(0.12f);

        // 2. Yarı saydam yanıp sönme
        float sayac = 0f;
        while (sayac < dokunulmazlikSuresi)
        {
            if (sr != null) sr.color = new Color(orjinalRenk.r, orjinalRenk.g, orjinalRenk.b, 0.25f);
            yield return new WaitForSeconds(0.1f);

            if (sr != null) sr.color = orjinalRenk;
            yield return new WaitForSeconds(0.1f);

            sayac += 0.2f;
        }

        if (sr != null) sr.color = orjinalRenk;
        hasarAlabilirMi = true;
    }
}