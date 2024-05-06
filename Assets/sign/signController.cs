using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;



public class signController : MonoBehaviour
{
    Animator anim;
    TextMeshProUGUI text;
    Dictionary<KeyCode, string> mapAlphabetical = new Dictionary<KeyCode, string>
    {
        { KeyCode.R, "giyeok" },
        { KeyCode.S, "nieun" },
        { KeyCode.E, "digeut" },
        { KeyCode.F, "rieul" },
        { KeyCode.A, "mieum" },
        { KeyCode.Q, "bieup" },
        { KeyCode.T, "shiot" },
        { KeyCode.D, "ieung" },
        { KeyCode.W, "jieut" },
        { KeyCode.C, "chieuch" },
        { KeyCode.Z, "kieuk" },
        { KeyCode.X, "tieut" },
        { KeyCode.V, "pieup" },
        { KeyCode.G, "hieu" },
        { KeyCode.K, "a" },
        { KeyCode.I, "ya" },
        { KeyCode.J, "eo" },
        { KeyCode.U, "yeo" },
        { KeyCode.H, "o" },
        { KeyCode.Y, "yo" },
        { KeyCode.N, "u" },
        { KeyCode.B, "yu" },
        { KeyCode.M, "eu" },
        { KeyCode.L, "i" },
        { KeyCode.P, "e" },
    };

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        if (text == null)
            print("null is returned.");
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var (key, action) in mapAlphabetical)
        {
            if (Input.GetKeyDown(key))
            {
                anim.CrossFade(action, 0.4f);
            }
        }
    }
}
