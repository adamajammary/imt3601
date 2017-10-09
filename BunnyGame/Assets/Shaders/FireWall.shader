Shader "Unlit/FireWall" {
	Properties {
		_NoiseSeed ("Noise seed", float) = 1.0
	}
	SubShader {
		AlphaToMask On
		Cull Off
		Tags {"Queue" = "Transparent-5" "RenderType"= "Transparent"}
		LOD 100

		Pass {
			CGPROGRAM
			#pragma exclude_renderers gles flash
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"
			#include "noise.hlsl"

			uniform float _NoiseSeed;

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float3 worldPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//Want the worldpos of vertex for fragment shader
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				float3 samplePos = i.worldPos;
				samplePos.y = samplePos.y + _NoiseSeed * 10; //Only move noise plane in y dir, so that fire rises
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
				float normH = ((i.worldPos.y + 40) / 500); //Normalized height (not perfect)
				// The thing with (ceil(x - a) - ceil(x - b)) is that it returns 1 when a < x < b for 0 < x < 1.
				// This makes it so that i wont need if statements, which are a bad idea on the GPU.
				fixed4 col =
					black * (ceil(normH + n1 - 0.0f) - ceil(normH + n1 - 0.1))
					+ red * (ceil(normH + n1 - 0.1) - ceil(normH + n1 - 0.4))
					+ orange * (ceil(normH + n1 - 0.4) - ceil(normH + n1 - 0.7))
					+ gray * (ceil(normH + n1 - 0.7) - ceil(normH + n1 - 0.9))
					+ white * (ceil(normH + n1 - 0.9) - ceil(normH + n1 - 1.0));
				return col;
			}
			ENDCG
		}
	}
}
