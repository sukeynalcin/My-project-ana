using UnityEngine;
using UnityEngine.InputSystem;

public class CorekFirlatici : MonoBehaviour
{
    [Header("Mermi Prefabları")]
    public GameObject[] corekPrefablar;

    [Header("Uçuş Hissi (Sakin ve Tatlı)")]
    [Tooltip("Çöreğin süzülme hızı. Çok hızlı olmasın diye 9-10 civarı idealdir.")]
    public float ucusHizi = 9.5f;
    public float yercekimiCarpani = 1.3f;

    [Header("Nişan Noktaları (Tam 3 Nokta)")]
    public int noktaSayisi = 3;
    [Range(0.05f, 0.25f)]
    public float noktaBoyutu = 0.11f;
    [Range(0.1f, 1f)]
    public float saydamlik = 0.40f;

    private GameObject[] noktalar;

    void Start()
    {
        LineRenderer eskiCizgi = GetComponent<LineRenderer>();
        if (eskiCizgi != null) eskiCizgi.enabled = false;

        if (ucusHizi <= 0) ucusHizi = 9.5f;
        if (yercekimiCarpani <= 0) yercekimiCarpani = 1.3f;

        NoktalariOlustur();
    }

    void Update()
    {
        // Platform taşıma modu aktifse veya az önce platform yerleştirildiyse çörek atışını ve nişan noktalarını kilitle
        if (PlatformKopyalaBolgesi.kopyalamaModuAktif || (Time.time - PlatformKopyalaBolgesi.sonYapistirmaZamani < 0.25f))
        {
            NoktalariGizle();
            return;
        }

        Vector2 baslangicKonumu = (Vector2)transform.position;
        Vector2 fareDunya = FareDunyaPozisyonu();

        // Sol/sağ ayrımı
        float yonX = fareDunya.x >= baslangicKonumu.x ? 1f : -1f;
        Vector2 cikisNoktasi = baslangicKonumu + new Vector2(yonX * 0.5f, 0.2f);

        // Çöreğin tam fareye gitmesi için gereken kavisli hızı hesapla
        Vector2 atisHizi = HedefeGidisHiziniHesapla(cikisNoktasi, fareDunya, out float ucusSuresi);

        // 3 silik noktayı bu kavisin üzerine diz
        NisanNoktalariniGuncelle(cikisNoktasi, atisHizi, ucusSuresi);

        // Atış (F veya Fare Sol Tık)
        bool atisBasildi = (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) ||
                           (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (atisBasildi)
        {
            Firlat(cikisNoktasi, atisHizi);
        }
    }

    // Yerçekimini önceden hesaba katan parabolik atış formülü:
    Vector2 HedefeGidisHiziniHesapla(Vector2 cikis, Vector2 hedef, out float ucusSuresi)
    {
        Vector2 fark = hedef - cikis;
        float mesafe = Mathf.Max(fark.magnitude, 0.6f);

        // Mesafe arttıkça havada kalma süresini belirle
        ucusSuresi = Mathf.Clamp(mesafe / ucusHizi, 0.25f, 1.8f);

        float g = Physics2D.gravity.y * yercekimiCarpani;

        // X ekseninde sabit hız
        float vx = fark.x / ucusSuresi;
        // Y ekseninde yerçekimini yenecek kadar yukarı eğim
        float vy = (fark.y - (0.5f * g * ucusSuresi * ucusSuresi)) / ucusSuresi;

        return new Vector2(vx, vy);
    }

    void NisanNoktalariniGuncelle(Vector2 cikis, Vector2 ilkHiz, float toplamSure)
    {
        if (CorekSayaci.Instance == null || CorekSayaci.Instance.toplamCorek <= 0)
        {
            NoktalariGizle();
            return;
        }

        float g = Physics2D.gravity.y * yercekimiCarpani;

        // 3 noktayı farenin olduğu hedefe doğru kavis boyu eşit dağıt
        for (int i = 0; i < noktaSayisi; i++)
        {
            if (noktalar[i] == null) continue;
            noktalar[i].SetActive(true);

            // Noktaları yolun %25, %50 ve %75'ine yerleştirir (3. nokta tam hedefe yaklaşır)
            float t = toplamSure * ((float)(i + 1) / (noktaSayisi + 1));

            float x = cikis.x + (ilkHiz.x * t);
            float y = cikis.y + (ilkHiz.y * t) + (0.5f * g * t * t);

            noktalar[i].transform.position = new Vector3(x, y, 0f);
        }
    }

    void NoktalariOlustur()
    {
        if (noktalar != null)
        {
            for (int i = 0; i < noktalar.Length; i++)
            {
                if (noktalar[i] != null) Destroy(noktalar[i]);
            }
        }

        Sprite daireSprite = YuvarlakSpriteUret();
        noktalar = new GameObject[noktaSayisi];

        for (int i = 0; i < noktaSayisi; i++)
        {
            GameObject nokta = new GameObject("NisanNoktasi_" + i);
            nokta.transform.SetParent(transform);
            nokta.transform.localScale = Vector3.one * noktaBoyutu;

            SpriteRenderer sr = nokta.AddComponent<SpriteRenderer>();
            sr.sprite = daireSprite;
            // İleriye gittikçe hafifçe silikleşen yumuşak beyaz:
            float kademeliSaydamlik = Mathf.Max(0.18f, saydamlik - (i * 0.08f));
            sr.color = new Color(1f, 1f, 1f, kademeliSaydamlik);
            sr.sortingOrder = 25;

            nokta.SetActive(false);
            noktalar[i] = nokta;
        }
    }

    void NoktalariGizle()
    {
        if (noktalar == null) return;
        for (int i = 0; i < noktalar.Length; i++)
        {
            if (noktalar[i] != null) noktalar[i].SetActive(false);
        }
    }

    Vector2 FareDunyaPozisyonu()
    {
        Vector3 fareEkrani = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition;
        Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();

        if (cam != null)
        {
            fareEkrani.z = Mathf.Abs(cam.transform.position.z);
            return cam.ScreenToWorldPoint(fareEkrani);
        }

        return transform.position;
    }

    void Firlat(Vector2 cikis, Vector2 atisHizi)
    {
        if (corekPrefablar == null || corekPrefablar.Length == 0) return;

        if (CorekSayaci.Instance != null && CorekSayaci.Instance.CorekHarca())
        {
            int rastgeleIndex = Random.Range(0, corekPrefablar.Length);
            GameObject secilenPrefab = corekPrefablar[rastgeleIndex];

            if (secilenPrefab != null)
            {
                GameObject mermi = Instantiate(secilenPrefab, cikis, Quaternion.identity);

                AtilanCorek atilanScript = mermi.GetComponent<AtilanCorek>();
                if (atilanScript == null) atilanScript = mermi.AddComponent<AtilanCorek>();

                atilanScript.Firlat(atisHizi, yercekimiCarpani);
            }
        }
    }

    Sprite YuvarlakSpriteUret()
    {
        int cap = 32;
        int yaricap = cap / 2;
        Texture2D tex = new Texture2D(cap, cap);
        Color seffaf = new Color(1, 1, 1, 0);
        Color beyaz = Color.white;

        for (int y = 0; y < cap; y++)
        {
            for (int x = 0; x < cap; x++)
            {
                float mesafe = Vector2.Distance(new Vector2(x, y), new Vector2(yaricap, yaricap));
                if (mesafe <= yaricap - 1.5f)
                    tex.SetPixel(x, y, beyaz);
                else if (mesafe <= yaricap)
                    tex.SetPixel(x, y, new Color(1, 1, 1, yaricap - mesafe));
                else
                    tex.SetPixel(x, y, seffaf);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, cap, cap), new Vector2(0.5f, 0.5f), 100f);
    }
}