// displays different colors in dependence of the coat thickness
Shader "Unlit/CookieToHeatmapShader"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _HeightTex ("Height Map", 2D) = "white" {}
        _ColorTex ("Colored Tex", 2D) = "white" {}
		_NoColor("no color", Color) = (0,0.5,1,1)
		_InsufficientColor("insufficient color", Color) = (0,0,1,1)
		_GoodColor("good color", Color) = (0,1,0,1)
		_ExcessiveColor("excessive color", Color) = (1,0,0,1)
		_RunningColor("running color", Color) = (0.5,0,0,1)
        _NoHeight("no height", Float) = 0.01
        _GoodHeight("good height", Float) = 800
        _ExcessiveHeight("excessive height", Float) = 3000
        _RunningHeight("running height", Float) = 3500
        _GradientSmoothness("gradient smoothness", Range(0, 1)) = 0.1
        _InvertColors("invert colors", Range(0,1)) = 0
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

            struct VertexOutput{
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _HeightTex;
            sampler2D _ColorTex;
            float4 _MainTex_ST;
			half4 _NoColor;
			half4 _InsufficientColor;
			half4 _GoodColor;
			half4 _ExcessiveColor;
			half4 _RunningColor;
            fixed _NoHeight;
            fixed _GoodHeight;
            fixed _ExcessiveHeight;
            fixed _RunningHeight;
            fixed _GradientSmoothness;
            uniform float4 _Coordinate;
            float _InvertColors;

            VertexOutput vert (appdata v)
            {
                VertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (VertexOutput o) : SV_Target
            {
                float4 main_tex = tex2D(_MainTex,o.uv);           
                float4 height_map = tex2D(_HeightTex, o.uv);  
                float4 color_tex = tex2D(_ColorTex, o.uv);
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
                
                float height = max(height_map.b, height_map.a);
                float gradient = (_ExcessiveHeight - _GoodHeight) * _GradientSmoothness;
                if (height < _NoHeight){
                    float diff = _NoHeight - gradient;
                    height_map = lerp(_NoColor, _InsufficientColor, saturate((height - diff) / gradient));
                }
                else if (height < _GoodHeight){
                    float diff = _GoodHeight - gradient;
                    height_map = lerp(_InsufficientColor, _GoodColor, saturate((height - diff) / gradient));
                }
                else if (height < _ExcessiveHeight){
                    float diff = _ExcessiveHeight - gradient;
                    height_map = lerp(_GoodColor,_ExcessiveColor, saturate((height - diff) / gradient));
                }
                else if (height < _RunningHeight){
                    float diff = _RunningHeight - gradient;
                    height_map = lerp(_ExcessiveColor,_RunningColor, saturate((height - diff) / gradient));
                }
                else {
                    height_map = _RunningColor;
                }
                
                // Decides output color value based on mask
                float4 output = lerp(height_map, color_tex, mask.xxxx);
                
                return output;
            }
            ENDCG
        }
    }
}
