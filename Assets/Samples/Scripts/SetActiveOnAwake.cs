using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 *  Set this, or another game object active or inactive on awake
 *  - good for prefabs left out, or hiding "pop up" UIs when not in use.
 */

public class SetActiveOnAwake : MonoBehaviour
{
    [Tooltip("if obj is not set default to this")]
    public GameObject obj;
    public bool setActive;
    public string prefabName;

    private void Awake()
    {
        // if obj is not set default to this
        if (obj == null) obj = gameObject;
        //Debug.Log("SetActiveOnAwake [1] " + obj.name);
        // for using to hide left out prefabs
        if (prefabName != "" && obj.name != prefabName) return;
        //Debug.Log("SetActiveOnAwake [2] " + obj.name);
        // set based on bool in inspector
        obj.SetActive(setActive);
    }
}
