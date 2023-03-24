using System.Threading.Tasks;
using UnityEngine;
using CustomExtensions;

/**
 *  MultiScene - Attached to prefab in each ChapterScene 
 *  - Awake() => check for (and load if not found) Managers scene, which takes control
 *  - To disable Managers loading just disable this GO
 */

namespace SneakawayStudio
{
    public class MultiScene : MonoBehaviour
    {
        string cName = "MultiScene";

        [Tooltip("Manager has loaded")]
        public bool managerLoaded;

        [Header("THIS Scene")]

        [Tooltip("Unique id / filename of THIS scene in JSON data")]
        public string sceneName;

        [Tooltip("Full path of THIS scene")]
        public string scenePath;

        [Tooltip("Scene finished loading")]
        public bool sceneLoaded;

        [Tooltip("Scene finished loading AND is valid")]
        public bool sceneValid;

        // and finally ...

        [Tooltip("Scene is ready to start")]
        public bool sceneReady;

        [Tooltip("Scene has started")]
        public bool sceneStarted;


        ////////////////////////////////////////////////////// 
        //////////////////////// INIT //////////////////////// 
        ////////////////////////////////////////////////////// 

        private void OnValidate()
        {
            sceneName = gameObject.scene.name;
            scenePath = gameObject.scene.path;
            UpdateBasicSceneData();
        }
        private void Awake() => UpdateBasicSceneData();
        void UpdateBasicSceneData()
        {
            sceneLoaded = gameObject.scene.isLoaded;
            sceneValid = gameObject.scene.IsValid();
            sceneReady = sceneLoaded && sceneValid;
        }
        void OnEnable() => StartupChecks();
        public async void StartupChecks()
        {
            Debug.Log($"{cName}.StartupChecks() [0] managerLoaded={managerLoaded}, sceneReady={sceneReady}".Yellow1());
            managerLoaded = await MultiSceneManager.LoadManagerSceneAsync();
            sceneReady = await CheckSceneIsReadyAsync();
            Debug.Log($"{cName}.StartupChecks() [1] managerLoaded={managerLoaded}, sceneReady={sceneReady}".Yellow2());
            sceneStarted = true;
            MultiSceneManager.activeSceneStarted = true;
            // DONE => Now some other listener should watch ^ and start game
        }
        private void Start() { /* show in inspector */ }



        public async Task<bool> CheckSceneIsReadyAsync(bool sceneReady = false, int attempts = 0)
        {
            string mName = "CheckSceneIsReadyAsync";

            // while not complete
            while (!sceneReady)
            {
                Debug.Log($"{cName}.{mName}() => sceneReady={sceneReady}, sceneLoaded={sceneLoaded}, sceneValid={sceneValid}, attempts={attempts}".Yellow2());
                sceneLoaded = gameObject.scene.isLoaded;
                sceneValid = gameObject.scene.IsValid();
                sceneReady = sceneLoaded && sceneValid;

                if (++attempts > 100) // safety
                {
                    Debug.LogWarning($"{cName}.{mName}() [X] => SAFETY FIRST!".Red());
                    break;
                }
                await Task.Delay(5); // milliseconds   
            }
            return sceneReady;
        }

        public async Task<bool> ExampleAsync()
        {
            await Task.Delay(1000); // simple example to return after 1 second (1000 milliseconds) delay
            return true;
        }



    }
}