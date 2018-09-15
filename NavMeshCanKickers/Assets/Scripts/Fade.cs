using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static Fade instance;

    void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static Coroutine Open()
    {
        if (instance == null) {
            return null;
        }
        return instance.StartCoroutine(instance.OpenClose(true));
    }

    public static Coroutine Close()
    {
        if (instance == null) {
            return null;
        }
        return instance.StartCoroutine(instance.OpenClose(false));
    }

    public IEnumerator OpenClose(bool b)
    {
        instance.animator.SetBool("open", b);
        yield return new WaitForSeconds(0.5f);
    }
}
