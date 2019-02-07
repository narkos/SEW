using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


[Serializable]
public sealed class Vector4ArrayParameter : ParameterOverride<Vector4[]>
{
    public Vector4ArrayParameter (int count, Vector4[] array) 
    {
        value = new Vector4[count];
        value = array;
    }

    public Vector4ArrayParameter ()
    {
        value = null;
    }

    public int Count()
    {
        Debug.Log(value);
        return value != null ? value.Length : 0;
    }
}


[Serializable]
[PostProcess(typeof(LwrpRaymarchingRenderer), PostProcessEvent.BeforeStack, "MINA/LwrpBasicRaymarch")]
public sealed class LwrpRaymarching : PostProcessEffectSettings
{
    public Vector4Parameter sphere = new Vector4Parameter();
    public Vector4[] targets;
    public Vector4ArrayParameter targetParam = new Vector4ArrayParameter();

    [Range(0.1f, 0.001f), Tooltip("Raymarch Accuracy")]
    public FloatParameter accuracy = new FloatParameter { value = 0.1f};
    
    public FloatParameter drawDistance = new FloatParameter { value = 50.0f};
    [Range(32, 256), Tooltip("Raymarch Max Iterations")]
    public IntParameter maxIterations = new IntParameter { value = 64 };
    public int numberOfTargets = 0;

    public void setTargets(int count, Transform[] newTransforms)
    {
        numberOfTargets = count;
        Debug.Log(numberOfTargets);
        if (targets == null || targets.Length != count)
        {
            targets = new Vector4[count];
        }
        for (int i = 0; i < count; i++)
        {
            targets[i] = newTransforms[i].position;          
            Debug.Log(targets[i]);  
        }
        targetParam = new Vector4ArrayParameter(count, targets);
    }
}

public sealed class LwrpRaymarchingRenderer : PostProcessEffectRenderer<LwrpRaymarching>
{
    public override void Init()
    {

    }

    public override void Render(PostProcessRenderContext context)
    {
        Camera cam = context.camera;
        Transform camTransform = cam.transform;

        Vector3[] frustumCorners = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1),
            cam.farClipPlane, cam.stereoActiveEye, frustumCorners);

        // FRUSTUM
        Vector3 bottomLeft = camTransform.TransformVector(frustumCorners[1]);
        Vector3 topLeft = camTransform.TransformVector(frustumCorners[0]);
        Vector3 bottomRight = camTransform.TransformVector(frustumCorners[2]);

        Matrix4x4 frustumVectorsArray = Matrix4x4.identity;
        frustumVectorsArray.SetRow(0, bottomLeft);
        frustumVectorsArray.SetRow(1, bottomLeft + (bottomRight - bottomLeft) * 2);
        frustumVectorsArray.SetRow(2, bottomLeft + (topLeft - bottomLeft) * 2);

        PropertySheet sheet = context.propertySheets.Get(Shader.Find("MINA/LwrpBasicRaymarch"));
        sheet.properties.SetMatrix("_Frustum", frustumVectorsArray);

        sheet.properties.SetMatrix("_CamToWorld", cam.cameraToWorldMatrix);
        sheet.properties.SetFloat("_DrawDistance", settings.drawDistance);
        sheet.properties.SetFloat("_Accuracy", settings.accuracy);
        sheet.properties.SetInt("_MaxIterations", settings.maxIterations);

        if (settings.sphere != null)
        {
            sheet.properties.SetVector("_Sphere1", settings.sphere);
        }
        // Debug.Log(settings.numberOfTargets);
        // Borde kunna cacha detta
        if (settings.targetParam.Count() > 0) {
            Debug.Log(settings.targetParam.Count());
        }
        if (settings.numberOfTargets > 0)
        {
            //Debug.Log(settings.numberOfTargets.value);
                Debug.Log(settings.numberOfTargets);
            if (settings.targets != null && settings.targets.Length > 0)
            {
                sheet.properties.SetInt("_TargetCount", settings.numberOfTargets);
                sheet.properties.SetVectorArray("_Targets", settings.targets);
            }
        } else {
                sheet.properties.SetInt("_TargetCount", 0);            
        }

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
