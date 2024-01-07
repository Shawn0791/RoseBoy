using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    public Material originalMat;
    private Material clonedMat;
    public float dissolveDuration;
    //private bool canDisslove;
    private bool isDissolving;
    void Start()
    {
        clonedMat = new Material(originalMat);
        GetComponent<MeshRenderer>().material = clonedMat;
    }

    // Update is called once per frame
    void Update()
    {
        //if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        //{
        //    StartCoroutine(StartDissolve());
        //}

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(StartDissolve());
        }
    }

    private IEnumerator StartDissolve()
    {
        if (!isDissolving)
        {
            isDissolving = true;
            float elapsedTime = 0;

            while (elapsedTime < dissolveDuration)
            {
                elapsedTime += Time.deltaTime;

                float strength = Mathf.Lerp(-3, 3, elapsedTime / dissolveDuration);
                clonedMat.SetVector("_DissolveOffest", new Vector3(0, strength, 0));

                yield return null;
            }

            //Destroy(gameObject);
        }
    }
}
