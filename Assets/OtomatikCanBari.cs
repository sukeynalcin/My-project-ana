using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OtomatikCanBari : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maksimumCan = 100f;
    public float mevcutCan;

    [Header("Darbe ve Savrulma")]
    public float dokunulmazlikSuresi = 1f;
    public float geriyeSavrulmaGucu = 3.5f;

    [Header("Karakter Altı Bar Ayarları")]
    public Vector3 barOfseti = new Vector3(0f, -0.65f, 0f); // Karakterin ne kadar altında duracağı
    public Vector2 barBoyutu = new Vector2(40f, 6f);        // Piksel boyutu
    public float gorunmeSuresi = 2.5f;                      // Hasar aldıktan sonra kaç sn açık kalsın?
    public Color barRengi = new Color(0.9f, 0.2f, 0.2f);    // Kırmızı dolgu rengi

    private GameObject barKutusu;
    private Image dolguGorseli;
    private CanvasGroup canvasGroup;
    private float kaybolmaZamanlayici = 0f;
    private bool dokunulmazMi = false;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Awake()
    {
        mevcutCan = maksimumCan;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        KarakterAltiBariKur();
    }

    void LateUpdate()
    {
        if (barKutusu == null) return;

        // Barı karakterin tam altına sabitle (Kız sağa/sola dönünce barın ters dönmesini engeller)
        barKutusu.transform.position = transform.position + barOfseti;
        barKutusu.transform.rotation = Quaternion.identity;

        // Can dolgusunu pürüzsüzce erit
        if (dolguGorseli != null)
        {
            float hedefDoluluk = mevcutCan / maksimumCan;
            dolguGorseli.fillAmount = Mathf.Lerp(dolguGorseli.fillAmount, hedefDoluluk, Time.deltaTime * 8f);
        }

        // Zamanlayıcı: Hasar alınca görünür, süre bitince yavaşça kaybolur
        if (kaybolmaZamanlayici > 0)
        {
            kaybolmaZamanlayici -= Time.deltaTime;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, Time.deltaTime * 6f);
        }
        else
        {
            // Zaman bittiğinde yavaşça solup gizlen
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, Time.deltaTime * 2f);
        }
    }

    public void HasarAl(float hasarMiktari, Vector2 darbeGelisYonu)
    {
        if (dokunulmazMi || mevcutCan <= 0) return;

        mevcutCan = Mathf.Clamp(mevcutCan - hasarMiktari, 0f, maksimumCan);
        kaybolmaZamanlayici = gorunmeSuresi; // Hasar anında barı görünür yap

        if (mevcutCan <= 0)
        {
            Debug.Log("Karakter öldü!");
            // Öldüğünde yapılacak işlem
        }
        else
        {
            StartCoroutine(HasarRutini(darbeGelisYonu));
        }
    }

    private IEnumerator HasarRutini(Vector2 darbeGelisYonu)
    {
        dokunulmazMi = true;

        if (rb != null)
        {
            Vector2 itis = new Vector2(-darbeGelisYonu.x, 0.5f).normalized;
            #if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = itis * geriyeSavrulmaGucu;
            #else
                rb.velocity = itis * geriyeSavrulmaGucu;
            #endif
        }

        if (sr != null)
        {
            Color orijinalRenk = Color.white;
            Color darbeRengi = new Color(1f, 0.3f, 0.3f, 0.6f);

            for (int i = 0; i < 3; i++)
            {
                sr.color = darbeRengi;
                yield return new WaitForSeconds(0.1f);
                sr.color = orijinalRenk;
                yield return new WaitForSeconds(0.1f);
            }
            sr.color = orijinalRenk;
        }

        dokunulmazMi = false;
    }

    void KarakterAltiBariKur()
    {
        // 1. Karakteri takip edecek bağımsız Dünya Alanı (World Space) Canvas'ı oluştur
        barKutusu = new GameObject("KarakterAlti_CanBari");
        Canvas canvas = barKutusu.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50; // Karakterin ve çimenin önünde görünsün

        canvasGroup = barKutusu.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Başlangıçta tamamen görünmez!

        // Piksel boyutlarını dünya ölçeğine uydur
        RectTransform canvasRect = barKutusu.GetComponent<RectTransform>();
        canvasRect.sizeDelta = barBoyutu;
        barKutusu.transform.localScale = Vector3.one * 0.02f;

        // Düz doku
        Texture2D beyazTexture = Texture2D.whiteTexture;
        Sprite duzSprite = Sprite.Create(beyazTexture, new Rect(0, 0, 4, 4), Vector2.zero);

        // 2. Koyu arka plan çerçevesi
        GameObject arkaplan = new GameObject("Arkaplan");
        arkaplan.transform.SetParent(barKutusu.transform, false);
        RectTransform arkaplanRect = arkaplan.AddComponent<RectTransform>();
        arkaplanRect.sizeDelta = barBoyutu + new Vector2(2f, 2f);
        Image bgImg = arkaplanAddComponentImage(arkaplan, duzSprite, new Color(0.08f, 0.08f, 0.08f, 0.85f));

        // 3. Kırmızı eriyen can dolgusu
        GameObject dolgu = new GameObject("Dolgu");
        dolgu.transform.SetParent(arkaplan.transform, false);
        RectTransform dolguRect = dolgu.AddComponent<RectTransform>();
        dolguRect.sizeDelta = barBoyutu;

        dolguGorseli = dolgu.AddComponent<Image>();
        dolguGorseli.sprite = duzSprite;
        dolguGorseli.color = barRengi;
        dolguGorseli.type = Image.Type.Filled;
        dolguGorseli.fillMethod = Image.FillMethod.Horizontal;
        dolguGorseli.fillOrigin = (int)Image.OriginHorizontal.Left;
        dolguGorseli.fillAmount = 1f;
    }

    private Image arkaplanAddComponentImage(GameObject obj, Sprite sprite, Color renk)
    {
        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.color = renk;
        return img;
    }

    private void OnDestroy()
    {
        // Karakter sahneden silinirse altındaki bar objesini de temizle
        if (barKutusu != null)
        {
            Destroy(barKutusu);
        }
    }
}