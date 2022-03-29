// scales the output in dependence of the given target height
Shader "Unlit/WidestreamCookieShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_TargetHeight("TargetHeight", Float) = 512
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
            float4 _MainTex_TexelSize;
            float _TargetHeight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float scaleFactorHeight = _MainTex_TexelSize.w / _TargetHeight;
                float startY = 0.5 - 0.5 /scaleFactorHeight;
                float endY = 0.5 + 0.5 / scaleFactorHeight;
                if (i.uv.y < startY || i.uv.y > endY)
                    return float4(0,0,0,0);
                return tex2D(_MainTex, float2(i.uv.x, 0.5 + (0.5 - i.uv.y) * scaleFactorHeight));
            }
            ENDCG
        }
    }
}
