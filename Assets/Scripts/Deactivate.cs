using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Deactivate: MonoBehaviourPunCallbacks
{
    public GameObject Kamera;
    void Start()
    {
        if(!photonView.IsMine)
        {
            Kamera.SetActive(false);
        }
    } 
}