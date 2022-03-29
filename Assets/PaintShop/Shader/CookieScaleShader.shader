
/// Scales the paint cookie in dependence of the directional movements 
/// of the spray gun. This is needed to avoid gabs between cookies drawn 
/// in successive frames if the spray gun was moved with a higher velocity.
///
/// Explanations:
/// cutting vector: the vector cutting the cookie in half
/// scaling vector: the vector used to scale the cookie (perpendicular to this vector)
/// cutting position: the cutting vector goes through the cutting position and the center of the uv (0.5,0.5) 
/// scaling position: the scaling vector goes through the scaling position and the center of the uv (0.5,0.5) 
/// The cutting vector and the scaling vector are equal when the cookie is scaled in only 
/// one direction (width or height). Otherwise the cutting vector is adopted because of 
/// the elliptical form of the cookie.
Shader "Unlit/CookieScaleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_TargetWidth("TargetWidth", Float) = 512
		_TargetHeight("TargetHeight", Float) = 512
		_Direction("Direction", Float) = 1
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
            float4 _MainTex_TexelSize;
            float _TargetWidth, _TargetHeight, _Direction;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // Returns the position on the cutting vector for a given x.
            // Allows to check of a pixel needs to be scaled.
            float getYOnCuttingVector(float x, float2 cuttingPos){
                return ((cuttingPos.y-0.5)/(cuttingPos.x-0.5)) * (x - 0.5) + 0.5;
            }
            
            // Determines the target pixel on the scaling vector for a pixel which needs to be scaled.
            // Uses the perpendicular plane to the cutting vector which contains the pixel.
            // Help: https://www.mathematik-oberstufe.de/vektoren/a/abstand-punkt-gerade-lot.html
            float2 getTargetPos(float2 uv, float2 center, float2 scalingPos, float2 cuttingPos){
                // direction of the scaling vector
                float2 r1 = scalingPos - center;
                // the coordinate function of the plane perpendicular to the cutting vector needs to satisfy d
                float d = r1.x*uv.x + r1.y*uv.y;
                // direction of the cutting vector
                float2 r2 = cuttingPos - center;
                // get the intersection point pf the plane and the cutting vector
                float s = (r1.x * center.x + r1.y * center.y - d) / (r1.x*r2.x + r1.y*r2.y); 
                return center - s * r2;
            }

            half4 frag (v2f i) : SV_Target
            {
                if (_Direction <= 0)
                    i.uv.y = 1- i.uv.y;
                
                float2 cuttingPos;
                float2 scalingPos;
                float diffWidth = _TargetWidth - _MainTex_TexelSize.z; 
                float diffHeight = _TargetHeight - _MainTex_TexelSize.w; 
                
                // determine the cutting and scaling position for the cutting and scaling vector
                if (diffWidth == 0 && diffHeight == 0) 
                {
                    cuttingPos = float2(0.6, 0);
                    scalingPos = float2(1, 0);
                }
                else if (diffWidth < diffHeight) 
                {
                    cuttingPos = float2(1 - (diffWidth / diffHeight) * 0.4, 0.5 - (diffWidth / diffHeight) * 0.5);
                    scalingPos = float2(1, 0.5 - (diffWidth / diffHeight) * 0.5);
                }
                else 
                {
                    cuttingPos = float2(0.5 + (diffHeight / diffWidth) * 0.1, 0);
                    scalingPos = float2(0.5 + (diffHeight / diffWidth) * 0.5, 0);
                }
                
                float2 scaledUV;
                float scaleFactorWidth = _TargetWidth /  _MainTex_TexelSize.z;
                float scaleFactorHeight = _TargetHeight /  _MainTex_TexelSize.w;
                float scaledX = i.uv.x * scaleFactorWidth;
                float scaledY = i.uv.y * scaleFactorHeight;
                
                if (scaledY <= getYOnCuttingVector(scaledX, cuttingPos)) 
                {
                    // avoid double cookie
                    if (scaledY <= 1 && scaledX <= 1)
                        scaledUV = float2(scaledX, scaledY);
                }
                else if ((1 - i.uv.y) * scaleFactorHeight <= getYOnCuttingVector((1 - i.uv.x) * scaleFactorWidth, cuttingPos)) 
                {
                    // avoid double cookie
                    if ((1 - i.uv.y) * scaleFactorHeight <= 1 && (1 - i.uv.x) * scaleFactorWidth <= 1)
                        scaledUV = float2(scaledX - (scaleFactorWidth - 1), scaledY - (scaleFactorHeight -1));
                }
                else 
                {
                    float2 targetPos = getTargetPos(float2(scaledX, scaledY), float2(0.5,0.5), scalingPos ,cuttingPos);
                    if (targetPos.y < 1 && targetPos.x < 1 && targetPos.y > 0 && targetPos.x > 0)
                        scaledUV = targetPos;
                }
                
                return tex2D(_MainTex, scaledUV);
            }
            ENDCG
        }
    }
}
