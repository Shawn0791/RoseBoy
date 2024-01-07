using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RoomDetection : MonoBehaviour
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
    [SerializeField] private bool isEntered;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private Collider leftHandCollider;
    [SerializeField] private Collider rightHandCollider;
    [SerializeField] private float bodyDistance;
    [SerializeField] private float leftHandDistance;
    [SerializeField] private float rightHandDistance;

    [Header("VFX parameters")]
    public float minValue;
    public float safeValue;
    [SerializeField] [Range(0, 1)] private float para = 0;
    [SerializeField] private float minPenetrationDepth;

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
        CalculateMultipleCollisionDepth();
        DataMatching();

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("one");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bodyCollider = other;
            CheckStatus();
        }
        else if (other.CompareTag("LeftHand"))
        {
            leftHandCollider = other;
            CheckStatus();
        }
        else if (other.CompareTag("RightHand"))
        {
            rightHandCollider = other;
            CheckStatus();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bodyCollider = null;
            bodyDistance = 0;
            CheckStatus();
        }
        //else if (other.CompareTag("LeftHand"))
        //{
        //    leftHandCollider = null;
        //    leftHandDistance = 0;
        //    CheckStatus();
        //}
        //else if (other.CompareTag("RightHand"))
        //{
        //    rightHandCollider = null;
        //    rightHandDistance = 0;
        //    CheckStatus();
        //}
    }

    private void CheckStatus()
    {
        if(bodyCollider==null&&
            leftHandCollider==null&&
            rightHandCollider == null)
        {
            isEntered = false;
            //reset collider data
            minPenetrationDepth = safeValue + 1;
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
        if (minPenetrationDepth < safeValue && minPenetrationDepth > minValue)
        {
            para = 1 - (minPenetrationDepth - minValue) / (safeValue - minValue);

            if (GameManager.instance.triedGoOut == false)
                GameManager.instance.triedGoOut = true;
        }
        else if (minPenetrationDepth < minValue)
        {
            para = 1;
        }
        else
        {
            para = 0;
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

        //if (leftHandCollider != null && leftHandCollider != targetCollider)
        //{
        //    Vector3 direction;
        //    float distance;
        //    bool isOverlap = Physics.ComputePenetration(leftHandCollider, leftHandCollider.transform.position, leftHandCollider.transform.rotation,
        //        targetCollider, targetCollider.transform.position, targetCollider.transform.rotation,
        //        out direction, out distance);

        //    if (isOverlap)
        //    {
        //        leftHandDistance = distance;
        //    }
        //}

        //if (rightHandCollider != null && rightHandCollider != targetCollider)
        //{
        //    Vector3 direction;
        //    float distance;
        //    bool isOverlap = Physics.ComputePenetration(rightHandCollider, rightHandCollider.transform.position, rightHandCollider.transform.rotation,
        //        targetCollider, targetCollider.transform.position, targetCollider.transform.rotation,
        //        out direction, out distance);

        //    if (isOverlap)
        //    {
        //        rightHandDistance = distance;
        //    }
        //}

        //minPenetrationDepth = Mathf.Min(bodyDistance, leftHandDistance, rightHandDistance);
        minPenetrationDepth = bodyDistance;
    }

    private void ControllerVibrationIndependently()
    {
        float leftPara = leftHandDistance < minValue ? leftHandDistance / minValue : 1;
        if (leftHandCollider != null)
        {
            VibrationManager.instance.TriggerVibration(iteration, frequency,
                Mathf.RoundToInt(maxStrength * leftPara), OVRInput.Controller.LTouch);
        }

        float rightPara = rightHandDistance < minValue ? rightHandDistance / minValue : 1;
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

    public Vector3 reducedSize = new Vector3(1f, 1f, 1f); // 要减少的尺寸

    private void OnDrawGizmosSelected()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        Vector3 size = boxCollider.size;
        Vector3 newSize = new Vector3(size.x - reducedSize.x, size.y - reducedSize.y, size.z - reducedSize.z);

        Vector3 center = transform.TransformPoint(boxCollider.center);

        // 绘制线框
        Gizmos.color = Color.red; // 设置线框颜色
        Gizmos.DrawWireCube(center, newSize);
    }
}
