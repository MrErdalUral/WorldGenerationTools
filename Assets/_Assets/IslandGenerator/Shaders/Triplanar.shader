Shader "Custom/Triplanar"
{
	Properties
	{
		_MainTexTop("Texture Top", 2D) = "white" {}
		_MainTexSide("Texture Side", 2D) = "white" {}
		_Tiling("Tiling", Float) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}
		_SlopeTreshold("Slope",Float) = 0.0
	}
		SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				half3 objNormal : TEXCOORD0;
				float3 coords : TEXCOORD1;
				float2 uv : TEXCOORD2;
				float4 pos : SV_POSITION;
			};

			float _Tiling;
			float4 _OcclusionMap_ST;

			v2f vert(float4 pos : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(pos);
				o.coords = pos.xyz * _Tiling;
				o.objNormal = normal;
				o.uv = TRANSFORM_TEX(uv, _OcclusionMap);
				return o;
			}

			sampler2D _MainTexTop;
			sampler2D _MainTexSide;
			sampler2D _OcclusionMap;
			half _SlopeTreshold;

			fixed4 frag(v2f i) : SV_Target
			{
				// use absolute value of normal as texture weights
				half3 blend = abs(i.objNormal);
				// make sure the weights sum up to 1 (divide by sum of x+y+z)

				if(blend.y > _SlopeTreshold)
					blend = half3(0,1,1);
				else
					blend.y = 0;

				blend /= dot(blend,1.0);

				// read the three texture projections, for x,y,z axes
				fixed4 cx = tex2D(_MainTexSide, i.coords.yz);
				fixed4 cy = tex2D(_MainTexTop, i.coords.xz);
				fixed4 cz = tex2D(_MainTexSide, i.coords.xy);
				// blend the textures based on weights
				fixed4 c = cx * blend.x + cy * blend.y + cz * blend.z;

				// modulate by regular occlusion map
				c *= tex2D(_OcclusionMap, i.uv);
				return c;
			}
			ENDCG
		}
	}
}