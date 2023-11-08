using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCollect : MonoBehaviour
{
    public GameObject Manager;
    public GameObject Button;
    void OnTriggerEnter(Collider other)
    {
        ScoringSys.Score += 50;
        Destroy(gameObject);
        Manager.GetComponent<UserName>().SendLeaderboard();
    }

}
