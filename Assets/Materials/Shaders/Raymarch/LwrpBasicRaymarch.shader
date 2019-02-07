Shader "MINA/LwrpBasicRaymarch"
{
    HLSLINCLUDE
        #pragma target 3.0

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        #define UNITY_MATRIX_MVP mul(unity_MatrixVP, unity_ObjectToWorld)
        #include "HLSLSupport.cginc"
        #include "DistanceFunctions.cginc"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4x4    _Frustum;
        float4x4    _CamToWorld;
        
        float       _DrawDistance;
        float       _Accuracy;
        int         _MaxIterations;
        
        float4      _Sphere1;
        int         _TargetCount;
        float4      _Targets[100];

        struct appdata
        {
            float4 vertex: POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
                float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 ray : TEXCOORD1;
        };

        v2f Vert(appdata v)
        {
            v2f o;
            v.vertex.z = 0.1;
        
            o.pos = float4(v.vertex.xy, 0.0, 1.0);
            o.uv = (v.vertex.xy + 1.0) * 0.5;

            int index = (o.uv.x / 2) + o.uv.y;
            o.ray = _Frustum[(int)index];
            o.ray.w = index;


            return o;
        }

     float phere(float3 p, float s)
        {
            return length(p) - s;
        }



        float distanceField(float3 p)
        {
            float value;
            if (_TargetCount > 0)
            {
                value = phere(p - _Targets[0].xyz, 0.5);
                // for (int i = 0; i < _TargetCount; i++)
                // {
                //     float sphere = sdSphere(p - _Targets[i].xyz, 0.1);
                //     if (i == 0)
                //     {
                //         value = sphere;
                //         continue;
                //     }
                //     value = opUS(sphere, value, 1);
                // }
            }
            return value;
            //return sdSphere(p - _Sphere1.xyz, _Sphere1.w);
        }

        fixed4 raymarch(float3 rayOrigin, float3 rayDirection)
        {
            fixed4 ret = fixed4(0,0,0,0);

            float t = 0;
            const int max_iteration = _MaxIterations || 64;
            for(int i = 0; i < max_iteration; i++)
            {
                if (t > _DrawDistance)
                {
                    ret = fixed4(0, 0, 0, 0);
                    break;
                }

                float3 p = rayOrigin + rayDirection * t;
                float d = distanceField(p);

                if (d < _Accuracy)
                {
                    ret = fixed4(1.0, 1.0, 1.0, 1.0);
                    break;
                }
                t += d;
            }
            return ret;
        }


        float4 Frag(v2f i) : SV_Target
        {
            fixed3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
            float3 rayDirection = normalize(i.ray.xyz);
            float3 rayOrigin = _WorldSpaceCameraPos;
            fixed4 result = raymarch(rayOrigin, rayDirection);
            return fixed4(col * (1.0 - result.w) + result.xyz * result.w, 1.0);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}