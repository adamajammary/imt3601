Shader "Custom/BlindEffect" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader{
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
				float seed = _Time * 45.3f;
				float3 samplePos = { i.uv.x + 17.3, i.uv.y + 189.2, 0.3 };
				samplePos.z += seed;
				samplePos *= 3.3;
				float n = noise(samplePos);
				n = noise(samplePos*n*0.01);

				fixed3 black = { 0, 0, 0 };
				fixed3 beige = { 0.92, 0.75, 0.49 };

				fixed4 col = { 1, 1, 1, 0.9 };
				col.rgb = lerp(beige, black, n);
				return col;
			}
			ENDCG
		}
	}
}
