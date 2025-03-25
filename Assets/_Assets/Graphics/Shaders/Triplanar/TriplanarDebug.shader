Shader "Custom/TriplanarDebug"
{
    Properties
    {
        _Tiling ("Tiling", Float) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
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

            v2f vert (float4 pos : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(pos);
                o.coords = pos.xyz * _Tiling;
                o.objNormal = normal;
				o.uv = TRANSFORM_TEX(uv, _OcclusionMap);
                return o;
            }

            sampler2D _MainTexUp;
            sampler2D _MainTexRight;
            sampler2D _MainTexForward;
            sampler2D _OcclusionMap;
            
            fixed4 frag (v2f i) : SV_Target
            {
                // use absolute value of normal as texture weights
                half3 blend = abs(i.objNormal);
                // make sure the weights sum up to 1 (divide by sum of x+y+z)
                blend /= dot(blend,1.0);
				blend*=blend;
                // read the three texture projections, for x,y,z axes
                fixed4 cx = fixed4(1,0,0,1);
                fixed4 cy = fixed4(0,1,0,1);
                fixed4 cz = fixed4(0,0,1,1);
                // blend the textures based on weights
                fixed4 c = cx * blend.x + cy * blend.y + cz * blend.z;
                // modulate by regular occlusion map
                c *= tex2D(_OcclusionMap,i.uv);
                return c;
            }
            ENDCG
        }
    }
}