/// Shorter version of the SpotDrawer shader. 
// In flashlight mode we just need to put the cookie on the workpiece.
Shader "Custom/FlashlightSpotDrawer"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		_DrawerPos("drawer position", Vector) = (0,0,0,0)
		_Color("drawer color", Color) = (1,1,1,1)
		_Emission("emission", Float) = 1

		_Cookie("spot cokie", 2D) = "white"{}
		_DrawerDepth("drawer depth texture", 2D) = "white"{}
		
		_DrawerDir("drawer direction", Vector) = (0,0,0,0)
		_MinDistance("MinDistance", Float) = 0.15
		_MaxDistance("MaxDistance", Float) = 0.20
		
		_AlphaToHeightmapMultiplier("AlphaToHeightmapMultiplier", Float) = 0.02
		_CloseDistanceThicknessModifier("CloseDistanceThicknessModifier", Float) = 0.75
		_FarDistanceThicknessModifier("FarDistanceThicknessModifier", Float) = 0.25
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal: NORMAL;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 normal : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v.uv2.y = 1.0 - v.uv2.y;

				v2f o;
				o.vertex = float4(v.uv2*2.0 - 1.0, 0.0, 1.0);
				o.uv = v.uv2;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			sampler2D _MainTex;

			uniform float4x4 _ProjMatrix, _WorldToDrawerMatrix;

			sampler2D _Cookie, _DrawerDepth;
			float4 _DrawerPos, _DrawerDir, _Color;
			float _Emission;
			float _MinDistance;
			float _MaxDistance;
			float _AlphaToHeightmapMultiplier;
			float _CloseDistanceThicknessModifier;
			float _FarDistanceThicknessModifier;

			
			/// Returns a float4 where
			/// x is the thickness of the paint applied from a range further away than _MaxDistance,
			/// y is the thickness of the paint applied from a range closer than _MinDistance,
			/// z is the general thickness of the paint but reduced on greater distances, and
			/// w is the general thickness of the paint independent of the distance.
			float4 frag (v2f i) : SV_Target
			{
				///spot cookie
				float4 drawerSpacePos = mul(_WorldToDrawerMatrix, float4(i.worldPos, 1.0));
				float4 projPos = mul(_ProjMatrix, drawerSpacePos);
				projPos.z *= -1;
				float2 drawerUv = projPos.xy / projPos.z;
				drawerUv = drawerUv * 0.5 + 0.5;
				float cookie = tex2D(_Cookie, drawerUv);
				cookie *= 0<drawerUv.x && drawerUv.x<1 && 0<drawerUv.y && drawerUv.y<1 && 0<projPos.z;
				
				return cookie;
			}
			
			ENDCG
		}
	}
}
