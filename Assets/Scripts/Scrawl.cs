using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scrawl : MonoBehaviour
{
    public Material[] mats;
    public float dissolveDuration;
    public GameObject room;
    public GameObject lighting;
    public GameObject toilet;
    public Animator whiteAnimator;
    public float restartTime;
    private bool isDissolving;
    public float penSpeedMin;
    public float penValueMax;
    private float penDrawValue;
    public AudioSource penDrawAudio;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Pen")&&
    //        GameManager.instance.canDraw)
    //    {
    //        StartTransition();
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Pen"))
        {
            float value = other.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
            if (penDrawValue < penValueMax &&
                GameManager.instance.canDraw &&
                value > penSpeedMin)
            {
                penDrawValue += value;

                if (!penDrawAudio.isPlaying)
                    penDrawAudio.Play();
            }
            else
            {
                if (penDrawAudio.isPlaying)
                    penDrawAudio.Pause();
            }

            if (penDrawValue >= penValueMax)
            {
                StartTransition();
            }

            Debug.Log(value);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pen"))
        {
            if (penDrawAudio.isPlaying)
                penDrawAudio.Pause();
        }
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ResetMats();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.instance.canDraw = true;
            GameManager.instance.triedGoOut = true;
            GameManager.instance.bellRang = true;
        }
    }

    private void StartTransition()
    {
        if (!isDissolving)
        {
            isDissolving = true;
            StartCoroutine(StartDissolve());
        }
    }

    private IEnumerator StartDissolve()
    {
        //objects outdoor
        room.SetActive(false);
        lighting.SetActive(false);

        //toilet
        isDissolving = true;
        float elapsedTimer1 = 0;

        while (elapsedTimer1 < dissolveDuration)
        {
            elapsedTimer1 += Time.deltaTime;

            float strength = Mathf.Lerp(0, 1, elapsedTimer1 / dissolveDuration);
            mats[0].SetFloat("_Dissolve", strength);

            yield return null;
        }

        float elapsedTimer2 = 0;
        whiteAnimator.SetTrigger("white");

        while (elapsedTimer2 < dissolveDuration)
        {
            elapsedTimer2 += Time.deltaTime;

            float strength = Mathf.Lerp(0, 1, elapsedTimer2 / dissolveDuration);
            mats[1].SetFloat("_Dissolve", strength);
            mats[2].SetFloat("_Dissolve", strength);
            mats[3].SetFloat("_Dissolve", strength);
            mats[4].SetFloat("_Dissolve", strength);
            mats[5].SetFloat("_Dissolve", strength);
            mats[6].SetFloat("_Dissolve", strength);
            //mats[7].SetFloat("_Dissolve", strength);
            //mats[8].SetFloat("_Dissolve", strength);

            yield return null;
        }

        toilet.SetActive(false);

        ResetMats();
        Invoke("RestartGame", restartTime);
    }

    private void ResetMats()
    {
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].SetFloat("_Dissolve", 0);
        }

        Debug.Log("reset mats");
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
}
