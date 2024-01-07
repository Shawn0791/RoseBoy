using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class Timeline_01start : MonoBehaviour
{
    public Transform target;
    public XROrigin xrOrigin;
    public Animator blackAnimator;
    
    public void CGfinished()
    {
        //recenter
        xrOrigin.MoveCameraToWorldLocation(target.position);
        xrOrigin.MatchOriginUpCameraForward(target.up, target.forward);
        
        
        blackAnimator.SetTrigger("blackFinished");
    }
}
