using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TotalScoreMM : MonoBehaviour
{
    public GameObject TsT;

    void Update()
    {
        TsT.GetComponent<Text>().text = "Score: " + ScoringSys.Score;
    }
}