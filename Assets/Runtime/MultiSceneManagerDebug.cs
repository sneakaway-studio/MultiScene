using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 *  Just to see serialized data (hidden inside static classes) in MultiSceneManager
 */

namespace SneakawayStudio
{
    public class MultiSceneManagerDebug : MonoBehaviour
    {

        [Header("⚠️ FOR DEBUGGING STATIC FIELDS - DO NOT EDIT! ⚠️")]
        [Space(5)]

        [Tooltip("Data update is enabled")]
        [SerializeField] bool debuggerEnabled = true;

        [Tooltip("Unique id / filename of THIS scene in JSON data")]
        [SerializeField] string managerSceneName;

        [Tooltip("Full path of THIS scene")]
        [SerializeField] string managerScenePath;

        [Tooltip("Manager is loaded")]
        [SerializeField] bool managerSceneLoaded;

        [Tooltip("Default active scene to load")]
        [SerializeField] string defaultActiveSceneName;

        void UpdateManagerScene()
        {
            managerSceneName = MultiSceneManager.managerSceneName;
            managerScenePath = MultiSceneManager.managerScenePath;
            managerSceneLoaded = MultiSceneManager.managerSceneLoaded;
            defaultActiveSceneName = MultiSceneManager.defaultActiveSceneName;
            //sceneLoaded = gameObject.scene.isLoaded;
        }



        [Header("Active Scene")]
        [Space(5)]

        [Tooltip("Unique id / filename of the ACTIVE scene")]
        [SerializeField] string activeSceneName;

        [Tooltip("Full path of ACTIVE scene")]
        [SerializeField] string activeScenePath;

        [Tooltip("Has ACTIVE scene finished loading?")]
        [SerializeField] bool activeSceneLoaded;

        [Tooltip("Scene is ready to start")]
        [SerializeField] bool activeSceneReady;

        [Tooltip("Scene has started")]
        [SerializeField] bool activeSceneStarted;

        [Tooltip("MultiScene script")]
        [SerializeField] MultiScene multiSceneScript;

        void UpdateActiveScene()
        {
            activeSceneName = MultiSceneManager.activeSceneName;
            activeScenePath = MultiSceneManager.activeScenePath;
            activeSceneLoaded = MultiSceneManager.activeSceneLoaded;
            activeSceneReady = MultiSceneManager.activeSceneReady;
            activeSceneStarted = MultiSceneManager.activeSceneStarted;
            multiSceneScript = MultiSceneManager.multiSceneScript;
        }


        [Header("Scene Indexes")]
        [Space(5)]

        [SerializeField] int prevSceneIndex;
        [SerializeField] int activeSceneIndex;
        [SerializeField] int nextSceneIndex;

        void UpdateSceneIndex()
        {
            prevSceneIndex = MultiSceneManager.prevSceneIndex;
            activeSceneIndex = MultiSceneManager.activeSceneIndex;
            nextSceneIndex = MultiSceneManager.nextSceneIndex;
        }

        [Tooltip("All currently loaded scenes")]
        [SerializeField] string[] loadedSceneNames;

        [Tooltip("All build scene names")]
        [SerializeField] List<string> buildSceneNames;



        // OnEnable / OnDisable listeners
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            // cancel when gameobject | scene unloads
            debuggerEnabled = false;
        }

        public void OnManagerValidated()
        {
            UpdateManagerScene();
            // only once, during validation
            buildSceneNames = MultiSceneManager.buildSceneNames;
            UpdateSceneIndex();
        }

        public void OnSceneBusiness()
        {
            UpdateManagerScene();
            UpdateActiveScene();
            // only during play
            loadedSceneNames = MultiSceneManager.loadedSceneNames;
            UpdateSceneIndex();
        }
        public void OnActiveSceneChanged(Scene previousActive, Scene newActive)
            => OnSceneBusiness();
        public void OnSceneLoaded(Scene previousActive, LoadSceneMode mode)
            => OnSceneBusiness();

        private void Awake() => StartCoroutine(UpdateData());

        IEnumerator UpdateData()
        {
            for (; ; )
            {
                if (debuggerEnabled)
                {
                    // Perform some action here
                    //Debug.Log($"MultiSceneManagerDebug.UpdateData()");
                    OnSceneBusiness();
                }
                yield return new WaitForSeconds(.1f);
            }
        }


    }
}