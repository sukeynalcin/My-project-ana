using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlatformKopyalaBolgesi : MonoBehaviour
{
    public static bool kopyalamaModuAktif = false;
    public static float sonYapistirmaZamani = 0f;

    [Header("Platform ve Hak Ayarları")]
    public GameObject platformPrefabi;
    public int yapistirmaHakki = 1; // İstediğin kadar hak verebilirsin (1, 2, 3...)

    [Header("Hayalet Önizleme Rengi")]
    public Color onizlemeRengi = new Color(0.4f, 0.85f, 1f, 0.45f);

    private bool oyuncuIcerideMi = false;
    private bool kopyalandiMi = false;
    private Transform oyuncuTransform;
    private Text uiYazi;
    private GameObject yaziPaneli;
    private GameObject onizlemeObjesi;
    private Camera anaKamera;

    void Awake()
    {
        anaKamera = Camera.main;
        ArayuzYazisiniKur();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OyuncuyuGirisYaptir(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!oyuncuIcerideMi)
        {
            OyuncuyuGirisYaptir(other);
        }
    }

    private void OyuncuyuGirisYaptir(Collider2D other)
    {
        if (other.CompareTag("Player") || other.name.ToLower().Contains("karakter"))
        {
            oyuncuIcerideMi = true;
            oyuncuTransform = other.transform;

            if (yaziPaneli != null)
            {
                CancelInvoke(nameof(YaziyiKapat));
                yaziPaneli.SetActive(true);

                if (yapistirmaHakki > 0)
                {
                    if (!kopyalandiMi)
                    {
                        uiYazi.text = $"Bu alanı geçmek için platform kurabilirsin! (Kalan Hak: {yapistirmaHakki})\n[C] ile kopyala, Fare ile hedefle, [Sol Tık] / [V] ile yapıştır.";
                    }
                }
                else
                {
                    uiYazi.text = "Bu alandaki tüm platform haklarını kullandın! Başka hakkın kalmadı.";
                    Invoke(nameof(YaziyiKapat), 2.5f);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.name.ToLower().Contains("karakter"))
        {
            if (other.transform == oyuncuTransform)
            {
                oyuncuIcerideMi = false;
                oyuncuTransform = null;

                OnizlemeyiTemizle();
                kopyalandiMi = false;
                kopyalamaModuAktif = false;

                if (yaziPaneli != null)
                {
                    yaziPaneli.SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        if (!oyuncuIcerideMi) return;

        // 1. Kopyalama Denemesi
        if (C_TusuBasildiMi())
        {
            if (yapistirmaHakki <= 0)
            {
                // Hak bittiyse uyar
                if (yaziPaneli != null)
                {
                    CancelInvoke(nameof(YaziyiKapat));
                    yaziPaneli.SetActive(true);
                    uiYazi.text = "<color=#FF6B6B>Platform kopyalama hakkın kalmadı!</color>";
                    Invoke(nameof(YaziyiKapat), 2f);
                }
                return;
            }

            if (!kopyalandiMi)
            {
                kopyalandiMi = true;
                kopyalamaModuAktif = true;
                HayaletOnizlemeOlustur();

                if (uiYazi != null)
                {
                    uiYazi.text = $"Fare ile yeri seç, [Sol Tık] veya [V] ile yerleştir! (Kalan Hak: {yapistirmaHakki})";
                }
            }
        }

        // Hayalet platform fareyi takip eder
        if (kopyalandiMi && onizlemeObjesi != null)
        {
            onizlemeObjesi.transform.position = FareDunyaPozisyonu();
        }

        // 2. Yapıştırma
        if (YapistirTusuBasildiMi() && kopyalandiMi && yapistirmaHakki > 0)
        {
            Yapistir();
        }
    }

    private Vector3 FareDunyaPozisyonu()
    {
        if (anaKamera == null) anaKamera = Camera.main;

        Vector3 ekranPikseli = Vector3.zero;

        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 mPos = Mouse.current.position.ReadValue();
            ekranPikseli = new Vector3(mPos.x, mPos.y, 0f);
        }
        #else
        ekranPikseli = Input.mousePosition;
        #endif

        if (anaKamera == null) return Vector3.zero;

        Vector3 dunyaNoktasi = anaKamera.ScreenToWorldPoint(new Vector3(ekranPikseli.x, ekranPikseli.y, -anaKamera.transform.position.z));
        dunyaNoktasi.z = 0f;
        return dunyaNoktasi;
    }

    private void HayaletOnizlemeOlustur()
    {
        if (platformPrefabi == null)
        {
            Debug.LogError("HATA: Platform Prefabi kutusu boş!");
            return;
        }

        onizlemeObjesi = Instantiate(platformPrefabi, FareDunyaPozisyonu(), Quaternion.identity);

        Collider2D[] coller = onizlemeObjesi.GetComponentsInChildren<Collider2D>();
        foreach (var col in coller) Destroy(col);

        Rigidbody2D rb = onizlemeObjesi.GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);

        SpriteRenderer[] spritelar = onizlemeObjesi.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in spritelar)
        {
            sr.color = onizlemeRengi;
            sr.sortingOrder += 2;
        }
    }

    private void OnizlemeyiTemizle()
    {
        if (onizlemeObjesi != null)
        {
            Destroy(onizlemeObjesi);
            onizlemeObjesi = null;
        }
    }

    void Yapistir()
    {
        Vector3 yerlesimYeri = onizlemeObjesi != null ? onizlemeObjesi.transform.position : FareDunyaPozisyonu();

        OnizlemeyiTemizle();
        Instantiate(platformPrefabi, yerlesimYeri, Quaternion.identity);

        yapistirmaHakki--;
        kopyalandiMi = false;
        kopyalamaModuAktif = false;
        sonYapistirmaZamani = Time.time;

        if (uiYazi != null)
        {
            if (yapistirmaHakki > 0)
            {
                uiYazi.text = $"Platform yerleştirildi! (Kalan Platform Hakkın: {yapistirmaHakki})";
            }
            else
            {
                uiYazi.text = "Platform yerleştirildi! Başka kopyalama hakkın kalmadı, dikkatli ilerle!";
            }
        }

        CancelInvoke(nameof(YaziyiKapat));
        Invoke(nameof(YaziyiKapat), 3f);
    }

    void YaziyiKapat()
    {
        if (yaziPaneli != null)
        {
            yaziPaneli.SetActive(false);
        }
    }

    private bool C_TusuBasildiMi()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame) return true;
        #else
        if (Input.GetKeyDown(KeyCode.C)) return true;
        #endif
        return false;
    }

    private bool YapistirTusuBasildiMi()
    {
        #if ENABLE_INPUT_SYSTEM
        bool solTik = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool vTusu = Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame;
        if (solTik || vTusu) return true;
        #else
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.V)) return true;
        #endif
        return false;
    }

    void ArayuzYazisiniKur()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("RehberCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        yaziPaneli = new GameObject("RehberYazisiPaneli");
        yaziPaneli.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = yaziPaneli.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -40f);
        panelRect.sizeDelta = new Vector2(650f, 65f);

        Image panelBg = yaziPaneli.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.75f);

        GameObject textObj = new GameObject("Yazi");
        textObj.transform.SetParent(yaziPaneli.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        uiYazi = textObj.AddComponent<Text>();
        uiYazi.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiYazi.fontSize = 16;
        uiYazi.alignment = TextAnchor.MiddleCenter;
        uiYazi.color = Color.white;

        yaziPaneli.SetActive(false);
    }
}