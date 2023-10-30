using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoringSys : MonoBehaviour
{
    public GameObject ScoreText;
    public static int Score;

    void Update()
    {
        ScoreText.GetComponent<Text>().text = "Score: " + Score;
    }
}
