using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventRegion : MonoBehaviour
{
    public LayerMask onTriggerEnterMask = -1;
    public UnityEvent onTriggerEnter;
    [Space(20)]
    public LayerMask onTriggerExitMask = -1;
    public UnityEvent onTriggerExit;
    [Space(20)]
    public LayerMask onTriggerStayMask = -1;
    public UnityEvent onTriggerStay;

    private void OnTriggerEnter(Collider other)
    {
        if((onTriggerEnterMask | (1 << other.gameObject.layer)) == onTriggerEnterMask)
        {
            onTriggerEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((onTriggerExitMask | (1 << other.gameObject.layer)) == onTriggerExitMask)
        {
            onTriggerExit.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ((onTriggerStayMask | (1 << other.gameObject.layer)) == onTriggerStayMask)
        {
            onTriggerStay.Invoke();
        }
    }

}
