using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager instance;
    private void Awake()
    {
        if (instance && instance != this)
            Destroy(this);
        else
            instance = this;
    }
    public void TriggerVibrationByAudio(AudioClip virbrationAudio,OVRInput.Controller controller)
    {
        OVRHapticsClip clip = new OVRHapticsClip(virbrationAudio);

        if (controller == OVRInput.Controller.LTouch)
        {
            //Trigger on left controller
            OVRHaptics.LeftChannel.Preempt(clip);
        }
        else if (controller == OVRInput.Controller.RTouch)
        {
            //Trigger on right controller
            OVRHaptics.RightChannel.Preempt(clip);
        }
    }

    public void TriggerVibration(int iteration, int frequency, int strength, OVRInput.Controller controller)
    {
        OVRHapticsClip clip = new OVRHapticsClip();

        for (int i = 0; i < iteration; i++)
        {
            clip.WriteSample(i % frequency == 0 ? (byte)strength : (byte)0);
        }

        if (controller == OVRInput.Controller.LTouch)
        {
            //Trigger on left controller
            OVRHaptics.LeftChannel.Preempt(clip);
        }
        else if (controller == OVRInput.Controller.RTouch)
        {
            //Trigger on right controller
            OVRHaptics.RightChannel.Preempt(clip);
        }
    }
}
