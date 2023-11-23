using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAbilities" , menuName = "CharacterAbilities")]
public class CharFeatures : MonoBehaviour
{

    public float walkSpeed = 0.15f;
    public float runSpeed = 1f;
    public float sprintSpeed = 2f;
    public float jumpHeight = 1.5f;
    public float jumpInertialForce = 10f;
}
