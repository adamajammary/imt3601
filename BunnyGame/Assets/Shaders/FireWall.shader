Shader "Unlit/FireWall" {
	Properties {
	}
	SubShader {
		AlphaToMask On
		Cull Off
		Tags {
		"Queue" = "Transparent-5" 
		"RenderType"= "Transparent"
		}
		Blend One One

		Pass {
			CGPROGRAM
			#pragma exclude_renderers gles flash
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "utils.hlsl"

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float3 worldPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
				//UNITY_FOG_COORDS(5)
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//Want the worldpos of vertex for fragment shader
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				// Fog
				//UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				float3 samplePos = i.worldPos;
				samplePos.y = samplePos.y - _Time.y * 400; //Only move noise plane in y dir, so that fire rises
				samplePos.xz /= 23.7;
				samplePos.y /= 102.3;

				//Kinda making the fire texture as a layerd cake, 
				// with black as bottom layer, progressing in same order as declared
				fixed4 black = { 0, 0, 0, 0.6 };
				fixed4 red = { 1, 0.2, 0, 0.6 };
				fixed4 orange = { 1, 0.55, 0, 0.6 };
				fixed4 gray = { 0.7, 0.7, 0.7, 0.6 };
				fixed4 white = { 1, 1, 1, 0.6 };
				fixed4 none = { 0, 0, 0, 0 };
		
				float n = abs(noise(samplePos));
				float n1 = n / 2;
				float normH = ((i.worldPos.y + 20) / 440); //Normalized height (not perfect)

				fixed4 col =
					black * inRange(normH + n1, 0.0, 0.1)
					+ red * inRange(normH + n1, 0.1, 0.4)
					+ orange * inRange(normH + n1, 0.4, 0.7)
					+ gray * inRange(normH + n1, 0.7, 0.75)
					+ white * inRange(normH + n1, 0.75, 0.8)
					+ none * inRange(normH + n1, 0.8, 1.0);


				// Fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
			ENDCG
		}
	}
}
