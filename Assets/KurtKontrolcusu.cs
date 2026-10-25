using UnityEngine;

public class KurtKontrolcusu : MonoBehaviour
{
    [Header("Hedef Karakter")]
    public Transform hedef;

    [Header("Menzil ve Hız Ayarları")]
    public float kovalamaMenzili = 20f;
    public float durdurmaMesafesi = 1.3f;
    public float kosmaHizi = 3.5f;
    public float devriyeHizi = 1.8f;
    public float devriyeMenzili = 3.5f;
    public float titremeOnlemeEsigi = 0.5f;

    [Header("Sersemleme Ayarları")]
    public float sersemlemeSuresi = 3f;

    [Header("Hasar")]
    public float vurmaHasari = 20f;
    public float hasarBeklemeSuresi = 1f;

    [Header("Görsel Yön Ayarları")]
    [Tooltip("Kurdun orijinal çizimi sola bakıyorsa bunu işaretli tutun.")]
    public bool spriteSolaBakiyor = true;
    [Tooltip("Scale yerine SpriteRenderer.flipX kullanmak isterseniz işaretleyin.")]
    public bool flipXKullan = false;

    private bool sersemlediMi = false;
    private float sersemlemeKalanSure = 0f;
    private float sonHasarZamani = 0f;

    private float baslangicX;
    private int devriyeYonu = 1;
    private float beklemeZamani = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        baslangicX = transform.position.x;

        // Orijinal rengin beyaz/varsayılan kaldığından emin ol
        if (sr != null)
        {
            sr.color = Color.white;
        }

        if (hedef == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null)
            {
                // Unity 6 için güncellendi (FindObjectsSortMode parametresi kaldırıldı)
                var tumObjeler = Object.FindObjectsByType<Transform>();
                foreach (var t in tumObjeler)
                {
                    if (t.name.ToLower().Contains("karakter") || t.name.ToLower().Contains("kırmızı") || t.name.ToLower().Contains("kirmizi"))
                    {
                        playerObj = t.gameObject;
                        break;
                    }
                }
            }

