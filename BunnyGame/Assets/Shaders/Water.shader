// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// This shader is a bit odd because i messed up the water model in blender, so the normals in the model don't make much sense.
Shader "Custom/Water" {
	Properties {
		_Col ("Color", Color) = (1, 1, 1, 1)
		_Wavyness("Wavyness", float) = 0.5
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
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "utils.hlsl"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			// vertex input: position, tangent
			struct appdata {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 posEye : TEXCOORD0;
				float3 lightDirEye : TEXCOORD1;
				float3 worldRefl : TEXCOORD2;
				SHADOW_COORDS(4) // put shadows data into TEXCOORD1
				fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
				UNITY_FOG_COORDS(5)
			};

			uniform float4 _Col;
			uniform float _Wavyness;

			v2f vert(appdata v) {
				v2f o;
				//Vertex manipulation for wavy effect
				float3 samplePos = v.vertex.xyz - _Time.y * 0.25;
				samplePos.xyz *= 11.2;
				float3 modifiedVertex = v.vertex + float3(0, 0, 1) * noise(samplePos)*_Wavyness;
				//Usefull data
				o.pos = UnityObjectToClipPos(modifiedVertex);
				o.posEye = UnityObjectToViewPos(modifiedVertex);
				o.lightDirEye = normalize(_WorldSpaceLightPos0); //It's a directional light
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				//Reflection
				o.worldRefl = reflect(-worldViewDir, float3(0, 1, 0));
				//Lighting
				half nl = max(0, dot(float3(0, 1, 0), _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.ambient = ShadeSH9(half4(float3(0, 1, 0), 1));
				// Shadow
				TRANSFER_SHADOW(o);
				// Fog
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				//Reflection
				float4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
				float3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
				//Shadow
				fixed shadow = SHADOW_ATTENUATION(i);
				//----Light----
				float3 specular = calcSpecular(i.lightDirEye, float3(0, 1, 0), i.posEye, 1);
				fixed3 light = (i.diff + specular) * shadow + i.ambient;
				//fixed3 light = specular;
				//Combine into final color
				fixed4 c = _Col;
				c.rgb = skyColor;
				c.rbg *= light;

				// Fog
				UNITY_APPLY_FOG(i.fogCoord, c);
				UNITY_OPAQUE_ALPHA(c.a);
				return c;
			}
			ENDCG
		}		
	}
}
