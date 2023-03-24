using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.IO;
using CustomExtensions;

/**
 *  MultiSceneManager 
 *  - Manager for scene (un)loading, listen for updates, display loading screen 
 *  - Part of Manager scene, loaded by MultiScene  
 *  - In charge of data and updates, with MultiSceneMgrDebug only showing the data
 */

namespace SneakawayStudio
{

    public class MultiSceneManager : MonoBehaviour
    {
        static string cName = "MultiSceneManager";


        ////////////////////////////////////////////////////// 
        ////////////////// INSTANCE MEMBERS //////////////////
        ////////////////////////////////////////////////////// 

        MultiSceneManagerDebug multiSceneManagerDebug;

        public Range sceneLimits = new Range(2, 2);



        ////////////////////////////////////////////////////// 
        /////////////////// STATIC MEMBERS ///////////////////
        // - Available everywhere w/o instance reference - //
        //////////////////////////////////////////////////////

        // ⚠️ EDIT THESE ITEMS TO MATCH YOUR PROJECT ⚠️ //

        // prefix and suffix to remove from paths
        public static string scenePathPrefix = "Assets/Scenes/";
        public static string scenePathSuffix = ".unity";

        // MANAGER SCENE
        public static bool managerSceneLoaded = false;
        // name and path of manager scene
        public static string managerSceneName = "ManagerScene";
        public static string managerScenePath = scenePathPrefix + managerSceneName;
        // default active scene to load
        public static string defaultActiveSceneName = "Scene-0-0";






        // ACTIVE SCENE
        public static Scene activeScene; // Scenes can't be serialized
        public static string activeSceneName; // but strings can
        public static string activeScenePath;
        public static bool activeSceneLoaded;
        public static bool activeSceneReady;
        public static bool activeSceneStarted;
        public static MultiScene multiSceneScript;

        // SCENE INDEXES
        public static int prevSceneIndex;
        public static int activeSceneIndex;
        public static int nextSceneIndex;

        // SCENES LOADED
        //public static int loadedScenesCount; // scenes currently loaded
        public static List<string> loadedSceneNames;

        // SCENES IN BUILD
        //public static int buildScenesCount; // scenes currently loaded
        public static List<string> buildSceneNames;




        private void OnValidate()
        {
            UpdateScenesInBuild();
            activeSceneName = GetActiveSceneName();
            if (multiSceneManagerDebug == null) multiSceneManagerDebug = GetComponent<MultiSceneManagerDebug>();
            multiSceneManagerDebug.OnManagerValidated();
        }

        private void Awake()
        {
            // inform other GameObjects that Manager has loaded
            managerSceneLoaded = true;

            // Check for an active scene, load default if not found
            CheckForLoadActiveScene();

            UpdateSceneIndexesFromBuild();
        }




        ////////////////////////////////////////////////////// 
        ///////////////////// LISTENERS //////////////////////
        //////////////////////////////////////////////////////


        // OnEnable / OnDisable listeners
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// When any scene has loaded (notified using delegate)
        /// </summary>
        async void OnSceneLoaded(Scene newScene, LoadSceneMode mode)
        {
            Debug.Log($"{cName}.OnSceneLoaded() [1] scene.name={newScene.name}, mode={mode}, activeSceneName={activeSceneName}".Peach1());
            await UpdateInfoAfterSceneBusiness("OnSceneLoaded");
        }

        /// <summary>
        /// When the active scene has changed (after OnSceneLoaded) (called from delegate, etc.)
        /// </summary>
        async void OnActiveSceneChanged(Scene previousActive, Scene newActive)
        {
            Debug.Log($"{cName}.OnActiveSceneChanged() [1] previousActive='{previousActive.name}', newActive={newActive.name}".Peach1());

            if (previousActive.name != "" && IsValidSceneName(previousActive.name) && !IsManagerScene(previousActive.name))
                // unload the previously active scene
                await UnloadSceneAsync(previousActive.name);
            else
                Debug.LogError("There was no previousActive.name");

            await UpdateInfoAfterSceneBusiness("OnActiveSceneChanged");
        }