            if (playerObj != null)
            {
                hedef = playerObj.transform;
            }
        }
    }

    void Update()
    {
        // 1. Sersemleme Durumu
        if (sersemlediMi)
        {
            sersemlemeKalanSure -= Time.deltaTime;
            HizAta(0f);

            if (sersemlemeKalanSure <= 0f)
            {
                Ayil();
            }
            return;
        }

        if (hedef == null)
        {
            DevriyeAt();
            return;
        }

        float mesafe = Vector2.Distance(transform.position, hedef.position);

        if (mesafe > kovalamaMenzili)
        {
            DevriyeAt();
        }
        else
        {
            Kovalamaca();
        }
    }

    void Kovalamaca()
    {
        float yatayFark = hedef.position.x - transform.position.x;
        float mesafe = Vector2.Distance(transform.position, hedef.position);

        // Platform altındayken titreme engeli (Ölü Bölge)
        if (Mathf.Abs(yatayFark) < titremeOnlemeEsigi)
        {
            HizAta(0f);
            AnimasyonlariGuncelle(false);
            return;
        }

        // Kıza vurma mesafesinde dur
        if (mesafe <= durdurmaMesafesi)
        {
            HizAta(0f);
            AnimasyonlariGuncelle(false);
            return;
        }

        float yon = Mathf.Sign(yatayFark);
        HizAta(yon * kosmaHizi);
        YonDegistir(yon);
        AnimasyonlariGuncelle(true);
    }

    void DevriyeAt()
    {
        if (beklemeZamani > 0f)
        {
            beklemeZamani -= Time.deltaTime;
            HizAta(0f);
            AnimasyonlariGuncelle(false);
            return;
        }

        float suankiX = transform.position.x;

        if (devriyeYonu == 1 && suankiX >= baslangicX + devriyeMenzili)
        {
            devriyeYonu = -1;
            beklemeZamani = 0.8f;
            YonDegistir(-1);
            AnimasyonlariGuncelle(false);
            return;
        }
        else if (devriyeYonu == -1 && suankiX <= baslangicX - devriyeMenzili)
        {
            devriyeYonu = 1;
            beklemeZamani = 0.8f;
            YonDegistir(1);
            AnimasyonlariGuncelle(false);
            return;
        }

        HizAta(devriyeYonu * devriyeHizi);
        YonDegistir(devriyeYonu);
        AnimasyonlariGuncelle(true);
    }

    void HizAta(float xHizi)
    {
        if (rb == null) return;
        #if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = new Vector2(xHizi, rb.linearVelocity.y);
        #else
        rb.velocity = new Vector2(xHizi, rb.velocity.y);
        #endif
    }

    void YonDegistir(float yon)
    {
        if (yon == 0) return;

        float carpan = spriteSolaBakiyor ? -1f : 1f;

        if (flipXKullan && sr != null)
        {
            sr.flipX = spriteSolaBakiyor ? (yon > 0) : (yon < 0);
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(yon) * carpan;
            transform.localScale = scale;
        }
    }

    void AnimasyonlariGuncelle(bool hareketEdiyor)
    {
        if (anim == null || sersemlediMi) return;

        foreach (var p in anim.parameters)
        {
            string isim = p.name.ToLower();

            if (p.type == AnimatorControllerParameterType.Bool)
            {
                if (isim.Contains("run") || isim.Contains("kos") || isim.Contains("walk") || 
                    isim.Contains("yuru") || isim.Contains("move") || isim.Contains("hareket"))
                {
                    anim.SetBool(p.name, hareketEdiyor);
                }
            }
            else if (p.type == AnimatorControllerParameterType.Float)
            {
                if (isim.Contains("speed") || isim.Contains("hiz"))
                {
                    anim.SetFloat(p.name, hareketEdiyor ? kosmaHizi : 0f);
                }
            }
        }
    }

    // Çörek Çarptığında Tetiklenen Fonksiyonlar
    public void Sersemle(float sure)
    {
        Sersemlet(sure);
    }

    public void Sersemle()
    {
        Sersemlet(sersemlemeSuresi);
    }

    public void Sersemlet(float sure)
    {
        sersemlediMi = true;
        sersemlemeKalanSure = sure;

        if (sr != null)
        {
            sr.color = Color.white;
        }

        HizAta(0f);
        SersemlemeAnimasyonunuBaslat();
    }

    void SersemlemeAnimasyonunuBaslat()
    {
        if (anim == null) return;

        foreach (var p in anim.parameters)
        {
            string isim = p.name.ToLower();

            if (isim.Contains("stun") || isim.Contains("sersem") || isim.Contains("hit") || isim.Contains("darbe"))
            {
                if (p.type == AnimatorControllerParameterType.Trigger)
                {
                    anim.SetTrigger(p.name);
                }
                else if (p.type == AnimatorControllerParameterType.Bool)
                {
                    anim.SetBool(p.name, true);
                }
            }
            else if (p.type == AnimatorControllerParameterType.Bool)
            {
                if (isim.Contains("run") || isim.Contains("kos") || isim.Contains("walk") || isim.Contains("move"))
                {
                    anim.SetBool(p.name, false);
                }
            }
            else if (p.type == AnimatorControllerParameterType.Float)
            {
                if (isim.Contains("speed") || isim.Contains("hiz"))
                {
                    anim.SetFloat(p.name, 0f);
                }
            }
        }

        if (StateMevcutMu("Kurt_stun"))
        {
            anim.Play("Kurt_stun", 0, 0f);
        }
        else if (StateMevcutMu("Stun"))
        {
            anim.Play("Stun", 0, 0f);
        }
        else
        {
            anim.Play("Kurt_stun");
        }
    }

    void Ayil()
    {
        sersemlediMi = false;

        if (sr != null)
        {
            sr.color = Color.white;
        }

        if (anim != null)
        {
            foreach (var p in anim.parameters)
            {
                string isim = p.name.ToLower();
                if ((isim.Contains("stun") || isim.Contains("sersem")) && p.type == AnimatorControllerParameterType.Bool)
                {
                    anim.SetBool(p.name, false);
                }
            }

            if (StateMevcutMu("Kurt_idle"))
            {
                anim.Play("Kurt_idle", 0, 0f);
            }
            else if (StateMevcutMu("Idle"))
            {
                anim.Play("Idle", 0, 0f);
            }
        }
    }

    bool StateMevcutMu(string stateAdi)
    {
        if (anim == null) return false;
        return anim.HasState(0, Animator.StringToHash(stateAdi)) ||
               anim.HasState(0, Animator.StringToHash("Base Layer." + stateAdi));
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (sersemlediMi) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.ToLower().Contains("karakter"))
        {
            if (Time.time - sonHasarZamani >= hasarBeklemeSuresi)
            {
                OtomatikCanBari canBari = collision.gameObject.GetComponent<OtomatikCanBari>();
                if (canBari != null)
                {
                    Vector2 darbeYonu = (collision.transform.position - transform.position).normalized;
                    canBari.HasarAl(vurmaHasari, darbeYonu);
                    sonHasarZamani = Time.time;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        float merkezX = Application.isPlaying ? baslangicX : transform.position.x;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(merkezX - devriyeMenzili, transform.position.y, 0),
                        new Vector3(merkezX + devriyeMenzili, transform.position.y, 0));

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, kovalamaMenzili);
    }
}