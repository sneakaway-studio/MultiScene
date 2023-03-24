using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SneakawayStudio;

public class SceneButton : MonoBehaviour
{

    void OnGUI()
    {
        if (GUI.Button(new Rect(150, Screen.height - 100, 100, 50), "<"))
            MultiSceneManager.LoadPreviousScene();

        if (GUI.Button(new Rect(Screen.width - 250, Screen.height - 100, 100, 50), ">"))
            MultiSceneManager.LoadNextScene();
    }
}
