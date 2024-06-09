using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class textController : MonoBehaviour
{
    TextMeshProUGUI text;
    Dictionary<KeyCode, string> mapAlphabetical = new Dictionary<KeyCode, string>
    {
        { KeyCode.R, "ぁ" },
        { KeyCode.S, "い" },
        { KeyCode.E, "ぇ" },
        { KeyCode.F, "ぉ" },
        { KeyCode.A, "け" },
        { KeyCode.Q, "げ" },
        { KeyCode.T, "さ" },
        { KeyCode.D, "し" },
        { KeyCode.W, "じ" },
        { KeyCode.C, "ず" },
        { KeyCode.Z, "せ" },
        { KeyCode.X, "ぜ" },
        { KeyCode.V, "そ" },
        { KeyCode.G, "ぞ" },
        { KeyCode.K, "た" },
        { KeyCode.I, "ち" },
        { KeyCode.J, "っ" },
        { KeyCode.U, "づ" },
        { KeyCode.H, "で" },
        { KeyCode.Y, "に" },
        { KeyCode.N, "ぬ" },
        { KeyCode.B, "ば" },
        { KeyCode.M, "ぱ" },
        { KeyCode.L, "び" },
        { KeyCode.O, "だ" },
        { KeyCode.P, "つ" },
    };

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
        text.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        /*
        foreach (var (key, word) in mapAlphabetical)
        {
            if (Input.GetKeyDown(key))
            {
                text.text = word;
            }
        }
        */
    }
}
