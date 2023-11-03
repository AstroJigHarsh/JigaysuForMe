using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
public class UserName : MonoBehaviour
{
    public TMP_Text Username;
    public TMP_InputField SubmittedUsername;
    public GameObject submitOff;
    public GameObject SubmittedUsernameOff;
    public GameObject UsernameOn;

    // Update is called once per frame
    public void Submit()
    {
        Username.text = SubmittedUsername.text;
        PhotonNetwork.NickName = SubmittedUsername.text;
        UsernameOn.SetActive(true);
        SubmittedUsernameOff.SetActive(false);
        submitOff.SetActive(false);
        
    }
}
