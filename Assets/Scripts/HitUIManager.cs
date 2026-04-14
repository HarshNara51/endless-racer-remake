using UnityEngine;
using TMPro;

public class HitUIManager : MonoBehaviour
{
    public static HitUIManager Instance;

    public TMP_Text totalHitsText;
    public GameObject floatingTextPrefab;
    public Transform canvasTransform;

    private int totalHits = 0;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterHit(string objectName)
    {
        totalHits++;
        totalHitsText.text = "Total Hits: " + totalHits;

        ShowFloatingText("Hit " + objectName);
    }

    void ShowFloatingText(string message)
    {
        GameObject obj = Instantiate(floatingTextPrefab, canvasTransform);

        TMP_Text txt = obj.GetComponent<TMP_Text>();
        txt.text = message;

        obj.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        Destroy(obj, 1.5f);
    }
}