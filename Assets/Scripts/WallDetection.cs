using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class WallDetection : MonoBehaviour
{
    [Header("Post Processing")]
    public Volume _volume;
    public float chromaticAberrationMaxValue = 1;
    public float lensDistortionMaxValue = -0.8f;
    public float vignetteMaxValue = 0.5f;
    public float filmGrainMaxValue = 1;
    public float motionBlurMaxValue = 1;
    private ChromaticAberration _chromaticAberration;
    private LensDistortion _lensDistortion;
    private Vignette _vignette;
    private FilmGrain _filmGrain;
    private MotionBlur _motionBlur;

    [Header("Collider Depth")]
    public Collider targetCollider;
    //[SerializeField]private List<Collider> colliders = new List<Collider>();
    //private float[] distances = new float[3];
    [SerializeField] private bool isEntered;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private Collider leftHandCollider;
    [SerializeField] private Collider rightHandCollider;
    [SerializeField] private float bodyDistance;
    [SerializeField] private float leftHandDistance;
    [SerializeField] private float rightHandDistance;

    [Header("VFX parameters")]
    public float maxValue;
    [SerializeField] [Range(0, 1)] private float para = 0;
    [SerializeField] private float maxPenetrationDepth;

    [Header("Controller Vibration")]
    public int iteration;
    public int frequency;
    [Range(0, 255)] public int maxStrength;

    [Header("Whisper Rotate")]
    public Transform whisper1;
    public Transform whisper2;
    public Transform player;
    public float orbitSpeed1;
    public float orbitSpeed2;
    private AudioSource audioSource1;
    private AudioSource audioSource2;
    void Start()
    {
        _volume.profile.TryGet<ChromaticAberration>(out _chromaticAberration);
        _volume.profile.TryGet<LensDistortion>(out _lensDistortion);
        _volume.profile.TryGet<Vignette>(out _vignette);
        _volume.profile.TryGet<FilmGrain>(out _filmGrain);
        _volume.profile.TryGet<MotionBlur>(out _motionBlur);

        audioSource1 = whisper1.GetComponent<AudioSource>();
        audioSource2 = whisper2.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isEntered)
        {
            CalculateMultipleCollisionDepth();
            DataMatching();
        }

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            OVRInput.SetControllerVibration(1, 1);

            Debug.Log("one, custom vibration");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.instance.triedGoOut == false)
                GameManager.instance.triedGoOut = true;

            //colliders.Add(other);
            bodyCollider = other;
            CheckStatus();
        }
        else if (other.CompareTag("LeftHand"))
        {
            //colliders.Add(other);
            leftHandCollider = other;
            CheckStatus();
        }
        else if (other.CompareTag("RightHand"))
        {
            //colliders.Add(other);
            rightHandCollider = other;
            CheckStatus();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //colliders.Remove(other);
            bodyCollider = null;
            bodyDistance = 0;
            CheckStatus();
        }
        else if (other.CompareTag("LeftHand"))
        {
            //colliders.Remove(other);
            leftHandCollider = null;
            leftHandDistance = 0;
            CheckStatus();
        }
        else if (other.CompareTag("RightHand"))
        {
            //colliders.Remove(other);
            rightHandCollider = null;
            rightHandDistance = 0;
            CheckStatus();
        }
    }

    private void CheckStatus()
    {
        //if (colliders.Count != 0)
        //{
        //    isEntered = true;
        //    Debug.Log("Player Enter");
        //}
        //else
        //{
        //    isEntered = false;
        //    maxPenetrationDepth = 0;
        //    para = 0;
        //    Debug.Log("Player Exit");
        //}

        if(bodyCollider==null&&
            leftHandCollider==null&&
            rightHandCollider == null)
        {
            isEntered = false;
            //reset collider data
            maxPenetrationDepth = 0;
            //reset para data (for post processing)
            para = 0;
            //reset voice data
            audioSource1.Stop();
            audioSource2.Stop();
            Debug.Log("Player Exit");
        }
        else
        {
            isEntered = true;
            //voice start
            audioSource1.Play();
            audioSource2.Play();
            Debug.Log("Player Enter");
        }
    }

    private void DataMatching()
    {
        if (maxPenetrationDepth > 0 && maxPenetrationDepth < maxValue)
        {
            para = maxPenetrationDepth / maxValue;
        }
        else if (maxPenetrationDepth > maxValue)
        {
            para = 1;
        }

        //post processing data
        _chromaticAberration.intensity.value = para * chromaticAberrationMaxValue;
        _lensDistortion.intensity.value = para * lensDistortionMaxValue;
        _vignette.intensity.value = para * vignetteMaxValue;
        _filmGrain.intensity.value = para * filmGrainMaxValue;
        _motionBlur.intensity.value = para * motionBlurMaxValue;

        //controller data
        //ControllerVibrationIndependently();
        ControllerVibrationWithHeadset();

        //whisper data
        WhisperOrbiting();
    }

    private void CalculateMultipleCollisionDepth()
    {
        //foreach (Collider collider in colliders)
        //{
        //    if (collider != targetCollider)
        //    {
        //        Vector3 direction;
        //        float distance;
        //        bool isOverlap = Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation,
        //            targetCollider, targetCollider.transform.position, targetCollider.transform.rotation,
        //            out direction, out distance);

        //        if (isOverlap)
        //        {
        //            distances[colliders.IndexOf(collider)] = distance;
        //            maxPenetrationDepth = distance;
        //        }
        //    }
        //}

        //maxPenetrationDepth = distances.Max();

        if (bodyCollider != null && bodyCollider != targetCollider)
        {
            Vector3 direction;
            float distance;
            bool isOverlap = Physics.ComputePenetration(bodyCollider, bodyCollider.transform.position, bodyCollider.transform.rotation,
                targetCollider, targetCollider.transform.position, targetCollider.transform.rotation,
                out direction, out distance);

            if (isOverlap)
            {
                bodyDistance = distance;
            }
        }

        if (leftHandCollider != null && leftHandCollider != targetCollider)
        {
            Vector3 direction;
            float distance;
            bool isOverlap = Physics.ComputePenetration(leftHandCollider, leftHandCollider.transform.position, leftHandCollider.transform.rotation,
                targetCollider, targetCollider.transform.position, targetCollider.transform.rotation,
                out direction, out distance);

            if (isOverlap)
            {
                leftHandDistance = distance;
            }
        }

        if (rightHandCollider != null && rightHandCollider != targetCollider)
        {
            Vector3 direction;
            float distance;
            bool isOverlap = Physics.ComputePenetration(rightHandCollider, rightHandCollider.transform.position, rightHandCollider.transform.rotation,
                targetCollider, targetCollider.transform.position, targetCollider.transform.rotation,
                out direction, out distance);

            if (isOverlap)
            {
                rightHandDistance = distance;
            }
        }

        maxPenetrationDepth = Mathf.Max(bodyDistance, leftHandDistance, rightHandDistance);
    }

    private void ControllerVibrationIndependently()
    {
        float leftPara = leftHandDistance < maxValue ? leftHandDistance / maxValue : 1;
        if (leftHandCollider != null)
        {
            VibrationManager.instance.TriggerVibration(iteration, frequency,
                Mathf.RoundToInt(maxStrength * leftPara), OVRInput.Controller.LTouch);
        }

        float rightPara = rightHandDistance < maxValue ? rightHandDistance / maxValue : 1;
        if (rightHandCollider != null)
        {
            VibrationManager.instance.TriggerVibration(iteration, frequency,
                Mathf.RoundToInt(maxStrength * rightPara), OVRInput.Controller.RTouch);
        }
    }

    private void ControllerVibrationWithHeadset()
    {
        VibrationManager.instance.TriggerVibration(iteration, frequency,
            Mathf.RoundToInt(maxStrength * para), OVRInput.Controller.LTouch);
        VibrationManager.instance.TriggerVibration(iteration, frequency,
            Mathf.RoundToInt(maxStrength * para), OVRInput.Controller.RTouch);
    }

    private void WhisperOrbiting()
    {
        audioSource1.volume = audioSource2.volume = para;

        whisper1.RotateAround(player.position, transform.up, orbitSpeed1 * Time.deltaTime);
        whisper2.RotateAround(player.position, transform.up, orbitSpeed2 * Time.deltaTime);
    }
}
