using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameState gameState { get; private set; }
    public InGameMenuUI pauseMenu;

    public static List<Puzzle> puzzles = new List<Puzzle>();
    public Interactables LookingInteractable { get; private set; }
    public static bool IsInteracting { get; private set; }
    public enum GameState
    {
        Playing, Paused
    }
    
    private void Awake()
    {
        if(instance && instance != this)
        {
            Destroy(this);
            Debug.LogError("Multiple GameManager in the scene");
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        GameCamera.instance.OnTakePhoto_Event += OnPhoto;

        Cursor.lockState = CursorLockMode.Locked;

        gameState = GameState.Playing;
    }

    private void Update()
    {
        GetInputs();
        Cheats();
        //CheckPuzzles();
        CheckForInteractables();
    }

    private void GetInputs()
    {
        if(Input.GetButtonDown("Pause"))
        {
            if (gameState.Equals(GameState.Playing))
                SetGameState(GameState.Paused);
            else if (gameState.Equals(GameState.Paused))
                SetGameState(GameState.Playing);
        }

    }

    public void SetGameState(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                pauseMenu.gameObject.SetActive(false);
                PlayerController.instance.SetCameraLocked(false);
                PlayerController.instance.SetMovimentationLocked(false);
                GameCamera.SetCameraLocked(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                gameState = state;
                break;

            case GameState.Paused:
                pauseMenu.ResetMenu();
                pauseMenu.gameObject.SetActive(true);
                PlayerController.instance.SetCameraLocked(true);
                PlayerController.instance.SetMovimentationLocked(true);
                GameCamera.SetActiveCamera(false, true);
                Album.instance.SetAlbumVisible(false, false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                gameState = state;
                break;
        }
    }

    private void OnPhoto(Photo photo)
    {
        List<Puzzle> activePuzzles = GetActivePuzzles();

        for(int i = 0; i < activePuzzles.Count; i++)
        {
            if(activePuzzles[i].UseReceiver)
                photo.resolvablesPuzzles.Add(activePuzzles[i]);
                activePuzzles[i].TryToSolve();
        }
    }

    private void Cheats()
    {
        //reload scene
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        //activate or deactivate freecamera
        if (Input.GetKeyDown(KeyCode.C))
        {
            FreeCamera.Activate(!FreeCamera.IsActive, GameCamera.instance.transform);
            PlayerController.SetActive(!FreeCamera.IsActive);
        }
    }

    private List<Puzzle> GetActivePuzzles()
    {
        List <Puzzle> activePuzzles = new List<Puzzle>(); 
        
        for (int i = 0; i < puzzles.Count; i++)
        {
            if (puzzles[i].IsInPuzzle(GameCamera.instance.transform))
                activePuzzles.Add(puzzles[i]);
        }

        return activePuzzles;
    }

    private void CheckForInteractables()
    {
        Ray ray = GameCamera.instance.Cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            LookingInteractable = hit.transform.GetComponent<Interactables>();
            if (LookingInteractable != null)
                UIManager.instance.ShowInteractionText(hit.transform.name);
            else
                UIManager.instance.HideInteractionText();
        }
        else
        {
            LookingInteractable = null;
            UIManager.instance.HideInteractionText();
        }
    }

    public void Interact()
    {
        if(!IsInteracting && !GameCamera.instance.IsActive && LookingInteractable != null && !Album.instance.IsVisible)
        {
            LookingInteractable.Interact();
        }
    }

    public void OnStopInteract()
    {
        IsInteracting = false;
    }
}
