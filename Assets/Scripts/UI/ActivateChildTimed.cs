using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivateChildTimed : MonoBehaviour
{

    public bool initAtStart = true;
    public float timeInterval = 0.5f;
    public bool setSelected = false;
    public Button selectedAtStart;
    public List<GameObject> nonAffected;

    private Transform[] allChild;

    private void Awake()
    {
        GetAllChild();
    }

    private void OnEnable()
    {
        DeactivateChilds();
        if (initAtStart)
            Show();

        if (setSelected)
            if (selectedAtStart)
            {
                selectedAtStart.Select();

                Animator anim = selectedAtStart.GetComponent<Animator>();
                if(anim)
                {
                    anim.SetTrigger("OnMouseEnter");
                }

            } else
            {
                Button first = transform.GetComponentInChildren<Button>();
                if (first)
                    first.Select();
            }
    }

    private void DeactivateChilds()
    {
        for (int i = 1; i < allChild.Length; i++)
        {
            allChild[i].gameObject.SetActive(false);
        }
    }

    private void GetAllChild()
    {
        allChild = transform.GetComponentsInChildren<Transform>();
    }

    public void Show()
    {
        StartCoroutine(ShowItensTimed(allChild, timeInterval));
    }

    public IEnumerator ShowItensTimed(Transform[] objs, float timing)
    {
        for (int i = 1; i < objs.Length; i++)
        {
            if (nonAffected.Contains(objs[i].gameObject))
                continue;

            objs[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(timing);
        }
    }

}
