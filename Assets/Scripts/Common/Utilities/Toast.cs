using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private TextMeshProUGUI header;
    Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    protected Toast()
    { }


    public void ShowToast(string message, string header)
    {
        Invoke("HideToast", duration);
        animator.SetTrigger("show");
    }

    private void HideToast()
    {
        animator.SetTrigger("hide");
    }
}