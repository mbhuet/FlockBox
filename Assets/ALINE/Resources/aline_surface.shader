Shader "Hidden/ALINE/Surface" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,0.5)
	_MainTex ("Texture", 2D) = "white" { }
	_Scale ("Scale", float) = 1
	_FadeColor ("Fade Color", Color) = (1,1,1,0.3)
}
SubShader {
	Tags {"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

	Offset -2, -20
	Cull Off

	Pass {
		// Z-write further back to avoid lines drawn at the same z-depth to partially clip the surface
		Offset 0, 0
		ZWrite On
		ColorMask 0
		
		HLSLPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma shader_feature UNITY_HDRP
		#include "aline_common.cginc"

		float4 _Color;
		
		struct v2f {
			float4  pos : SV_POSITION;
			float alpha : COLOR;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f vert (appdata_color v) {
			UNITY_SETUP_INSTANCE_ID(v);
			v2f o;
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = TransformObjectToHClip(v.vertex);
			o.alpha = (v.color * _Color).a;
			return o;
		}
		
		float4 frag (v2f i) : COLOR {
			if (i.alpha < 0.3) discard;
			return float4(1,1,1,1);
		}
		ENDHLSL
	}

	
	// Render behind
	Pass {
		ZWrite Off
		ZTest Greater
		Blend SrcAlpha OneMinusSrcAlpha

		HLSLPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma shader_feature UNITY_HDRP
		#include "aline_common.cginc"

		sampler2D _MainTex;
		float _Scale;
		float4 _Color;
		float4 _FadeColor;

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
			float4 col : COLOR;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		float4 _MainTex_ST;

		v2f vert (appdata_color v) {
			UNITY_SETUP_INSTANCE_ID(v);
			v2f o;
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = TransformObjectToHClip(v.vertex);

			float4 worldSpace = mul(UNITY_MATRIX_M, v.vertex);
			o.uv = float2 (worldSpace.x*_Scale,worldSpace.z*_Scale);
			o.col = v.color * _Color * _FadeColor;
			o.col.rgb = ConvertSRGBToDestinationColorSpace(o.col.rgb);
			return o;
		}

		float4 frag (v2f i) : COLOR {
			return tex2D (_MainTex, i.uv) * i.col;
		}
		ENDHLSL

	}
	 
	// Render in front
	Pass {
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha

		HLSLPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma shader_feature UNITY_HDRP
		#include "aline_common.cginc"

		float4 _Color;
		sampler2D _MainTex;
		float _Scale;

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
			float4 col : COLOR;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f vert (appdata_color v)
		{
			UNITY_SETUP_INSTANCE_ID(v);
			v2f o;
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = TransformObjectToHClip(v.vertex);

			float4 worldSpace = mul(UNITY_MATRIX_M, v.vertex);
			o.uv = float2 (worldSpace.x*_Scale,worldSpace.z*_Scale);
			o.col = v.color * _Color;
			o.col.rgb = ConvertSRGBToDestinationColorSpace(o.col.rgb);
			return o;
		}

		float4 frag (v2f i) : COLOR
		{
			return tex2D (_MainTex, i.uv) * i.col;
		}
		ENDHLSL
	}
}
Fallback Off
}
