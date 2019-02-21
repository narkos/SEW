using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class RaymarchController : MonoBehaviour
{
    private PostProcessVolume _volume;
    private PostProcessProfile _profile;
    private LwrpRaymarching _raymarcher;

    public Transform _target;
    public Transform[] _targets;

    void Start()
    {

        
        
        _raymarcher = ScriptableObject.CreateInstance<LwrpRaymarching>();
        _raymarcher.hideFlags = HideFlags.DontSave;
        _raymarcher.enabled.Override(true);
        _raymarcher.sphere.Override(new Vector4(_target.position.x, _target.position.y, _target.position.z, 1.0f));

        if (_targets.Length > 0)
        {
            _raymarcher.SetTargets(_targets.Length, _targets);
        }

        _profile = ScriptableObject.CreateInstance<PostProcessProfile>();
        _profile.hideFlags = HideFlags.DontSave;
        _profile.AddSettings(_raymarcher);

        _volume = gameObject.AddComponent<PostProcessVolume>();
        _volume.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        _volume.sharedProfile = _profile;
        _volume.isGlobal = true;
        _volume.priority = 1000;

        //_volume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("PostProcessing"), 100f, _raymarcher);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _raymarcher.sphere.Override(new Vector4(_target.position.x, _target.position.y, _target.position.z, 1.0f));
        if (_targets.Length > 0)
        {
            _raymarcher.SetTargets(_targets.Length, _targets);
            //_raymarcher.targetParam = new Vector4ArrayParameter(_targets.Length, _targets);
        }
    }

    void OnDestroy()
    {
        Destroy(_raymarcher);
        RuntimeUtilities.DestroyProfile(_profile, true);
        RuntimeUtilities.DestroyVolume(_volume, true, true);
    }
    // void Start()
    // {

    //     _raymarcher = ScriptableObject.CreateInstance<LwrpRaymarching>();
    //     _raymarcher.enabled.Override(true);
    //     //_raymarcher.sphere 
    //     _raymarcher.sphere.Override(new Vector4(_target.position.x, _target.position.y, _target.position.z, 1.0f));

    //     _volume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("PostProcessing"), 100f, _raymarcher);
    //     if (_targets.Length > 0)
    //     {
    //         _raymarcher.setTargets(_targets.Length, _targets);
    //     }
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     _raymarcher.sphere.Override(new Vector4(_target.position.x, _target.position.y, _target.position.z, 1.0f));
        
    // }

    // void OnDestroy()
    // {
    //     RuntimeUtilities.DestroyVolume(_volume, true, true);
    // }
}