        /// <summary>
        /// When any scene has unloaded (notified using delegate)
        /// </summary>
        async void OnSceneUnloaded(Scene current)
        {
            Debug.Log($"{cName}.OnSceneUnloaded() [1] current.name='{current.name}', activeSceneName={activeSceneName}".Peach1());

            // update after a short delay to prevent errors
            await Task.Delay(5);

            await UpdateInfoAfterSceneBusiness("OnSceneUnloaded");

            // update reference to script (may need this one day, doesn't yet work though)
            //multiSceneScript = GameObject.FindGameObjectWithTag("MultiSceneGameObject").GetComponent<MultiScene>();
        }








        ////////////////////////////////////////////////////// 
        //////////////////// SCENE INDEX /////////////////////
        //////////////////////////////////////////////////////

        /// <summary>Load previous scene</summary>
        public static async void LoadPreviousScene()
        {
            //Debug.Log($"{cName}.LoadPreviousScene() -> activeSceneIndex={activeSceneIndex}/nextSceneIndex={nextSceneIndex}".Peach1());
            await LoadSceneAsync(GetSceneNameFromBuild(prevSceneIndex), true, true);
        }
        /// <summary>Load next scene</summary>
        public static async void LoadNextScene()
        {
            //Debug.Log($"{cName}.LoadNextScene() -> activeSceneIndex={activeSceneIndex}/nextSceneIndex={nextSceneIndex}".Peach1());
            await LoadSceneAsync(GetSceneNameFromBuild(nextSceneIndex), true, true);
        }





        ////////////////////////////////////////////////////// 
        //////////////////// LOAD SCENES /////////////////////
        //////////////////////////////////////////////////////

        /// <summary>
        /// Check for an active scene, load default if not found
        /// </summary>
        /// <returns></returns>
        async void CheckForLoadActiveScene()
        {
            Debug.Log($"{cName}.CheckForLoadActiveScene() [0]".Yellow1());
            await Task.Delay(1);

            Debug.Log($"{cName}.CheckForLoadActiveScene() [1]".Yellow2());
            // are there too few scenes?
            if (SceneManager.sceneCount < sceneLimits.min)
                // load the multi scene, make active
                await LoadSceneAsync(defaultActiveSceneName, true, true);

            Debug.Log($"{cName}.CheckForLoadActiveScene() [2] {GetActiveSceneName()} ?? {managerSceneName}".Yellow3());

            AlwaysSetNonManagerActive();
        }




        /// <summary>
        /// Load manager scene
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> LoadManagerSceneAsync()
        {
            string mName = "LoadManagerSceneAsync";
            Debug.Log($"{cName}.{mName}() [0] => managerSceneName={managerSceneName}, managerSceneLoaded={managerSceneLoaded}".Yellow1());
            int attempts = 0;
            // while not complete
            while (!managerSceneLoaded)
            {
                // wait 5 milliseconds to give manager time to report it is loaded
                await Task.Delay(5);
                Debug.Log($"{cName}.{mName}() [1] => managerSceneName={managerSceneName}, managerSceneLoaded={managerSceneLoaded}, attempts={attempts}".Yellow2());

                // attempt
                if (!managerSceneLoaded && attempts < 1)
                {
                    Debug.Log($"{cName}.{mName}() [2] => ATTEMPTING LOAD".Yellow3());
                    // if not then load Manager scene but DO NOT set active
                    await LoadSceneAsync(managerSceneName, true, false);
                }
                if (++attempts > 100) // safety
                {
                    Debug.LogWarning($"{cName}.{mName}() [X] => SAFETY FIRST!".Red());
                    break;
                }
                await Task.Delay(5); // milliseconds
            }
            return managerSceneLoaded;
        }

        public static void LoadScene(string _name)
        {
            LoadSceneAsync(_name, true, true);
        }


