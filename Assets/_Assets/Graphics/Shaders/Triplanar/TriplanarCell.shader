Shader "Custom/TriplanarCell"
{
    Properties
    {
        _MainTexTop("Texture Top", 2D) = "white" {}
        _MainTexSide("Texture Side", 2D) = "white" {}
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _Tiling("Tiling", Float) = 1.0
        _SlopeTreshold("Slope", Float) = 0.0
        _Steps("Shading Steps", Range(2,8)) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Use a surface shader with custom toon lighting.
        #pragma surface surf ToonLighting
        #include "UnityCG.cginc"

        sampler2D _MainTexTop;
        sampler2D _MainTexSide;
        sampler2D _OcclusionMap;
        float _Tiling;
        float _SlopeTreshold;
        float _Steps;

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

            // Optionally modify the blend weights based on slope.
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
            // Multiply by occlusion map (sampled with the provided UVs).
            c *= tex2D(_OcclusionMap, IN.uv_OcclusionMap);

            // Output to the surface shader.
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        // Custom cell shading lighting function.
        half4 LightingToonLighting (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            // Compute standard Lambert diffuse term.
            half ndotl = saturate(dot(s.Normal, lightDir));
            // Quantize the lighting into discrete steps.
            ndotl = floor(ndotl * _Steps) / (_Steps - 1.0);
            half4 c;
            c.rgb = s.Albedo * ndotl * atten;
            c.a = 1.0;
            return c;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
