using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Receiver : MonoBehaviour, Interactables
{
    [SerializeField] private bool active = true;
    [SerializeField] private Puzzle[] puzzlesToSolve;
    public Transform photoPosition;

    [Header("Animation")]
    //public Color activeColor = Color.red;
    //public Color inactiveColor = Color.gray;
    //public Color solvedColor = Color.green;
    public Material activeScreenMaterial;
    public Material inactiveScreeMaterial;

    public Material activeLedMaterial;
    public Material inactiveLedMaterial;
    public Material solvedLedMaterial;
    public MeshRenderer led;
    public MeshRenderer photoScreenMesh;
    public MeshRenderer photoMesh;
    public Animator anim;

    public UnityEvent onPutAnyPhoto;
    public UnityEvent onPutWrongPhoto;

    public Photo AtualPhoto { get; private set; }
    public GameObject AtualPhotoObj { get; private set; }

    public static bool showDebugLines = true;
    private string photoImageTag = "photoImagePrefab";

    #region GETTER & SETTER
    public bool ActiveReceiverInteraction
    {
        get
        {
            return active;
        }

        set
        {
            active = value;
            if(active)
            {
                SetLedColor(LedColor.Active);
                PlayBip(BipType.Active);
            }
            else
            {
                SetLedColor(LedColor.Inactive);
            }
        }
    }
    #endregion

    private void Start()
    {
        for (int i = 0; i < puzzlesToSolve.Length; i++)
        {
            puzzlesToSolve[i].OnSolve.AddListener(() => OnSolveAnyPuzzle());
        }
    }

    private void OnSolveAnyPuzzle()
    {
        SetLedColor(LedColor.Solved);
        PlayBip(BipType.Solved);
    }

    public void PutPhoto(Photo photo)
    {
        if (!active)
            return;

        StartCoroutine(CloseAlbumRoutine());

        AtualPhoto = photo;
        photoMesh.material.SetTexture("_MainTex", AtualPhoto.renderTexture);
        RunPhotoAnim();

        onPutAnyPhoto.Invoke();
        for (int i = 0; i < puzzlesToSolve.Length; i++)
        {
            if (photo.CanResolvePuzzle(puzzlesToSolve[i]))
            {
                puzzlesToSolve[i].PutInReceiver();
                break;
            }

            if (i == puzzlesToSolve.Length - 1)
                onPutWrongPhoto.Invoke();
        }
    }

    IEnumerator CloseAlbumRoutine()
    {
        yield return null;
        Album.instance.SetAlbumVisible(false);
    }

    public void Interact()
    {
        if (!active)
            return;

        Album.instance.SetAlbumVisible(true);
        Album.instance.SetAlbumPhotoFunction(p => PutPhoto(p));
    }

    public void StopInteract()
    {
        Album.instance.SetAlbumVisible(true);
        Album.instance.ResetAlbumPhotoFunction();
        GameManager.instance.OnStopInteract();
    }

    private void OnDrawGizmos()
    {
        if (puzzlesToSolve == null || !showDebugLines)
            return;

        //draw red lines and green (onsolve obj position / puzzles to solve)
        for (int j = 0; j < puzzlesToSolve.Length; j++)
        {
            Gizmos.color = Color.red;
            Component p = puzzlesToSolve[j];
            if (p)
            {
                if (p.GetType() == typeof(Perspective))
                {
                    Perspective perspective = p as Perspective;
                    Gizmos.DrawLine(transform.position + Vector3.up, perspective.targetPosition + p.transform.position);
                }
                else if (p.GetType() == typeof(Object_Puzzle))
                {
                    Object_Puzzle obj_puzzle = p as Object_Puzzle;
                    Gizmos.DrawLine(transform.position + Vector3.up, obj_puzzle.transform.position);
                }

                //draw mesh
                GameObject pobj = p.gameObject;
                if (!pobj.activeInHierarchy)
                {
                    List<MeshFilter> m = new List<MeshFilter>(pobj.gameObject.GetComponents<MeshFilter>());
                    m.AddRange(pobj.gameObject.GetComponentsInChildren<MeshFilter>());
                    for (int i = 0; i < m.Count; i++)
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.05f);
                        Gizmos.DrawWireMesh(m[i].sharedMesh, m[i].transform.position, m[i].transform.rotation, m[i].transform.lossyScale);
                    }
                }
            }

            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.3f);
            if (puzzlesToSolve[j] != null)
            {
                for (int i = 0; i < puzzlesToSolve[j].OnSolve.GetPersistentEventCount(); i++)
                {
                    Gizmos.color = Color.green;
                    UnityEngine.Object o = puzzlesToSolve[j].OnSolve.GetPersistentTarget(i);
                    if (!o)
                        continue;

                    if (o.GetType() == typeof(GameObject))
                    {
                        GameObject obj = o as GameObject;
                        Gizmos.DrawLine(transform.position + Vector3.up, obj.transform.position);
                        Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);

                        //draw mesh
                        if (!obj.activeInHierarchy)
                        {
                            List<MeshFilter> m = new List<MeshFilter>(obj.gameObject.GetComponents<MeshFilter>());
                            m.AddRange(obj.gameObject.GetComponentsInChildren<MeshFilter>());
                            for (int k = 0; k < m.Count; k++)
                            {
                                Gizmos.color = new Color(0, 1, 0, 0.05f);
                                Gizmos.DrawWireMesh(m[k].sharedMesh, m[k].transform.position, m[k].transform.rotation, m[k].transform.lossyScale);
                            }
                        }
                    }
                    else
                    {
                        Component obj = o as Component;
                        Gizmos.DrawLine(transform.position + Vector3.up, obj.transform.position);
                        Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.1f);

                        //draw mesh
                        if (!obj.gameObject.activeInHierarchy)
                        {
                            List<MeshFilter> m = new List<MeshFilter>(obj.gameObject.GetComponents<MeshFilter>());
                            m.AddRange(obj.gameObject.GetComponentsInChildren<MeshFilter>());

                            for (int k = 0; k < m.Count; k++)
                            {
                                Gizmos.color = new Color(0, 1, 0, 0.05f);
                                Gizmos.DrawWireMesh(m[k].sharedMesh, m[k].transform.position, m[k].transform.rotation, m[k].transform.lossyScale);
                            }
                        }
                    }
                }
            }
        }

        //draw blue lines (onPutPhoto)
        Gizmos.color = Color.blue;
        for (int i = 0; i < onPutAnyPhoto.GetPersistentEventCount(); i++)
        {
            UnityEngine.Object o = onPutAnyPhoto.GetPersistentTarget(i);
            if (!o)
                continue;

            if (o.GetType() == typeof(GameObject))
            {
                GameObject obj = o as GameObject;
                Gizmos.DrawLine(transform.position + Vector3.up, obj.transform.position);
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);
            }
            else
            {
                Component obj = o as Component;
                Gizmos.DrawLine(transform.position + Vector3.up, obj.transform.position);
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);
            }
        }

        //draw cyan lines (onPutWrongPhoto)
        Gizmos.color = Color.cyan;
        for (int i = 0; i < onPutWrongPhoto.GetPersistentEventCount(); i++)
        {
            UnityEngine.Object o = onPutWrongPhoto.GetPersistentTarget(i);
            if (!o)
                continue;

            if (o.GetType() == typeof(GameObject))
            {
                GameObject obj = o as GameObject;
                Gizmos.DrawLine(transform.position + Vector3.up, obj.transform.position);
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);
            }
            else
            {
                Component obj = o as Component;
                Gizmos.DrawLine(transform.position + Vector3.up, obj.transform.position);
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);
            }
        }
    }
    
    //animations

    private void RunPhotoAnim()
    {
        anim.SetTrigger("Put Photo");
    }

    private enum LedColor
    {
        Active, Inactive, Solved
    }
    private void SetLedColor(LedColor color)
    {
        if(!led)
        {
            Debug.LogWarning("Led not setted");
            return;
        }

        if (!led.sharedMaterial)
            return;

        switch (color)
        {
            case LedColor.Active:
                led.material = activeLedMaterial;
                photoScreenMesh.material = activeScreenMaterial;
                break;
            case LedColor.Inactive:
                led.material = inactiveLedMaterial;
                photoScreenMesh.material = inactiveScreeMaterial;
                break;
            case LedColor.Solved:
                led.material = solvedLedMaterial;
                photoScreenMesh.material = activeScreenMaterial;
                break;
        }

    }

    private enum BipType
    {
        Active, Solved
    }
    private void PlayBip(BipType type)
    {
        SFXProfile profile = new SFXProfile(volume: 0.7f);
        switch (type)
        {
            case BipType.Active:
                SFX.Play("Receiver_Active", profile);
                break;
            case BipType.Solved:
                SFX.Play("Receiver_Solved", profile);
                break;
        }
    }

    public void ShowAtualPhotoInScreen()
    {
        if(AtualPhoto != null)
        {
            photoScreenMesh.material.SetTexture("_MainTex", AtualPhoto.renderTexture);
        }
    }

    private void OnValidate()
    {
        LedColor color = active ? LedColor.Active : LedColor.Inactive;
        SetLedColor(color);
    }
}
