using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetScore : MonoBehaviour
{
    private TextMeshProUGUI Score;
    // Start is called before the first frame update
    void Start()
    {
        Score = gameObject.GetComponent<TextMeshProUGUI>();
        Debug.Log(Score.text);
    }

    // Update is called once per frame
    void Update()
    {
        var rnd = Random.Range(0, 100000);
        Score.text = rnd.ToString();
    }
}
