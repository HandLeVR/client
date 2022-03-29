// converts the heightmap to color
Shader "Unlit/HeightmapToColorShader"
{
    Properties
    {
        // Heightmap
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0,0,1,1)
		_BaseColor("BaseColor", Color) = (1,1,1,1)
		_FullColorHeightmapThreshold("FullColorHeightmapThreshold", Float) = 10000
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
			half4 _Color;
			half4 _BaseColor;
			float _FullColorHeightmapThreshold;
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                
                col.rgb = lerp(_BaseColor.rgb, _Color.rgb, saturate(max(col.b, col.a) / _FullColorHeightmapThreshold));
                col.a = 1;
                
                return col;
            }
            ENDCG
        }
    }
}
