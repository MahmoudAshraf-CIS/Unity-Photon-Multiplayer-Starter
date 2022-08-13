using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnPC : MonoBehaviour
{

#if UNITY_EDITOR || !(UNITY_IOS || UNITY_ANDROID)
    private void Awake()
    {
        gameObject.SetActive(false);
    }
#endif
}
