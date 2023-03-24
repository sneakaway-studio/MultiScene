using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SneakawayStudio;

/**
 *  Load a scene by name 
 */

public class SceneLoader : MonoBehaviour
{
    public string builtInScene;
    public bool loadAsAdditive;

    private async void Awake()
    {
        if (builtInScene != "")
        {
            //SceneHelper.LoadScene(builtInScene, loadAsAdditive);
            await MultiSceneManager.LoadSceneAsync(builtInScene, loadAsAdditive, true);
        }
        else
        {
            Debug.LogWarning("SceneLoader.Awake() => builtInScene == empty");
        }
    }

    private void Start() {  /* for inspector */  }

}
