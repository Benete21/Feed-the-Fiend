using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Testing_Scene");
    }
    public void Tut()
    {
        SceneManager.LoadScene("Tutorial");
    }
}
