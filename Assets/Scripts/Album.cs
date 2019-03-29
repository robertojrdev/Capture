using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Album : MonoBehaviour
{
    public static Album instance;

    private event Action<Photo> OnClickInPhoto;

    [SerializeField] private RawImage bigImage;
    [SerializeField] private GameObject albumObj;
    [SerializeField] private GameObject contentHolder;
    [SerializeField] private GameObject photoPrefab;
    [SerializeField] private Scrollbar listScroolBar;
    [SerializeField] private UnityEvent OnOpenAlbum;
    [SerializeField] private UnityEvent OnCloseAlbum;
    public bool IsVisible { get; private set; }
    private List<AlbumPhoto> albumPhotos = new List<AlbumPhoto>(); //instantiated buttons holding all info
    private int selectedIndex = -1;
    private int numOfColumns = 2;
    public float intervalBtwSelection = 0.1f;
    private bool isInInterval = false;
    private Transform lastSelectionOutline;
    private Coroutine scroolRoutine;

    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(this);
            Debug.Log("Multiple Album in the scene");
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        SetAlbumVisible(false);
    }

    private void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        if (Input.GetButtonDown("Open Album"))
            SetAlbumVisible(!IsVisible);

        if (!IsVisible)
            return;

        if (Input.GetButtonDown("Back"))
            SetAlbumVisible(false);
    }

    public void AddPhoto(Photo photo)
    {
        if (albumPhotos == null)
            albumPhotos = new List<AlbumPhoto>();

        GameObject albumPhotoObj = Instantiate(photoPrefab, contentHolder.transform);
        albumPhotoObj.transform.SetAsFirstSibling();
        albumPhotoObj.SetActive(true);

        AlbumPhoto albumPhoto = albumPhotoObj.GetComponent<AlbumPhoto>();
        albumPhoto.image.texture = photo.renderTexture;
        albumPhoto.button.onClick.AddListener(() => ClickPhoto(photo));

        albumPhotos.Add(albumPhoto);

        //on select
        EventTrigger buttonTrigger = albumPhoto.button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Select;
        entry.callback.AddListener(x => bigImage.texture = photo.renderTexture);
        buttonTrigger.triggers.Add(entry);
    }

    public void SetAlbumVisible(bool visible, bool setPlayer = true)
    {
        Debug.Log("Try to set album visible: " + visible);

        if (GameCamera.instance.IsActive || GameManager.instance.gameState == GameManager.GameState.Paused)
            return;

        IsVisible = visible;

        if (visible)
        {
            if(albumObj)
                albumObj.SetActive(true);
            if (setPlayer)
            {
                PlayerController.instance.SetCameraLocked(true);
                PlayerController.instance.SetMovimentationLocked(true);
            }
            IsVisible = true;
            selectedIndex = contentHolder.transform.childCount - 2; // minus 2 because the prefab is a child and the count start in 1 and the [] in 0

            if (albumPhotos != null && albumPhotos.Count > 0)
            {
                albumPhotos[selectedIndex].button.Select();
                bigImage.color = Vector4.one;
            }
            else
                bigImage.color = Vector4.zero;

            OnOpenAlbum.Invoke();
        }
        else
        {
            if(albumObj)
                albumObj.SetActive(false);
            if (setPlayer)
            {
                PlayerController.instance.SetCameraLocked(false);
                PlayerController.instance.SetMovimentationLocked(false);
            }
            IsVisible = false;
            ResetAlbumPhotoFunction();
            selectedIndex = -1;

            OnCloseAlbum.Invoke();
        }
    }

    private void ClickPhoto(Photo photo)
    {
        if(OnClickInPhoto != null)
            OnClickInPhoto(photo);
    }

    public void SetAlbumPhotoFunction(params Action<Photo>[] call)
    {
        OnClickInPhoto = null;

        for (int i = 0; i < call.Length; i++)
        {
            OnClickInPhoto += call[i];
        }
    }

    public void ResetAlbumPhotoFunction()
    {
        OnClickInPhoto = null;
        //maximize image
    }

    public static void SelectPhoto()
    {
        if (!instance || instance.selectedIndex == -1 || instance.albumPhotos == null)
            return;


        if(instance.albumPhotos.Count -1 >= instance.selectedIndex && instance.albumPhotos[instance.selectedIndex] != null)
            instance.ClickPhoto(instance.albumPhotos[instance.selectedIndex].photo);
    }
}
