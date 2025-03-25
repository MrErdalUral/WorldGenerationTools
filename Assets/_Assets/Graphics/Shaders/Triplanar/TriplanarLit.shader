Shader "Custom/TriplanarLit"
{
    Properties
    {
        _MainTexTop("Texture Top", 2D) = "white" {}
        _MainTexSide("Texture Side", 2D) = "white" {}
        _Tiling("Tiling", Float) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _SlopeTreshold("Slope", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Use a surface shader with Lambert lighting so that the shader is lit.
        #pragma surface surf Lambert
        #include "UnityCG.cginc"

        sampler2D _MainTexTop;
        sampler2D _MainTexSide;
        sampler2D _OcclusionMap;
        float _Tiling;
        float _SlopeTreshold;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_OcclusionMap;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Scale the world position for triplanar texture coordinates.
            float3 coords = IN.worldPos * _Tiling;
            // Get blending weights based on the absolute value of the world normal.
            float3 blend = abs(IN.worldNormal);

            // Optionally modify the blend weights based on slope
            if(blend.y > _SlopeTreshold)
                blend = float3(0, 1, 1);
            else
                blend.y = 0;

            // Normalize the blend weights so they sum to 1.
            blend /= (blend.x + blend.y + blend.z);

            // Sample the textures using different projections.
            fixed4 cx = tex2D(_MainTexSide, coords.yz);
            fixed4 cy = tex2D(_MainTexTop,  coords.xz);
            fixed4 cz = tex2D(_MainTexSide, coords.xy);

            // Blend the texture samples according to the computed weights.
            fixed4 c = cx * blend.x + cy * blend.y + cz * blend.z;
            // Multiply by occlusion map (sampled with the provided UVs)
            c *= tex2D(_OcclusionMap, IN.uv_OcclusionMap);

            // Output to the surface shader: the albedo gets the blended color.
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
