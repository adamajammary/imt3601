Shader "Custom/Island" {
	Properties{
		_Color("Color", Color) = (1, 1, 1, 1)
		_FireWallPos("FWP", Vector) = (0, 0, 0, 0)
		_FireWallRadius("FWR", Float) = 100000
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
			#include "inRange.hlsl"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			struct v2f {
				float4 pos : SV_POSITION;
				float3 posEye : TEXCOORD0;
				float3 lightDirEye : TEXCOORD1;
				float3 eyeNormal : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				SHADOW_COORDS(4) // put shadows data into TEXCOORD1
				fixed4 diff : COLOR0;
				fixed3 ambient : COLOR1;
			};

			uniform float4 _Color;
			uniform float4 _FireWallPos;
			uniform float _FireWallRadius;
			
			v2f vert (appdata_base v) {
				v2f o;
				//Usuefull data
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.eyeNormal = normalize(UnityObjectToViewPos(v.normal));
				o.posEye = UnityObjectToViewPos(v.vertex);
				o.lightDirEye = normalize(_WorldSpaceLightPos0); //It's a directional light
				//Light
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * float4( 1, 1, 1, 1 );
				o.ambient = ShadeSH9(half4(worldNormal, 1));
				//Shadow
				TRANSFER_SHADOW(o)
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				//shadow
				fixed shadow = SHADOW_ATTENUATION(i);
				//light
				//	Specular
				float3 eyeReflection = reflect(i.lightDirEye, i.eyeNormal);
				float3 posToViewer = normalize(-i.posEye);
				float dotSpecular = saturate(dot(eyeReflection, posToViewer));
				float3 specular = pow((dotSpecular), 5) * float4(1, 1, 1, 1);
				fixed3 light = (i.diff + specular) * shadow + i.ambient;
				//Burn
				float3 pos = i.worldPos;
				pos.y = _FireWallPos.y;
				float dist = distance(pos, _FireWallPos.xyz);
				float outsideFireWall = saturate(floor(_FireWallRadius / dist));
				//Final fragment color
				float seed = _Time * 25.3f;
				float3 samplePos = i.worldPos;
				samplePos.y += seed;
				samplePos *= 0.3;
				float n = noise(samplePos);

				fixed3 black = { 0.1, 0.1, 0.1 };
				fixed3 red = { 1, 0, 0 };
				fixed3 burnColor = black + red * inRange(n, 0.1, 0.15);

				fixed4 c = _Color;
				c.rbg = _Color.rbg * outsideFireWall + burnColor * ceil(1 - outsideFireWall);
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
