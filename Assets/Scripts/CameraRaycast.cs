using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CameraRaycast : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask layerMask;
    public float raycastInterval = 1f; 
    public float timerThreshold = 3f;

    public PlayableDirector timeline02;

    private RaycastHit hit;
    private float counter = 0;
    private bool isObserving = false;
    private bool canSay;

    void Start()
    {
        InvokeRepeating("PerformRaycast", 0f, raycastInterval);
    }
    private void PerformRaycast()
    {
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, Mathf.Infinity, layerMask.value)) 
        {
            //Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * 10f, Color.red);
            
            if (hit.collider != null)
            {
                //print(hit.collider.name);
                if (hit.collider.gameObject.CompareTag("RaycastReceiver")) 
                {
                    //print("hit");
                    if (!isObserving)
                    {
                        isObserving = true;
                        counter = 0;
                        canSay = true;
                    }
                    else
                    {
                        counter += raycastInterval;
                        if (counter >= timerThreshold)
                        {
                            Debug.Log("Successful continuous observation!");

                            counter = 0;

                            if (GameManager.instance.bellRang &&
                                GameManager.instance.triedGoOut &&
                                canSay) 
                            {
                                canSay = false;
                                timeline02.Play();
                            }
                        }
                    }
                }
                else
                {
                    isObserving = false;
                    counter = 0f;
                }
            }
        }
        else
        {
            //Debug.DrawLine(mainCamera.transform.position, mainCamera.transform.position + mainCamera.transform.forward * 10f, Color.green);

            isObserving = false;
            counter = 0f;
        }
    }
}
