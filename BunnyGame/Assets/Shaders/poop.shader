Shader "Unlit/poop" {
	Properties {
		_Col ("Color", Color) = (0.54, 0.27, 0.07, 1.0)
		_NoiseSeed("Noise seed", float) = 1.0
	}
	SubShader {
		/*
		this stuff is based on:
		https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
		except the noise
		*/
		Pass {
			Tags{ "LightMode" = "ForwardBase" }

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
			
			v2f vert (appdata_base v) {
				v2f o;
				//Vertex manipulation for the wobbly effect
				float3 samplePos = v.vertex.xyz + _NoiseSeed;
				samplePos.xyz *= 2.2;
				o.pos = UnityObjectToClipPos(v.vertex + v.normal * noise(samplePos)*0.6);
				//Usuefull data
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				//Light
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.ambient = ShadeSH9(half4(worldNormal, 1));
				//Shadow
				TRANSFER_SHADOW(o)
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				//shadow
				fixed shadow = SHADOW_ATTENUATION(i);
				//light
				fixed3 light = i.diff * shadow + i.ambient;
				//Final fragment color
				fixed4 c = _Col;
				c.rbg *= light;
				return c;
			}
			ENDCG
		}
		// shadow caster rendering pass
		// using macros from UnityCG.cginc
		Pass {
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v) {
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag(v2f i) : SV_Target {
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
		// shadow casting support
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
