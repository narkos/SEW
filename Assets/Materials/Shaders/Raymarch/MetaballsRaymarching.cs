using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class MetaballsRaymarching : SceneViewFilter
{
    [SerializeField]
    public Shader _shader;

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
    
    public Transform _target;
    public Transform[] _targets;
    private int _targetCount = 0;
    public float _radius;

    public float _drawDistance;
    [Range(1,300)]
    public int _maxIterations;
    [Range(0.1f,0.001f)]
    public float _accuracy;

    public void SetPositions(List<SmoothBodyController> positions)
    {
        if (positions != null && positions.Count > 0)
        {
            if (positions.Count > _targetCount)
            {
                _targets = new Transform[positions.Count];
            }
            for (int i = 0; i < positions.Count; i++)
            {
                _targets[i] = positions[i].transform;
            }
        }
    }

    private void SetVariables()
    {
        _raymarchMaterial.SetMatrix("_frustum", CamFrustum(_camera));
        _raymarchMaterial.SetMatrix("_camToWorld", _camera.cameraToWorldMatrix);
        _raymarchMaterial.SetFloat("_drawDistance", _drawDistance);
        _raymarchMaterial.SetFloat("_accuracy", _accuracy);

        _raymarchMaterial.SetInt("_maxIterations", _maxIterations);

        _raymarchMaterial.SetVector("_position", _target.position);
        _raymarchMaterial.SetFloat("_radius", _radius);

        if (_targets != null && _targets.Length > 0)
        {
            int numberOfObjects = _targets.Length;
            Vector4[] positions = new Vector4[numberOfObjects];
            for(int i = 0; i < numberOfObjects; i++) {

                    positions[i] = _targets[i] ?
                        new Vector4(_targets[i].position.x, _targets[i].position.y, _targets[i].position.z, 1 )
                        : Vector4.zero;

            }
            _raymarchMaterial.SetInt("_transformCount", _targets.Length);
            _raymarchMaterial.SetVectorArray("_transforms", positions);
        }
        //_raymarchMaterial.SetVectorArray("")
    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_raymarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }

        SetVariables();
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
