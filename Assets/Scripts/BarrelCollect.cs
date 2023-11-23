using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCollect : MonoBehaviour
{
    public GameObject Manager;
    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
        Manager.GetComponent<UserName>().SendLeaderboard();
    }
}
