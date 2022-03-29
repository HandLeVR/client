// maps the heightmap to the metallic value
Shader "Unlit/HeightmapToMetallicShader"
{
    Properties
    {
        // Heightmap
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0,0,0,1)
        _BaseColor("BaseColor", Color) = (0,0,0,1)
        _MaxSmoothness("MaxSmoothness", Range(0.0, 1.0)) = 1.0
		_MaxSmoothnessHeightmapThreshold("MaxSmoothnessHeightmapThreshold", Float) = 10000
        _PowerValue("PowerValue", Float) = 2
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
            fixed _MaxSmoothness;
            fixed _MaxSmoothnessHeightmapThreshold;
            fixed _PowerValue;
			
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
                              
			    float t = saturate(pow(col.b / _MaxSmoothnessHeightmapThreshold, _PowerValue));
                col.rgb = lerp(_BaseColor.rgb, _Color.rgb, t);
                col.a = lerp(_BaseColor.a, _MaxSmoothness, t);
                
                return col;
            }
            ENDCG
        }
    }
}
