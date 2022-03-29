/// Adapted from the "Unlit/ShowUv2" shader from the "ProjectionSpray-v2" asset.
Shader "Custom/NormalsDrawerShader"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100 Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv2 : TEXCOORD1;
				float3 normal: NORMAL;
			};

			struct v2f
			{
				float2 uv2 : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			v2f vert(appdata v)
			{
			#if UNITY_UV_STARTS_AT_TOP
				v.uv2.y = 1.0 - v.uv2.y;
			#endif

				v2f o;
				o.vertex = float4(v.uv2*2.0 - 1.0, 0.0, 1.0);
				o.uv2 = v.uv2;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
			    // convert normal (value between -1 and 1) to color (value between 0 and 1)
			    return half4((i.normal.xyz + 1) / 2,1);
			}
			ENDCG
		}
	}
}
