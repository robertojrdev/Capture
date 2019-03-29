using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    public static Initializer instance;

    public GameObject startKit;
    public Transform playerInitialPosition;

    private void Awake()
    {
        if (instance && instance != this)
            Destroy(this);
        else
            instance = this;

        TiltWithCamera.ResetRectsList();
        InstantiatePlayer();
        StartAmbientSounds();
    }

    private void InstantiatePlayer()
    {
        if (instance.startKit)
        {
            GameObject player = Instantiate(instance.startKit);
            player.SetActive(true);
            player.name = "PLAYER INSTANCE";
            if (instance.playerInitialPosition)
            {
                player.transform.position = instance.playerInitialPosition.position;
                player.transform.rotation = instance.playerInitialPosition.rotation;
            }
        }
    }

    public void StartAmbientSounds()
    {
        SFXProfile profile = new SFXProfile(loop: true, volume: 0.2f);
        SFX.Play("Enviroment_Birds", profile);
    }
}
