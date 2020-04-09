using System.Collections;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    public static GameMgr instance;
    public string id = "Tank";

    void Awake()
    {
        instance = this;
    }
}
