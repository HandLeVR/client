Shader "Unlit/CookieToAlphaShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CookieTex ("CookieTexture", 2D) = "white" {}
        // if this heightmap value is reached the color fully covers the underlying layer
		_FullColorHeightmapThreshold("FullColorHeightmapThreshold", Float) = 10000
		_DampFactorAfterMinThickness("DampFactorAfterMinThickness", Float) = 0.2
		_MaxAlpha("MaxAlpha", Float) = 1
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
            sampler2D _CookieTex;
            float4 _MainTex_ST;
            float _FullColorHeightmapThreshold;
            float _DampFactorAfterMinThickness;
            float _MaxAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 currentHeight = tex2D(_MainTex, i.uv);
                float4 cookieHeight = tex2D(_CookieTex, i.uv);
                
                float currentFloat = currentHeight.a;
                float cookieFloat = cookieHeight.a;
                
                float puffer = _FullColorHeightmapThreshold + (_FullColorHeightmapThreshold / 100) * 5;
                
                // needed to avoid hard edges
                // cookieFloat needs to be dumped to avoid too fast increase of the color
                float newGeneralHeight = currentFloat > _FullColorHeightmapThreshold ?
                    currentFloat + cookieFloat * _DampFactorAfterMinThickness :
                    lerp(currentFloat, puffer, saturate(cookieFloat / puffer));
                
                float newAdaptedHeight = currentHeight.b > _FullColorHeightmapThreshold ?
                    currentHeight.b + cookieHeight.b * _DampFactorAfterMinThickness :
                    lerp(currentHeight.b, puffer, saturate(cookieHeight.b / puffer));
                
                return float4(currentHeight.rg + cookieHeight.rg, min(newAdaptedHeight,_MaxAlpha), min(newGeneralHeight,_MaxAlpha));
            }
            ENDCG
        }
    }
}
