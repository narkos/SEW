Shader "MINA/MetaballsRaymarching"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "DistanceFunctions.cginc"


            uniform sampler2D _MainTex;
            uniform float4 _MainTex_TexelSize;
            // Raymarching Vars
            uniform float4x4 _camToWorld;
            uniform float4x4 _frustum;
            uniform float _drawDistance;
            uniform int _maxIterations;
            uniform float _accuracy;

            uniform float3 _position;
            uniform float _radius;

            uniform int _transformCount;
            uniform float4 _transforms[10];

            struct appdata
			{
				// Remember, the z value here contains the index of _FrustumCornersES to use
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 ray : TEXCOORD1;
			};

            v2f vert (appdata v)
            {
                v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0.1;
				
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
				#endif

				o.ray = _frustum[(int)index].xyz;
				o.ray /= abs(o.ray.z);

				o.ray = mul(_camToWorld, o.ray);

				return o;
            }

            /// FRAG FUNCTIONS

            float distanceField(float3 p)
            {
                float value;
                if (_transformCount > 0)
                {
                    for (int i = 0; i < _transformCount; i++)
                    {
                        float currentRadius = 0.5 * (sin((_Time.y * 5) + i * 0.5)) + 0.5;
                        currentRadius = currentRadius * ( 1 - _radius) + _radius;
                        float sphere = sdSphere(p - _transforms[i], currentRadius);
                        if (i == 0) 
                        {
                            value = sphere;
                            continue;
                        }
                        value = opUS(sphere, value, 1);
                    }
                }
                //float sphere1 = sdSphere(p - _position.xyz, _radius);
                return value;
            }

            fixed4 raymarch(float3 ro, float3 rd, float depth) 
            {
                fixed result = fixed4(0, 0, 0, 0);
      
                const int max_iterations = _maxIterations;
                float t = 0;
                for (int i = 0; i < max_iterations; ++i)
                {
                    if (t > _drawDistance)
                    {
                        result = fixed4(0, 0, 0, 0);
                        break;
                    }

                    float3 p = ro + rd * t;
                    float d = distanceField(p);

                    if (d < _accuracy)
                    {
                        result = fixed4(1, 1, 1, 1);
                        break;
                    }
                    t += d;
                }

                return result;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
				// depth *= length(i.ray);

				fixed3 col = tex2D(_MainTex, i.uv);
				// ray direction
				float3 rd = normalize(i.ray.xyz);
				// ray origin (camera position)
				float3 ro = _WorldSpaceCameraPos;

				fixed4 result = raymarch(ro, rd, 1.0);
				// return fixed4(result.xyz, 1);
				//return fixed4(col ,1.0);
                //fixed4 result = fixed4(rd, 0.5);
				return fixed4(col * (1.0 - result.w) + result.xyz * result.w, 1.0);
            }
            ENDCG
        }
    }
}
