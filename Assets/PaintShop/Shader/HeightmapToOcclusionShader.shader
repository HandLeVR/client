Shader "Unlit/HeightmapToOcclusionShader"
{
    Properties
    {
         // Heightmap
        _MainTex ("Texture", 2D) = "white" {}
        _OcclusionTex ("OcclusionTex", 2D) = "white" {}
		_BaseColor("BaseColor", Color) = (1,1,1,1)
		_FullOcclusionHeightmapThreshold("FullOcclusionHeightmapThreshold", Float) = 0.1
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
            float4 _MainTex_ST;
            sampler2D _OcclusionTex;
			half4 _BaseColor;
			float _FullOcclusionHeightmapThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 occ = tex2D(_OcclusionTex, i.uv);
                col.rgb = lerp(_BaseColor.rgb, occ.rgb, saturate(col.r / _FullOcclusionHeightmapThreshold));
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}
