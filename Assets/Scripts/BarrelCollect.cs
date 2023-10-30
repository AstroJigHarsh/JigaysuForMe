using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCollect : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        ScoringSys.Score += 50;
        Destroy(gameObject);
    }

}
