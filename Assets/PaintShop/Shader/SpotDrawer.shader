/// Adopted version of the SpotDrawer from the ProjectionSpray-v2 asset.
/// Allows drawing a metallic gloss map.
Shader "Custom/SpotDrawer"
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
			/// x is the thickness of the paint applied from a range further away than _MaxDistance, and
			/// y is the thickness of the paint applied from a range closer than _MinDistance.
			float2 distanceMap (v2f i)
			{
				///diffuse
				float3 to = i.worldPos - _DrawerPos.xyz;
				float3 dir = normalize(to);
				
				// calculate distance from vertex to plane constructed by the drawer position and pointing direction
				float a = _DrawerDir.x * _DrawerPos.x + _DrawerDir.y * _DrawerPos.y + _DrawerDir.z * _DrawerPos.z;
				float dist = _DrawerDir.x * i.worldPos.x + _DrawerDir.y * i.worldPos.y + _DrawerDir.z * i.worldPos.z - a;
				dist = dist / length(_DrawerDir);
				// we need to adapt the resulting distance because for some reason the distance is not
				// calculated correctly as it is increasing to much on higher distances
				dist = dist/1.1;
				// calculate how much the the distance differs from the optimal range
				float transDist = min(dist - _MinDistance, 0) + max(dist - _MaxDistance, 0);
				// map the "wrong" distance to a value between 0 and 1
				float relDist = clamp(abs(transDist) / _MinDistance, 0, 1);
				// calculates a factor depending on the angle between the spray direction and the the normal of the vertex
				// d=0 if the angle is greater than 90° and d=1 if the angle is 0°
				// this ensures that parts of the workpiece false declared as painted from to far away (e.g. if an edge of a workpiece is painted)
				float d = saturate(dot(normalize(-_DrawerDir),normalize(i.normal)));
				// decrease influence of the angle on greater distances
				d = d * pow(lerp(d,1,1-saturate(dist/_MaxDistance)),2);
				float atten = _Emission * dot(-dir, i.normal);
				atten = atten * relDist * relDist * d;

				///spot cookie
				float4 drawerSpacePos = mul(_WorldToDrawerMatrix, float4(i.worldPos, 1.0));
				float4 projPos = mul(_ProjMatrix, drawerSpacePos);
				projPos.z *= -1;
				float2 drawerUv = projPos.xy / projPos.z;
				drawerUv = drawerUv * 0.5 + 0.5;
				float cookie = tex2D(_Cookie, drawerUv);
				cookie *= 0<drawerUv.x && drawerUv.x<1 && 0<drawerUv.y && drawerUv.y<1 && 0<projPos.z;

				///shadow
				float drawerDepth = tex2D(_DrawerDepth, drawerUv).r;
				atten *= 1.0 - saturate(100 * abs(dist) - 100 * drawerDepth);

				// apply the thickness modifier in dependence of the distance
				float thicknessFactor = abs(min(sign(transDist),0)) * _CloseDistanceThicknessModifier +
					max(sign(transDist),0) * _FarDistanceThicknessModifier;
				float result = sign(transDist) * max(0,_Emission * atten * cookie * thicknessFactor);

				return float2(max(0,result), abs(min(0,result)));
			}

			
			/// Returns a float4 where
			/// x is the thickness of the paint applied from a range further away than _MaxDistance,
			/// y is the thickness of the paint applied from a range closer than _MinDistance,
			/// z is the general thickness of the paint but reduced on greater distances, and
			/// w is the general thickness of the paint independent of the distance.
			float4 frag (v2f i) : SV_Target
			{
				///diffuse
				float3 to = i.worldPos - _DrawerPos.xyz;
				float3 dir = normalize(to);
				float dist = length(to);
				
				float a = _DrawerDir.x * _DrawerPos.x + _DrawerDir.y * _DrawerPos.y + _DrawerDir.z * _DrawerPos.z;
				float distPlain = _DrawerDir.x * i.worldPos.x + _DrawerDir.y * i.worldPos.y + _DrawerDir.z * i.worldPos.z - a;
				distPlain = distPlain / length(_DrawerDir);
				// we need to adapt the resulting distance because for some reason the distance is not
				// calculated correctly as it is increasing to much on higher distances
				distPlain = distPlain/1.1;
				// increased influence of distance (less emission on greater distance)
				// dot prevents painting the backside of the object
				// max is needed to avoid negative values (e.g. when painting from backside)
				float atten = _Emission * max(0,dot(-dir, i.normal)) / (dist * dist);

				///spot cookie
				float4 drawerSpacePos = mul(_WorldToDrawerMatrix, float4(i.worldPos, 1.0));
				float4 projPos = mul(_ProjMatrix, drawerSpacePos);
				projPos.z *= -1;
				float2 drawerUv = projPos.xy / projPos.z;
				drawerUv = drawerUv * 0.5 + 0.5;
				float cookie = tex2D(_Cookie, drawerUv);
				cookie *= 0<drawerUv.x && drawerUv.x<1 && 0<drawerUv.y && drawerUv.y<1 && 0<projPos.z;
				
				///shadow
				float drawerDepth = tex2D(_DrawerDepth, drawerUv).r;
				atten *= 1.0 - saturate(100 * abs(distPlain) - 100 * drawerDepth);
				
				float height = _Emission * atten * cookie * _AlphaToHeightmapMultiplier;
				return float4(distanceMap(i), height - saturate((distPlain - _MaxDistance) / 0.1) * height, height);
			}
			
			ENDCG
		}
	}
}
