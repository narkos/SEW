// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MINA/RaymarchShader"
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
			uniform sampler2D _CameraDepthTexture;

			uniform float4x4 _CamToWorld;
			uniform float4x4 _Frustum;

			uniform float _drawDistance;
			uniform int _maxIterations;
			uniform float _accuracy;

			// Light
			uniform float3 _lightDir;
			uniform fixed4 _lightColor;
			uniform float _lightIntensity;
			
			// Shadows
			uniform float _shadowIntensity;
			uniform float2 _shadowDistance;
			uniform float _shadowPenumbra;

			// AO
			uniform float _aoStepSize;
			uniform float _aoIntensity;
			uniform int _aoIterations;

			// Distance field
			uniform fixed4 _mainColor;
			uniform float4 _sphere1; 
			uniform float4 _sphere2;
			uniform float4 _box1;

			uniform float _box1Round;
			uniform float _boxSphereSmooth;
			uniform float  _sphereIntersectSmooth;



			uniform float3 _modInterval;
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

				o.ray = _Frustum[(int)index].xyz;
				o.ray /= abs(o.ray.z);

				o.ray = mul(_CamToWorld, o.ray);

				return o;
			}

			float BoxSphere(float3 p)
			{
				float2 Sphere1 = sdSphere(p - _sphere1.xyz, _sphere1.w);
				float Box1 = sdRoundBox(p - _box1.xyz, _box1.www, _box1Round);
				float combine1 = opSS(Sphere1, Box1, _boxSphereSmooth);
				float2 Sphere2 = sdSphere(p - _sphere2.xyz, _sphere2.w);
				float combine2 = opIS(Sphere2, combine1, _sphereIntersectSmooth);

				return combine2;
			}

			float distanceField(float3 p) {
				// float ground = sdPlane(p, float4(0, 1, 0, 0));
				// // float modX = pMod1(p.x, _modInterval.x);
				// // float modY = pMod1(p.y, _modInterval.y);
				// // float modZ = pMod1(p.z, _modInterval.z);
				// float boxSphere1 = BoxSphere(p);

				//return opUS(ground, boxSphere1, 0.2);

				return sdSphere(p - _sphere1.xyz, _sphere1);
			}

			float3 getNormal(float3 p)
			{
				const float2 offset = float2(0.001, 0.0);
				float3 n = float3(
					distanceField(p + offset.xyy) - distanceField(p - offset.xyy),
					distanceField(p + offset.yxy) - distanceField(p - offset.yxy),
					distanceField(p + offset.yyx) - distanceField(p - offset.yyx)
				);
				return normalize(n);
			}

			float hardShadow(float3 ro, float3 rd, float mint, float maxt)
			{
				for (float t = mint; t < maxt;)
				{
					float h = distanceField(ro+rd*t);
					if (h < 0.001)
					{
						return 0.0; 
					}
					t += h;
				}
				return 1.0;
			}
			float softShadow(float3 ro, float3 rd, float mint, float maxt, float k)
			{
				float result = 1.0;
				for (float t = mint; t < maxt;)
				{
					float h = distanceField(ro+rd*t);
					if (h < 0.001)
					{
						return 0.0; 
					}
					result = min(result, k * h / t);
					t += h;
				}
				return result;
			}

			float ambientOcclusion(float3 p, float3 n)
			{
				float step = _aoStepSize;
				float ao = 0.0;
				float dist;
				for (int i = 1; i <= _aoIterations; i++)
				{
					dist = step * i;
					ao += max(0.0, (dist - distanceField(p + n * dist)) / dist);
				}
				return (1.0 - ao * _aoIntensity);
			}

			float3 shading(float3 p, float3 n)
			{
				float3 result;
				// Diffuse Color
				float3 color = _mainColor.rgb;
				// Directional Light
				float3 light = (_lightColor.rgb * dot(-_lightDir, n) * 0.5 + 0.5) * _lightIntensity;

				// Shadows
				float shadow = softShadow(p, -_lightDir, _shadowDistance.x, _shadowDistance.y, _shadowPenumbra) * 0.5 + 0.5;
				shadow = max(0.0, pow(shadow, _shadowIntensity));
				
				// AO
				float ao = ambientOcclusion(p, n);

				result = color * light * shadow * ao;

				return result;
			}

			fixed4 raymarch(float3 ro, float3 rd, float depth) {
				fixed4 ret = fixed4(0,0,0,0);

				const int max_iteration = _maxIterations;
				float t = 0; // current distance traveled along ray
				for (int i = 0; i < max_iteration; ++i) {
					if (t > _drawDistance || t >= depth) 
					{
						ret = fixed4(rd, 0);
						break;
					}

					float3 p = ro + rd * t; // World space position of sample
					float d = distanceField(p);		// Sample of distance field (see map())

					// If the sample <= 0, we have hit something (see map()).
					if (d < _accuracy) 
					{
						//shading
						float3 n = getNormal(p);

						float3 s = shading(p, n);

						ret = fixed4( s,1);
						break;
					}
					t += d;
				}

				return ret;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
				depth *= length(i.ray);

				fixed3 col = tex2D(_MainTex, i.uv);
				// ray direction
				float3 rd = normalize(i.ray.xyz);
				// ray origin (camera position)
				float3 ro = _WorldSpaceCameraPos;

				fixed4 result = raymarch(ro, rd, depth);
				
				return fixed4(col * (1.0 - result.w) + result.xyz * result.w ,1.0);
			}
			ENDCG
		}
    }
}
