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
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				float3 samplePos = i.worldPos;
				samplePos.y = samplePos.y + _NoiseSeed * 10;
				samplePos.xz /= 23.7;
				samplePos.y /= 102.3;


				fixed4 red = { 1, 0.2, 0, 0.6 };
				fixed4 orange = { 1, 0.55, 0, 0.6 };
				fixed4 black = { 0, 0, 0, 0.6 };
				fixed4 white = { 1, 1, 1, 0.6 };
				fixed4 gray = { 0.7, 0.7, 0.7, 0.6 };
				float n = abs(noise(samplePos));
				float n1 = n / 2;
				float n2 = n / 4;
				float normH = ((i.worldPos.y + 40) / 450);
				fixed4 col = 
					black * (ceil(normH + n1 - 0.0f) - ceil(normH + n1 - 0.2))
					+ red * (ceil(normH + n1 - 0.2) - ceil(normH + n1 - 0.5))
					+ orange * (ceil(normH + n1 - 0.5) - ceil(normH + n1 - 0.9))
					+ gray * (ceil(normH + n1 - 0.9) - ceil(normH + n1 - 0.95))
					+ white *(ceil(normH + n1 - 0.95) - ceil(normH + n1 - 1.0));
				return col;
			}
			ENDCG
		}
	}
}
