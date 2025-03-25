Shader "Custom/Reflection"
{
	Properties
	{
		_ReflectionTex("Texture", 2D) = "white" {}
		_ReflectionAmount("Reflection", Range(0,1)) = 0.25
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" }
			LOD 100
			Blend SrcAlpha OneMinusSrcAlpha

			Stencil
			{
				Ref 1
				Comp Equal
			}
			Pass
			{


				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _ReflectionTex;
				float _ReflectionAmount;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_ReflectionTex, i.uv);
					col.a = _ReflectionAmount;
					return col;
				}
				ENDCG
			}
		}
}
