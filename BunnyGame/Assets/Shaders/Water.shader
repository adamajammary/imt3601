Shader "Custom/Water" {
	Properties {
		_Col ("Color", Color) = (0,0,1,0.7)
		_NoiseSeed("Noise seed", float) = 1.0
	}
	SubShader{
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask On

		Tags{
			"RenderType" = "Fade"
			"Queue" = "Transparent"
			"LightMode" = "ForwardBase"
		}
		Pass {


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "noise.hlsl"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			struct v2f {
				float4 pos : SV_POSITION;
				SHADOW_COORDS(1) // put shadows data into TEXCOORD1
				fixed4 diff : COLOR0;
				fixed3 ambient : COLOR1;
			};

			uniform float4 _Col;
			uniform float _NoiseSeed;

			v2f vert(appdata_base v) {
				v2f o;
				float3 samplePos = v.vertex.xyz + _NoiseSeed;
				samplePos.xyz *= 1.2;
				o.pos = UnityObjectToClipPos(v.vertex + v.normal * noise(samplePos)*0.05);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.ambient = ShadeSH9(half4(worldNormal, 1));
				// compute shadows data
				TRANSFER_SHADOW(o)
					return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				fixed4 c = _Col;
				// compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
				fixed shadow = SHADOW_ATTENUATION(i);
				fixed3 light = i.diff * shadow + i.ambient;
				c.rbg *= light;
				return c;
			}
			ENDCG
		}		
	}
}
