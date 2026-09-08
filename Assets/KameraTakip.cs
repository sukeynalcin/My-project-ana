using UnityEngine;

public class KameraTakip : MonoBehaviour
{
    public Transform hedef; // Kız (Player) objemiz
    public float yumusaklik = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        if (hedef != null)
        {
            Vector3 hedefPozisyon = hedef.position + offset;
            transform.position = Vector3.Lerp(transform.position, hedefPozisyon, yumusaklik);
        }
    }
}