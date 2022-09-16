using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameButton: MonoBehaviour
{
    public void GoBackToSelectionMenu()
    {
        SceneManager.LoadScene("LevelSelection");
    }
}
