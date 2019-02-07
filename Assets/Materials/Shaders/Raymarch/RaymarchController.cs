using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class RaymarchController : MonoBehaviour
{
    private PostProcessVolume _volume;
    private LwrpRaymarching _raymarcher;

    public Transform _target;
    public Transform[] _targets;

    void Start()
    {

        _raymarcher = ScriptableObject.CreateInstance<LwrpRaymarching>();
        _raymarcher.enabled.Override(true);
        //_raymarcher.sphere 
        _raymarcher.sphere.Override(new Vector4(_target.position.x, _target.position.y, _target.position.z, 1.0f));

        _volume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("PostProcessing"), 100f, _raymarcher);
        if (_targets.Length > 0)
        {
            _raymarcher.setTargets(_targets.Length, _targets);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _raymarcher.sphere.Override(new Vector4(_target.position.x, _target.position.y, _target.position.z, 1.0f));
        
    }

    void OnDestroy()
    {
        RuntimeUtilities.DestroyVolume(_volume, true, true);
    }
}
