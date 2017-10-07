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
				samplePos.xyz = samplePos.xyz + _NoiseSeed;
				samplePos /= 33.7;

				fixed4 red = { 1, 0, 0, 0.5f };
				fixed4 black = { 0, 0, 0, 0.5f };
				fixed4 white = { 1, 1, 1, 0.5f };
				float n = noise(samplePos);
				fixed4 col = lerp(lerp(black, red, n), lerp(red, white, n*n), n);
				return col;
			}
			ENDCG
		}
	}
}
