using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


[Serializable]
public sealed class Vector4ArrayParameter : ParameterOverride<Vector4[]>
{
    public Vector4ArrayParameter (Vector4[] array) 
    {
        int count = array.Length;
        value = new Vector4[count];
        value = array;
        //Debug.Log(value[0]);
    }

    public Vector4ArrayParameter (int count, Transform[] array) 
    {
        value = new Vector4[count];
        for (int i = 0; i < array.Length; i++)
        {
            value[i] = array[i].position;
        }
    }

    public Vector4ArrayParameter ()
    {
        value = null;
    }

    public int Count()
    {
        return (this).value != null ? value.Length : 0;
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

    public void SetTargets(int count, Transform[] newTransforms)
    {
        if (targets == null || targets.Length != count)
        {
            this.targets = new Vector4[count];
        }
        for (int i = 0; i < count; i++)
        {
            this.targets[i] = newTransforms[i].position;          
        }
        targetParam.Override(targets);
    }
}

public sealed class LwrpRaymarchingRenderer : PostProcessEffectRenderer<LwrpRaymarching>
{
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

        // Set target array
        int targetCount = settings.targetParam.Count();
        if (targetCount > 0)
        {
            Vector4[] targetPositions = settings.targetParam.value;
            if (targetPositions != null)
            {
                sheet.properties.SetInt("_TargetCount", targetCount);
                sheet.properties.SetVectorArray("_Targets", targetPositions);
            }
        } else {
                sheet.properties.SetInt("_TargetCount", 0);            
        }

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
