using System.Collections;
using UnityEngine;
using TMPro;

public class HudScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI number;
    [SerializeField] private TextMeshProUGUI plusNumber;
    [SerializeField] private Animation plusNumberAnimation;
    [SerializeField] private float plusAnimationTime = 0.5f;

    private int targetTotal;
    private int currentTotal;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (plusNumberAnimation == null && plusNumber != null) {
            plusNumberAnimation = plusNumber.GetComponent<Animation>();
        }
    }

    void Update()
    {
        if (targetTotal != currentTotal) {
            currentTotal += Mathf.Min(5, targetTotal - currentTotal);
            number.text = currentTotal.ToString();
        }
    }

    public void Init(string s)
    {
        label.text = s;
        number.text = "0";
        plusNumber.text = "";
    }

    public void SetColor(Color c)
    {
        label.color = c;
        number.colorGradient = new VertexGradient
        {
            topLeft = c,
            topRight = c,
            bottomLeft = Color.white,
            bottomRight = Color.white
        };
    }

    public void OnUpdateScore(int n)
    {
        number.text = n.ToString();
    }

    public void OnUpdateScore(int total, int inc)
    {
        StartCoroutine(InternalUpdateScore(total, inc));
    }

    private IEnumerator InternalUpdateScore(int total, int inc)
    {
        if (plusNumberAnimation != null) {
            plusNumber.text = "+" + inc;
            plusNumberAnimation.Play();
            plusNumberAnimation.Rewind();
            yield return new WaitForSeconds(plusAnimationTime);
        }
        targetTotal = total;
        // number.text = total.ToString();
    }
}
