Shader "Unlit/HeightmapToColorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("BumpTex", 2D) = "white" {}
        _RoughnessTex ("RoughnessTex", 2D) = "white" {}
		_BaseColor("BaseColor", Color) = (0.5,0.5,1,1)
		_StartRoughnessHeightmapThreshold("StartRoughnessHeightmapThreshold", Float) = 2500
		_FullRoughnessHeightmapThreshold("FullRoughnessHeightmapThreshold", Float) = 5000
		_MinFlowHeightmapThreshold("MinFlowHeightmapThreshold", Float) = 5000
		_RoughnessStrength("RoughnessStrength", Float) = 0.1
		_DistanceRoughnessStrength("RoughnessStrength", Float) = 1
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _BumpMap;
            sampler2D _RoughnessTex;
            float4 _MainTex_ST;
			half4 _BaseColor;
			float _StartRoughnessHeightmapThreshold;
			float _FullRoughnessHeightmapThreshold;
			float _MinFlowHeightmapThreshold;
			float _RoughnessStrength;
			float _DistanceRoughnessStrength;
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 heightmap = tex2D(_MainTex, i.uv);
                half4 normalmap = tex2D(_BumpMap, i.uv);
                half4 roughness = tex2D(_RoughnessTex, i.uv);

                float diff = _FullRoughnessHeightmapThreshold - _StartRoughnessHeightmapThreshold;
                float naturalRoughness = (heightmap.a - _StartRoughnessHeightmapThreshold) / diff;
                float distanceRoughness = ((heightmap.r - _StartRoughnessHeightmapThreshold) / diff) * _DistanceRoughnessStrength;
                // is 0 if the thickness exceeds the flow threshold to disable roughness on running coat
                float switchDistanceRoughness = heightmap.a > _MinFlowHeightmapThreshold ? 0 : 1;
                return lerp(normalmap, roughness, (saturate(naturalRoughness) * 0.25 + saturate(distanceRoughness) * 0.75) * _RoughnessStrength * switchDistanceRoughness);
            }
            ENDCG
        }
    }
}
