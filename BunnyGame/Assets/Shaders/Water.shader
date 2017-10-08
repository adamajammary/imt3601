// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Water" {
	Properties {
		_Col ("Color", Color) = (0, 0, 0.2, 0.7)
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
				float3 posEye : TEXCOORD0;
				float3 lightPosEye : TEXCOORD1;
				float3 worldRefl : TEXCOORD2;
				float3 eyeNormal : TEXCOORD3;
				SHADOW_COORDS(4) // put shadows data into TEXCOORD1
				fixed4 diff : COLOR0;
				fixed3 ambient : COLOR1;
			};

			uniform float4 _Col;
			uniform float _NoiseSeed;

			v2f vert(appdata_base v) {
				v2f o;
				//Vertex manipulation for wavy effect
				float3 samplePos = v.vertex.xyz + _NoiseSeed;
				samplePos.xyz *= 1.2;
				float3 modifiedVertex = v.vertex + v.normal * noise(samplePos)*0.5;
				//Usefull data
				o.pos = UnityObjectToClipPos(modifiedVertex);
				o.eyeNormal = UnityObjectToViewPos(v.normal);
				o.posEye = UnityObjectToViewPos(modifiedVertex);
				o.lightPosEye = UnityObjectToViewPos(_WorldSpaceLightPos0);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				//Reflection
				o.worldRefl = reflect(-worldViewDir, worldNormal);
				//Lighting
				float nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.ambient = ShadeSH9(half4(worldNormal, 1));
				// Shadow
				TRANSFER_SHADOW(o)
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				//Reflection
				float4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
				float3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
				//Shadow
				fixed shadow = SHADOW_ATTENUATION(i);
				//----Light----
				//	Specular
				float3  eyeReflection = reflect(i.lightPosEye, i.eyeNormal);
				float3 posToViewer = normalize(-i.posEye);
				float dotSpecular = dot(eyeReflection, posToViewer);
				dotSpecular = max(dotSpecular, 0);
				float specular = pow(dotSpecular, 0.02);
				//specular = 0;
				fixed3 light = (i.diff + specular) * shadow + i.ambient;				
				//Combine into final color
				fixed4 c = _Col;
				c.rgb += skyColor;
				c.rbg *= light;
				return c;
			}
			ENDCG
		}		
	}
}
