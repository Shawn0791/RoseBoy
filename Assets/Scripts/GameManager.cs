using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool bellRang { get; set; }
    public bool triedGoOut { get; set; }
    public bool canDraw { get; set; }
    private void Awake()
    {
        instance = this;
    }
}
