using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OverHeadText : MonoBehaviour
{
    public TMP_Text tmpText;
    public Vector3 screenPosition;
    Animator anim;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        anim = GetComponent<Animator>();
        StartCoroutine(displayText());
    }

    IEnumerator displayText()
    {
        yield return new WaitForSeconds(0.02f);
        tmpText.rectTransform.position = screenPosition;
        anim.SetTrigger("show");
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }

}
