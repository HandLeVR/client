Shader "Unlit/CustomShowUv2"
{
	Properties{
		_T("slider", Range(0,1)) = 0
	}
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
				float3 normal: NORMAL;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv2 : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 normal : TEXCOORD2;
				float4 vertex : SV_POSITION;
				float3 tangent : TANGENT;
			};

			float _T;

			v2f vert(appdata v)
			{
                #if UNITY_UV_STARTS_AT_TOP
                    v.uv2.y = 1.0 - v.uv2.y;
                    v.uv.y = 1.0 - v.uv.y;
                #endif
			
				float4 pos0 = UnityObjectToClipPos(v.vertex);
				float4 pos1 = float4(v.uv2*2.0 - 1, 0.0, 1.0);
				float4 tangent = v.tangent;

				v2f o;
				o.vertex = lerp(pos0, pos1, _T);
				o.uv2 = v.uv2;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
				float3 bitangent = cross(o.normal, v.tangent.xyz) * v.tangent.w;
				o.tangent = UnityObjectToWorldDir(v.tangent);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
			    return fixed4(i.normal.xyz * 0.5 + 0.5,1);
			}
			ENDCG
		}
	}
}
