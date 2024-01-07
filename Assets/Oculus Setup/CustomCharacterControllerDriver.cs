using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomCharacterControllerDriver : CharacterControllerDriver
{
    private void FixedUpdate()
    {
        UpdateCharacterController();
    }
}
