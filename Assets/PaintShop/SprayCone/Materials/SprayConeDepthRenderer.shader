// Sources:
// https://halisavakis.com/shader-bits-camera-depth-texture
// https://www.ronja-tutorials.com/post/017-postprocessing-depth
Shader "Unlit/DepthShader"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			
			#include "UnityCG.cginc"

			
            //the object data that's put into the vertex shader
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f{
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            	float4 scrPos : TEXCOORD1;
            };

            //the vertex shader
            v2f vert(appdata v){
                v2f o;
                //convert the vertex positions from object space to clip space so they can be rendered
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
            	o.scrPos = ComputeScreenPos(o.position);
                return o;
            }
			
			fixed4 frag(v2f i) : SV_TARGET{
			    //get depth from depth texture
			    float depth = tex2Dproj(_CameraDepthTexture, i.scrPos);
			    //linear depth between camera and far clipping plane
			    depth = Linear01Depth(depth);

			    return fixed4(depth,0,0,1);
			}
			ENDCG
		}
	}
}
