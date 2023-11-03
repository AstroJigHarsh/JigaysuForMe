using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class Chat : MonoBehaviour
{

    [SerializeField]
    public InputField inputField;
    public GameObject Content;
    public GameObject Message;

    public void SendMessage()
    {
        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All,(PhotonNetwork.NickName + " : " + inputField.text));
    }

    [PunRPC]
    public void GetMessage(String ReceiveMessage)
    {
        GameObject M = Instantiate(Message , Vector3.zero , Quaternion.identity , Content.transform);
        M.GetComponent<Message>().MessageObj.text = ReceiveMessage;
    }

}