        /// <summary>
        /// Load a scene by name (use for gameManagers or any multi scene)
        /// - See also SceneHelper.LoadScene https://gist.github.com/kurtdekker/862da3bc22ee13aff61a7606ece6fdd3
        /// </summary>
        public static async Task<bool> LoadSceneAsync(string _name, bool _additive, bool _setActive)
        {
            string mName = "LoadSceneAsync";
            Debug.Log($"{cName}.{mName}('{_name}') _additive={_additive}, _setActive={_setActive}".Peach1());

            // quit if empty or null
            if (!IsValidSceneName(_name))
            {
                Debug.LogWarning($"{cName}.{mName}('{_name}') => NOT A VALID SCENE NAME".Peach2());
                return false;
            }

            // quit if the same scene
            if (IsSceneLoaded(_name))
            {
                Debug.LogWarning($"{cName}.{mName}('{_name}') => ALREADY LOADED".Peach2());
                //ReloadCurrentSceneAsync(); // just call it the right way, don't use this
                return false;
            }

            // start loading scene
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_name, _additive ? LoadSceneMode.Additive : 0);
            int safety = 0;
            // wait until the scene is fully unloaded
            while (!asyncOperation.isDone)
            {
                // check if done 
                Debug.Log($"{cName}.{mName}('{_name}') safety={safety} // progress={asyncOperation.progress}".Peach3());
                if (++safety > 100)
                {
                    Debug.LogError($"{cName}.{mName}('{_name}') SAFETY FIRST!!!".Red());
                    break;
                }
                await Task.Delay(10); // milliseconds  
            }
            if (asyncOperation.isDone)
            {
                // complete loading
                if (_setActive) await SetSceneActive(_name);
                //HideLoadingPanel(); // if using this
                Debug.Log($"{cName}.{mName}('{_name}') asyncOperation.isDone={asyncOperation.isDone}".Peach4());
                return true;
            }
            //else
            Debug.LogError($"{cName}.{mName}('{_name}') COULD NOT LOAD THE SCENE".Red());
            return false;
        }
        public static async Task<bool> SetSceneActive(string _name)
        {
            string mName = "SetSceneActive";
            Debug.Log($"{cName}.{mName}('{_name}') [0]".Peach1());

            // wait for a frame to load to mark it active 
            await Task.Delay(10); // milliseconds  
            Debug.Log($"{cName}.{mName}('{_name}') [1]".Peach2());
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_name));
            Debug.Log($"{cName}.{mName}('{_name}') [2]".Peach3());
            return true;
        }

        /// <summary>
        /// Reload current scene - All reloads must run through this, don't try to load
        /// </summary>
        public static async void ReloadCurrentSceneAsync()
        {
            // 1. store the current active name
            string sceneToReload = GetActiveSceneName();
            Debug.Log($"{cName}.ReloadCurrentSceneAsync() [1] => sceneToReload={sceneToReload} // GetActiveSceneName()={GetActiveSceneName()}".Peach1());

            // 2. create a fake scene and set active - this will cause the delegate to unload the previous active (the one we want to unload)
            Scene newScene = SceneManager.CreateScene("ReloadCurrentSceneDummy");
            SceneManager.SetActiveScene(newScene);
            Debug.Log($"{cName}.ReloadCurrentSceneAsync() [2] => sceneToReload={sceneToReload} // GetActiveSceneName()={GetActiveSceneName()}".Peach2());

            // 3. load the scene again
            bool sceneLoaded = await LoadSceneAsync(activeSceneName, true, true);
            Debug.Log($"{cName}.ReloadCurrentSceneAsync() [3] => sceneToReload={sceneToReload} // GetActiveSceneName()={GetActiveSceneName()}".Peach3());
        }

        /// <summary>
        /// Unload a scene (by name) async
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        public static async Task<bool> UnloadSceneAsync(string _name = "")
        {
            Debug.Log($"{cName}.UnloadSceneAsync({_name}) [0]".Peach1());

            // make sure it isn't empty or null
            if (!IsValidSceneName(_name)) return false;
            // never unload the manager scene
            if (IsManagerScene(_name)) return false;

            Debug.Log($"{cName}.UnloadSceneAsync({_name}) [1]".Peach2());

            // start loading scene
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(_name);
            int safety = 0;
            // wait until the scene is fully unloaded
            while (!asyncOperation.isDone)
            {
                // check if done 
                Debug.Log($"{cName}.UnloadSceneAsync({_name}) [2] safety={safety} // progress={asyncOperation.progress}".Peach3());
                if (++safety > 100)
                {
                    Debug.LogError($"{cName}.UnloadSceneAsync({_name}) SAFETY FIRST!!!".Red());
                    break;
                }
                await Task.Delay(5); // milliseconds  
            }
            if (asyncOperation.isDone)
            {
                Debug.Log($"{cName}.UnloadSceneAsync('{_name}') [3.0] -> asyncOperation.isDone={asyncOperation.isDone}".Peach4());
                return true;
            }
            //else
            Debug.LogError($"{cName}.UnloadSceneAsync({_name}) [3.1] -> COULD NOT UNLOAD THE SCENE".Red());
            return false;
        }




        ////////////////////////////////////////////////////// 
        ////////////////// GET SCENE DATA ////////////////////
        //////////////////////////////////////////////////////

        /// <summary>Returns true if the scene is loaded AND valid</summary>
        public static bool IsSceneLoaded(string _name)
        {
            Scene scene = SceneManager.GetSceneByName(_name);
            Debug.Log($"{cName}.IsSceneLoaded('{_name}') scene.isLoaded={scene.isLoaded}, scene.IsValid()={scene.IsValid()}".Peach1());
            return (scene.isLoaded && scene.IsValid());
        }
        /// <summary>Returns true if valid name</summary>
        public static bool IsValidSceneName(string _name)
        {
            if (_name == null || _name == "")
            {
                Debug.LogWarning($"{cName}.IsValidSceneName('{_name}') => _name is NULL || EMPTY".Peach1());
                return false;
            }
            return true;
        }
        /// <summary>Returns true if _name is the manager scene</summary>
        public static bool IsManagerScene(string _name)
        {
            if (!_name.Contains(managerSceneName)) return false;
            // else
            Debug.LogError($"{cName}.IsManagerScene('{_name}') => TRUE => DO NOT UNLOAD THE MANAGER SCENE!".Peach1());
            return true;
        }

        /// <summary>Get the active scene name (string)</summary>
        public static string GetActiveSceneName() => SceneManager.GetActiveScene().name;

        /// <summary>Get the active scene build index (int)</summary>
        public static int GetActiveSceneBuildIndex() => SceneManager.GetActiveScene().buildIndex;



        ////////////////////////////////////////////////////// 
        //////////////// UPDATE SCENE DATA ///////////////////
        //////////////////////////////////////////////////////

        /// <summary>
        /// Update details on active scene
        /// </summary>
        public async static Task<bool> UpdateInfoAfterSceneBusiness(string caller = "")
        {
            // active scene
            activeScene = SceneManager.GetActiveScene();
            activeSceneName = activeScene.name;
            activeSceneIndex = activeScene.buildIndex;
            activeScenePath = activeScene.path;
            activeSceneLoaded = activeScene.isLoaded;
            // update total
            //loadedScenesCount = SceneManager.sceneCount;

            await Task.Delay(1); // milliseconds  

            // set prev, current, next indexes from the list data
            UpdateSceneIndexesFromBuild();

            // refresh list of loaded scene names
            UpdateLoadedSceneNames(caller);

            return true;
        }

        /// <summary>Find the current, next, and previous index of the current active scene</summary>
        public static void UpdateSceneIndexesFromBuild()
        {
            AlwaysSetNonManagerActive();

            //if (buildSceneNames == null || buildSceneNames.Count < 1) return;

            Debug.Log($"{cName}.UpdateSceneIndexesFromBuild() [0] => activeSceneName = {activeSceneName}; activeSceneIndex = {activeSceneIndex}; buildSceneNames.Count = {buildSceneNames.Count}".Peach1());

            activeSceneIndex = buildSceneNames.IndexOf(activeSceneName);

            Debug.Log($"\t{cName}.UpdateSceneIndexesFromBuild() [1] => activeSceneName = {activeSceneName}; activeSceneIndex = {activeSceneIndex}; buildSceneNames.Count = {buildSceneNames.Count}".Peach1());

            nextSceneIndex = activeSceneIndex < (buildSceneNames.Count - 1) ? activeSceneIndex + 1 : buildSceneNames.Count - 1;
            prevSceneIndex = activeSceneIndex > 0 ? activeSceneIndex - 1 : 0;

            Debug.Log($"\t{cName}.UpdateSceneIndexesFromBuild() [2] => activeSceneName = {activeSceneName}; activeSceneIndex = {activeSceneIndex}; buildSceneNames.Count = {buildSceneNames.Count}".Peach1());
        }




        ////////////////////////////////////////////////////// 
        ////////////////// SET SCENE DATA ////////////////////
        //////////////////////////////////////////////////////

        /// <summary>
        /// The Manager scene should never be active
        /// </summary>
        static async void AlwaysSetNonManagerActive()
        {
            if (GetActiveSceneName() == managerSceneName)
            {
                int mgrIndex = loadedSceneNames.IndexOf(managerSceneName);
                if (mgrIndex == 0)
                    await SetSceneActive(loadedSceneNames[1]);
                else
                    await SetSceneActive(loadedSceneNames[0]);
            }
        }

        /// <summary>Set a scene active via its reference (Scene)</summary>
        public bool SetActiveScene(Scene _scene)
        {
            if (!_scene.IsValid()) return false;
            // else 
            SceneManager.SetActiveScene(_scene);
            return true;
        }
        /// <summary>Set a scene active via it name (string)</summary>
        public bool SetActiveSceneByName(string _name)
        {
            return SetActiveScene(SceneManager.GetSceneByName(_name));
        }
        /// <summary>not sure why I needed to get the name w/o Unity, maybe for comparison</summary>
        public static string GetSceneName(string _name)
        {
            // e.g. retrieve "gameManagers" from "Assets/_Project/Scenes/Managers.unity" 
            if (_name.Contains(".unity"))
                _name = GetFileNameWithoutExtension(_name);
            return _name;
        }









        //static void MarkCorrectSceneActive()
        //{
        //    Debug.Log($"{cName}.MarkCorrectSceneActive() [1] GetActiveSceneName()={GetActiveSceneName()}");
        //    // get array 
        //    Scene[] _loadedScenes = ReturnLoadedScenes();
        //    // loop 
        //    for (int i = 0; i < _loadedScenes.Length; i++)
        //    {
        //        // if not Manager
        //        if (_loadedScenes[i].name != managerSceneName)
        //        {
        //            if (_loadedScenes[i].isLoaded && _loadedScenes[i].IsValid())
        //            {
        //                SceneManager.SetActiveScene(_loadedScenes[i]);
        //            }
        //        }
        //    }
        //    // after fix?
        //    Debug.Log($"{cName}.MarkCorrectSceneActive() [2] GetActiveSceneName()={GetActiveSceneName()}");
        //}


        /// <summary>
        /// Update the names in the array
        /// </summary>
        static void UpdateLoadedSceneNames(string caller = "")
        {
            string mName = "UpdateLoadedSceneNames";
            Debug.Log($"{cName}.{mName}() [1] GetActiveSceneName()={GetActiveSceneName()}".Peach1());

            // reset
            loadedSceneNames = new List<string>();
            // get array 
            Scene[] _loadedScenes = GetLoadedScenes();

            string str = $" [via {caller}]=> "; // TEST
                                                // loop through loaded scenes
            for (int i = 0; i < _loadedScenes.Length; i++)
            {
                loadedSceneNames.Add(_loadedScenes[i].name);
                str += $"{i}. {_loadedScenes[i].name} ";  // TEST
            }
            Debug.Log($"{cName}.{mName}() {str} // GetActiveSceneName()={GetActiveSceneName()}".Peach2());  // TEST
        }

        /// <summary>
        /// Return all the loaded scenes
        /// </summary>
        static Scene[] GetLoadedScenes()
        {
            //string mName = "GetLoadedScenes";
            int loadedScenesCount = SceneManager.sceneCount;
            //Debug.Log($"{cName}.{mName}() => loadedScenesCount={loadedScenesCount}".Peach1());

            Scene[] _loadedScenes = new Scene[loadedScenesCount];

            //string str = $"{cName}.{mName}() => loadedScenesCount={loadedScenesCount} // ";
            for (int i = 0; i < loadedScenesCount; i++)
            {
                _loadedScenes[i] = SceneManager.GetSceneAt(i);
                //str += $"{i}. {_loadedScenes[i].name} ";
            }
            //Debug.Log($"{str} // GetActiveSceneName()={GetActiveSceneName()}".Peach2());
            return _loadedScenes;
        }





        ////////////////////////////////////////////////////// 
        //////////////////// BUILD SCENES //////////////////// 
        ////////////////////////////////////////////////////// 

        /// <summary>
        /// Get a sceneAsset (string) by its index in the data
        /// </summary>
        public static string GetSceneNameFromBuild(int index)
        {
            if (index > buildSceneNames.Count - 1) return "";
            return buildSceneNames[index];
        }
        public static async Task<List<string>> UpdateBuildScenesAsync()
        {
            return await Task.Run(() => UpdateScenesInBuild());
        }
        public static List<string> UpdateScenesInBuild()
        {
            buildSceneNames = GetScenesInBuild();
            return buildSceneNames;
        }
        public static List<string> GetScenesInBuild()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            List<string> scenes = new List<string>();
            for (int i = 0; i < sceneCount; i++)
            {
                string str = SceneUtility.GetScenePathByBuildIndex(i)
                    .Replace(scenePathPrefix, "")
                    .Replace(scenePathSuffix, "");
                scenes.Add(str);
                //Debug.Log($"GetScenesInBuild() {str}");
            }
            return scenes;
        }


        ////////////////////////////////////////////////////// 
        //////////////////// FILE HELPERS //////////////////// 
        ////////////////////////////////////////////////////// 

        public static string GetFileName(string _str = "")
        {
            if (_str == "") return "";
            //string filename = Regex.Match(_str, @"[^\/]*$").Value;
            string filename = Path.GetFileName(_str);
            Debug.Log(filename);
            return filename;
        }
        public static string GetFileNameWithoutExtension(string _str = "")
        {
            string filename = Path.GetFileNameWithoutExtension(_str);
            Debug.Log(filename);
            return filename;
        }


    }


    /// <summary>
    /// Range struct with min/max
    /// </summary>
    [System.Serializable]
    public struct Range
    {
        public float min;
        public float max;
        public Range(float _min, float _max)
        {
            min = _min;
            max = _max;
        }
    }




    ////////////////////////////////////////////
    //////////////// Data //////////////////////
    ////////////////////////////////////////////

    // Note, that scenes are based on build settings now
    // but they might be included in manager to add spreadsheet output

    //// has the data for the game loaded?
    //public static bool chapterSceneDataLoaded;

    //// the data for all ChapterScenes
    //public static Dictionary<string, ChapterScenePoint> chapterScenesDict;

    //// a list of ALL the chapterSceneAssets (in order)
    ////public static List<string> chapterSceneAssetsListFull;

    // THIS ONE HAS BEEN REPLACED
    //// JUST the sceneAssets in the build (in order)
    //public static List<string> sceneAssetsInBuild;

    //// whether to show loading panel or not
    //public static bool displayLoadingPanel;

    ///// <summary>
    ///// Class for each scene (and its chapter) in the game 
    ///// </summary>
    //[System.Serializable]
    //public class ChapterScenePoint
    //{
    //    public int chapterNumber { get; set; }
    //    public string chapterName { get; set; }
    //    public string chapterFolder { get; set; }

    //    public int sceneNumber { get; set; }
    //    public string sceneName { get; set; }
    //    public string sceneType { get; set; }
    //    public string sceneAsset { get; set; } // unique, also the key
    //    public string sceneAssetPath { get; set; }

    //    public int status { get; set; }
    //    public bool restartOk { get; set; }

    //    //public string sceneDescription { get; set; }
    //}

}