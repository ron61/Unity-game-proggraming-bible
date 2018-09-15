using System.Collections.Generic;
using UnityEngine;

public class PopTextPool : MonoBehaviour
{
    [SerializeField] private PopText popObj;
    [SerializeField] private int popMax = 5;

    private Queue<PopText> pops = new Queue<PopText>();

    void Start()
    {
        for (var i = 0; i < popMax; ++i) {
            var obj = Instantiate(popObj.transform, popObj.transform.parent);
            obj.gameObject.SetActive(false);
            var c = obj.GetComponent<PopText>();
            pops.Enqueue(c);
            c.onPopEnd.AddListener(OnPopEnd);
        }
        popObj.gameObject.SetActive(false);
    }

    void Update()
    {
    }

    public void Pop(string text, Vector3 position)
    {
        if (pops.Count == 0) {
            return;
        }
        pops.Dequeue().Show(text, position);
    }

    private void OnPopEnd(PopText p)
    {
        pops.Enqueue(p);
    }
}
