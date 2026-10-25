using UnityEngine;
using System.Reflection;

public class DusmeHasari : MonoBehaviour
{
    [Header("Düşme Ayarları")]
    [Tooltip("Karakter bu yükseklikten daha fazla düşerse canı gider")]
    public float guvenliDususMesafesi = 5.5f;

    [Tooltip("Her 1 birim fazladan düşüşte gidecek can miktarı")]
    public int birimBasiHasar = 10;

    [Tooltip("Tek seferde gidebilecek sabit/taban hasar")]
    public int tabanHasar = 20;

    private Rigidbody2D rb;
    private float enYuksekY;
    private bool havadaMi = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enYuksekY = transform.position.y;
    }

    void Update()
    {
        // Unity 6 linearVelocity, eski sürümler için velocity kontrolü
        #if UNITY_6000_0_OR_NEWER
        float dikeyHiz = rb.linearVelocity.y;
        #else
        float dikeyHiz = rb.velocity.y;
        #endif

        // Karakter yukarı zıpladıkça tepe noktasını kaydet
        if (dikeyHiz > 0.1f)
        {
            havadaMi = true;
            if (transform.position.y > enYuksekY)
            {
                enYuksekY = transform.position.y;
            }
        }
        // Aşağı düşüş başladığında
        else if (dikeyHiz < -0.2f)
        {
            havadaMi = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Zemine yukarıdan basıp basmadığını kontrol et
        foreach (ContactPoint2D temas in collision.contacts)
        {
            if (temas.normal.y > 0.5f && havadaMi)
            {
                Zemineİndi();
                break;
            }
        }
    }

    void Zemineİndi()
    {
        havadaMi = false;
        float dusulenMesafe = enYuksekY - transform.position.y;

        // Eşik mesafesinden yüksekten düştüyse
        if (dusulenMesafe > guvenliDususMesafesi)
        {
            float fazlaMesafe = dusulenMesafe - guvenliDususMesafesi;
            int toplamHasar = tabanHasar + Mathf.RoundToInt(fazlaMesafe * birimBasiHasar);

            Debug.Log($"<color=orange>[Düşme Hasarı] {dusulenMesafe:F1} birimden düşüldü! Alınan Hasar: {toplamHasar}</color>");

            // Hasarı otomatik uyarla ve uygula
            AkilliHasarUygula(toplamHasar);
        }

        enYuksekY = transform.position.y;
    }

    void AkilliHasarUygula(int hasar)
    {
        // Karakterin üstündeki OtomatikCanBari bileşenini bul
        Component canBari = GetComponent("OtomatikCanBari");
        if (canBari == null) return;

        System.Type tip = canBari.GetType();
        MethodInfo[] metodlar = tip.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        bool hasarVerildi = false;

        // OtomatikCanBari'ndaki HasarAl fonksiyonunun kaç ve ne tür parametre istediğini çözer
        foreach (MethodInfo m in metodlar)
        {
            if (m.Name == "HasarAl")
            {
                ParameterInfo[] p = m.GetParameters();
                try
                {
                    // 1 parametreli ise: HasarAl(hasar)
                    if (p.Length == 1)
                    {
                        object arg0 = p[0].ParameterType == typeof(float) ? (object)(float)hasar : (object)hasar;
                        m.Invoke(canBari, new object[] { arg0 });
                        hasarVerildi = true;
                        break;
                    }
                    // 2 parametreli ise: HasarAl(hasar, yon/darbe/obje)
                    else if (p.Length == 2)
                    {
                        object arg0 = p[0].ParameterType == typeof(float) ? (object)(float)hasar : (object)hasar;
                        object arg1 = null;

                        if (p[1].ParameterType == typeof(Vector2)) arg1 = Vector2.up; // Yerden yukarı hafif tepki
                        else if (p[1].ParameterType == typeof(Vector3)) arg1 = Vector3.up;
                        else if (p[1].ParameterType == typeof(Transform)) arg1 = transform;
                        else if (p[1].ParameterType == typeof(GameObject)) arg1 = gameObject;

                        m.Invoke(canBari, new object[] { arg0, arg1 });
                        hasarVerildi = true;
                        break;
                    }
                }
                catch { }
            }
        }

        // Eğer fonksiyon çağrılamazsa doğrudan mevcut can değerini düşür
        if (!hasarVerildi)
        {
            FieldInfo canAlani = tip.GetField("mevcutCan", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (canAlani != null)
            {
                if (canAlani.FieldType == typeof(float))
                {
                    float suanki = (float)canAlani.GetValue(canBari);
                    canAlani.SetValue(canBari, Mathf.Max(0, suanki - hasar));
                }
                else if (canAlani.FieldType == typeof(int))
                {
                    int suanki = (int)canAlani.GetValue(canBari);
                    canAlani.SetValue(canBari, Mathf.Max(0, suanki - hasar));
                }
            }
        }
    }
}