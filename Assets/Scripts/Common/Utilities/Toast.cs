using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    
    #region Singleton
    private static Toast _instance;

    public static Toast Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    #endregion
    
    [SerializeField] private float duration;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private TextMeshProUGUI header;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    protected Toast()
    { }


    public void ShowToast(string message, string header)
    {
        this.message.text = message;
        this.header.text = header;
        Invoke("HideToast", duration);
        animator.SetTrigger("show");
    }

    private void HideToast()
    {
        animator.SetTrigger("hide");
    }
}