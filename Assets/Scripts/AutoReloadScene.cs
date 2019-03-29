using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoReloadScene : MonoBehaviour 
{
    public float timer = 60;
    private bool counting;
    private float counter;

    private void Update()
    {
        OnAnyKey();

        if (!counting)
            return;

        counter += Time.deltaTime;
        Reload();
    }

    private void OnAnyKey()
    {
        float movement = Input.GetAxis("Horizontal");
        movement += Input.GetAxis("Vertical");

        if (Input.anyKey || Mathf.Abs(movement) > 0.2f)
        {
            counter = 0;
            counting = true;
        }
    }

    private void Reload()
    {
        if (counter >= timer)
        {
            LoadScreen.StartLoadScene(SceneManager.GetActiveScene().buildIndex, mode: LoadSceneMode.Single);
        }
    }
}
