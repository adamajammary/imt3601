Shader "Custom/fireParticle"
{
	Properties {
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="TransparentCutOut" "Queue" = "Transparent-5" }
		Blend One OneMinusSrcAlpha // Premultiplied transparency
		AlphaToMask On

		Pass {
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

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				float smokeHeight = 10;
				float seed = _Time * 75.3f;
				float3 samplePos = i.worldPos + 20;
				float n = noise(samplePos / 0.4f + seed);
				n = noise(samplePos*n*0.5);
				n = noise(samplePos*n*0.5);
				
				fixed3 gray = { 0.7, 0.7, 0.7 };
				fixed3 black = { 0, 0, 0 };		
				fixed4 red = { 1, 0.2, 0, 0.6 };
				fixed4 orange = { 1, 0.55, 0, 0.6 };

				fixed4 col = { 1, 1, 1, 1 };
				float cutoff = ceil((0.5 - (length(i.uv - 0.5) + n * 0.2)) * 2 - 0.1);
				col.rgb = lerp(lerp(orange, red, n), lerp(gray, black, n), saturate((i.worldPos.y + 16) / smokeHeight));
				col *= cutoff;
				return col;
			}
			ENDCG
		}
	}
}
