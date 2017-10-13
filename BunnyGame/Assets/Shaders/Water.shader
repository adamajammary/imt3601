// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

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

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "noise.hlsl"
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
				float3 eyeNormal : TEXCOORD3;
				SHADOW_COORDS(4) // put shadows data into TEXCOORD1
				fixed4 diff : COLOR0;
				fixed3 ambient : COLOR1;
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
				o.eyeNormal = normalize(UnityObjectToViewPos(v.normal));
				o.posEye = UnityObjectToViewPos(modifiedVertex);
				o.lightDirEye = normalize(_WorldSpaceLightPos0); //It's a directional light
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				float3 wNormal = UnityObjectToWorldNormal(v.normal);
				//Reflection
				o.worldRefl = reflect(-worldViewDir, wNormal);
				//Lighting
				float nl = max(0, dot(wNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.ambient = ShadeSH9(half4(wNormal, 1));
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
				float3 eyeReflection = reflect(i.lightDirEye, i.eyeNormal);
				float3 posToViewer = normalize(-i.posEye);
				float dotSpecular = saturate(dot(eyeReflection, posToViewer));
				float3 specular = pow((dotSpecular), 10) *_LightColor0;
				fixed3 light = (i.diff*2 + specular) * shadow + i.ambient*2;
				//Combine into final color
				fixed4 c = _Col;
				c.rgb = skyColor;
				c.rbg *= light;
				return c;
			}
			ENDCG
		}		
	}
}
