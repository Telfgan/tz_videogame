using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public void RestartGame()
    {
        // Получаем индекс текущей сцены
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Загружаем текущую сцену заново
        SceneManager.LoadScene(currentSceneIndex);
    }
}
