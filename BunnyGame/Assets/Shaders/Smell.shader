Shader "Custom/Smell" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent-5" }
		Blend One OneMinusSrcAlpha // Premultiplied transparency
		AlphaToMask On

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "noise.hlsl"

			struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				float seed = _Time * 75.3f;
				float3 samplePos = i.worldPos;
				float n = noise(samplePos * 13.1f + seed);
				n = noise(samplePos*n);

				fixed3 black = { 0, 0, 0 };
				fixed3 green = { 0, 1, 0 };

				fixed4 col = { 1, 1, 1, 1 };
				float cutoff = ceil((0.5 - (length(i.uv - 0.5) + n * 0.2)) * 2 - 0.1);
				col.rgb = lerp(green, black, n);
				col *= cutoff;
				return col;
			}
			ENDCG
		}
	}
}
