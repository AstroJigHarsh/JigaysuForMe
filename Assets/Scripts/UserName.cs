using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UserName : MonoBehaviour
{
    public Text Username;
    public InputField SubmittedUsername;
    public GameObject submitOff;
    public GameObject SubmittedUsernameOff;
    public GameObject UsernameOn;

    // Update is called once per frame
    public void Submit()
    {
        Username.text = SubmittedUsername.text;
        UsernameOn.SetActive(true);
        SubmittedUsernameOff.SetActive(false);
        submitOff.SetActive(false);
        
    }
}
