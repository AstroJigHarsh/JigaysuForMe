using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioSource WalkingS;  
    public AudioSource RunningS;

    bool Walking;
    bool Running;

    void Awake()
    {
        Walking = GetComponent<MoveBehaviour>().Walking;
        Running = GetComponent<MoveBehaviour>().Running;
    }

    void Update()
    {
        SoundManagement();
    }
    void SoundManagement()
    {
        if(Walking == true && Running == false)
        {
            WalkingS.enabled = true;
            WalkingS.Play();
        }
        else
        {
            WalkingS.enabled = false;
            WalkingS.Stop();
        }
        
        if(Walking == true && Running == true)
        {
            RunningS.enabled = true;
            RunningS.Play();
        }
        else
        {
            RunningS.enabled = false;
            RunningS.Stop();
        }
    }
}
