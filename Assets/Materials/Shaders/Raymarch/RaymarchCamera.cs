using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RaymarchCamera : SceneViewFilter
{
    [SerializeField]
    private Shader _shader;

    public Material _raymarchMaterial
    {
        get
        {
            if (!_raymarchMat && _shader)
            {
                _raymarchMat = new Material(_shader);
                _raymarchMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return _raymarchMat;
        }
    }

    private Material _raymarchMat;

    public Camera _camera
    {
        get
        {
            if (!_cam)
            {
                _cam = GetComponent<Camera>();
            }
            return _cam;
        }
    }

    private Camera _cam;
    [Header("General Settings")]
    public float _drawDistance;
    [Range(1,300)]
    public int _maxIterations;
    [Range(0.1f,0.001f)]
    public float _accuracy;

    [Header("Directional Light")]
    public Transform _directionalLight;
    public Color _lightColor;
    public float _lightIntensity;


    [Header("Shadow")]
    [Range(0, 4)]
    public float _shadowIntensity = 1;
    public Vector2 _shadowDistance = new Vector2(0.1f, 50);
    [Range(1, 128)]
    public float _shadowPenumbra = 1;

    [Header("Ambient Occlusion")]
    [Range(0.01f, 10.0f)]
    public float _aoStepSize = 0.1f;
    [Range(1, 5)]
    public int _aoIterations = 1;
    [Range(0,1)]
    public float _aoIntensity = 0.2f;

    [Header("Signed Distance Field")]
    public Color _mainColor;
    public Vector4 _sphere1;
    public Vector4 _sphere2;
    public Vector4 _box1;
    public float _box1Roundness;
    public float _boxSphereSmooth;
    public float _sphereIntersectSmooth;


    public Vector3 _modInterval;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_raymarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }
        _raymarchMaterial.SetMatrix("_Frustum", CamFrustum(_camera));
        _raymarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);
        _raymarchMaterial.SetFloat("_drawDistance", _drawDistance);
        _raymarchMaterial.SetFloat("_accuracy", _accuracy);
        _raymarchMaterial.SetInt("_maxIterations", _maxIterations);
        
        _raymarchMaterial.SetFloat("_aoStepSize", _aoStepSize);
        _raymarchMaterial.SetFloat("_aoIntensity", _aoIntensity);
        _raymarchMaterial.SetInt("_aoIterations", _aoIterations);
        
        _raymarchMaterial.SetColor("_mainColor", _mainColor);
        
        _raymarchMaterial.SetVector("_lightDir", _directionalLight ? _directionalLight.forward : Vector3.down);
        _raymarchMaterial.SetColor("_lightColor", _lightColor);
        _raymarchMaterial.SetFloat("_lightIntensity", _lightIntensity);


        _raymarchMaterial.SetVector("_shadowDistance", _shadowDistance);
        _raymarchMaterial.SetFloat("_shadowIntensity", _shadowIntensity);
        _raymarchMaterial.SetFloat("_shadowPenumbra", _shadowPenumbra);

        _raymarchMaterial.SetFloat("_sphereIntersectSmooth", _sphereIntersectSmooth);
        _raymarchMaterial.SetVector("_sphere1", _sphere1);
        _raymarchMaterial.SetVector("_sphere2", _sphere2);
        _raymarchMaterial.SetVector("_box1", _box1);
        _raymarchMaterial.SetFloat("_box1Round", _box1Roundness);
        _raymarchMaterial.SetFloat("_boxSphereSmooth", _boxSphereSmooth);
        _raymarchMaterial.SetVector("_modInterval", _modInterval);


        RenderTexture.active = destination;

        _raymarchMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        _raymarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        // BL
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);

        // BR
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);

        // TR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);

        // TL
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 CamFrustum(Camera cam)
    {
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float fovWHalf = fov * 0.5f;
        float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
        //float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);

        Vector3 goRight = Vector3.right * tan_fov * aspect;
        Vector3 goUp = Vector3.up * tan_fov;

        Vector3 TL = (-Vector3.forward - goRight + goUp);
        Vector3 TR = (-Vector3.forward + goRight + goUp);

        Vector3 BR = (-Vector3.forward + goRight - goUp);
        Vector3 BL = (-Vector3.forward - goRight - goUp);

        //TL.w = TR.w = BR.w = BL.w = 1.0f;

        frustum.SetRow(0, TL);
        frustum.SetRow(1, TR);
        frustum.SetRow(2, BR);
        frustum.SetRow(3, BL);


        return frustum;
    }
}
