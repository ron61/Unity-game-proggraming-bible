using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PopText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float popSpeed = 5f;
    [SerializeField] private float showTime = 1f;
    [SerializeField] private RectTransform canvasTransform;

    public PopEvent onPopEnd = new PopEvent();

    public class PopEvent : UnityEvent<PopText> { }

    private RectTransform mTrans;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (canvasTransform == null) {
            var c = GetComponentInParent<Canvas>();
            canvasTransform = c.GetComponent<RectTransform>();
        }
        if (mTrans == null) {
            mTrans = GetComponent<RectTransform>();
        }
    }

    public void Show(string text, Vector3 popPosition)
    {
        gameObject.SetActive(true);
        label.text = text;
        StartCoroutine(Pop(popPosition));
    }

    private IEnumerator Pop(Vector3 popPosition)
    {
        var ofs = Vector2.zero;
        for (var t = 0f; t < showTime; t += Time.deltaTime) {
            ofs.y += popSpeed * Time.deltaTime;
            mTrans.anchoredPosition = CanvasPosition(popPosition, canvasTransform) + ofs;
            yield return null;
        }
        gameObject.SetActive(false);
        onPopEnd.Invoke(this);
    }

    private static Vector2 CanvasPosition(Vector3 position, RectTransform canvasTransform)
    {
        var scrpos = Camera.main.WorldToScreenPoint(position);
        var lpos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, scrpos, null, out lpos);
        return lpos;
    }
}