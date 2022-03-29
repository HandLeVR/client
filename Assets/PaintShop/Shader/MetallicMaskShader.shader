Shader "Unlit/MetallicMaskShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MetalTex ("Metallic Texture", 2D) = "white" {}
        _InvertColors("invert colors", Range(0,1)) = 0
    }
    SubShader
    {

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
            sampler2D _MetalTex;
            float4 _MainTex_ST;
            float _InvertColors;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Shader that applies a mask or cookie to the metallic map so it only appears locally
            float4 frag (v2f i) : SV_Target
            {
                float4 main_tex = tex2D(_MainTex, i.uv);
                float4 metal_tex = tex2D(_MetalTex, i.uv);

                float mask = 0;
                
                if (_InvertColors == 1)
                {
                    // prepare cookie color values
                    mask = 1-(main_tex.a >= 0.0000000000000000000000001);
                }
                else
                {
                    // prepare cookie color values
                    mask = (main_tex.a >= 0.0000000000000000000000001);
                }
                
                return lerp(float4(0,0,0,0), metal_tex, mask.xxxx);
            }
            ENDCG
        }
    }
}
