Shader "Custom/TriplanarDithered"
{
    Properties
    {
        _MainTexTop       ("Texture Top",       2D) = "white" {}
        _MainTexTopSnow   ("Texture Top Snow",  2D) = "white" {}
        _MainTexTopSand   ("Texture Top Sand",  2D) = "white" {}
        _MainTexSide      ("Texture Side",      2D) = "white" {}
        _OcclusionMap     ("Occlusion",         2D) = "white" {}

        _Tiling           ("Tiling",            Float) = 1.0
        _SlopeTresholdStart    ("Slope Start",             Float) = 0.0
        _SlopeTresholdEnd    ("Slope End",             Float) = 0.0

        // Height-based blending properties
        _SandStart        ("Sand Start",        Float) = 0.0
        _SandEnd          ("Sand End",          Float) = 6.0
        _SnowStart        ("Snow Start",        Float) = 25.0
        _SnowEnd          ("Snow End",          Float) = 30.0

		_DitherScale ("Dither Scale", Float) = 1.0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        // Use your custom lighting function
        #pragma surface surf DitheredLightingCustom fullforwardshadows
        #pragma target 3.0

        #include "UnityCG.cginc"

        sampler2D _MainTexTop;
        sampler2D _MainTexTopSnow;
        sampler2D _MainTexTopSand;
        sampler2D _MainTexSide;
        sampler2D _OcclusionMap;

		float4 _MainTexTop_ST;
		float4 _MainTexTopSnow_ST;
		float4 _MainTexTopSand_ST;
		float4 _MainTexSide_ST;

        float _Tiling;
        float _SlopeTresholdStart;
        float _SlopeTresholdEnd;

        // Height-based blending
        float _SandStart;
        float _SandEnd;
        float _SnowStart;
        float _SnowEnd;

        float _DitherScale;


        struct SurfaceOutputCustom
        {
            fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            half   Gloss;
            fixed  Alpha;
            half2  Dither; // Custom dither value
        };

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_OcclusionMap;
            float4 screenPos : SCREEN_POSITION;
        };

        // Helper function to remap a world Y into a [0..1] blend factor
        // given two thresholds (start < end).
        float BlendFactor (float y, float start, float end)
        {
            // Lerp between 0 and 1 if y is between [start..end].
            // Clamp via saturate so it never goes below 0 or above 1.
            return saturate((y - start) / (end - start));
        }

        void surf (Input IN, inout SurfaceOutputCustom o)
        {
            // --- Triplanar Texture Blending (for side vs top) ---
            float3 coords = IN.worldPos * _Tiling;
            float3 blend  = abs(IN.worldNormal);

            // If the normal's Y component exceeds threshold, treat top as fully dominant
            // else zero out Y so X and Z can dominate on steep slopes.
            
            blend.y = lerp(0, 1, BlendFactor(blend.y, _SlopeTresholdStart, _SlopeTresholdEnd));

            blend /= (blend.x + blend.y + blend.z + 1e-6);

            fixed4 cSideX = tex2D(_MainTexSide, coords.yz);
            fixed4 cSideZ = tex2D(_MainTexSide, coords.xy);

            // --- Height-Based Top Blending ---
            // 1. Compute how much we should blend up into default or snow
            //    vs. down into sand. We'll do this in two passes:
            //    - First pass: Sand -> Default
            //    - Second pass: (Sand/Default) -> Snow

            // Height in world coordinates
            float yVal = IN.worldPos.y;

            // Compute blend factors
            float sandToDefault = BlendFactor(yVal, _SandStart, _SandEnd);   // 0..1 between sand region and default region
            float defaultToSnow = BlendFactor(yVal, _SnowStart, _SnowEnd);   // 0..1 between default region and snow region

            // Grab each possible top texture
            fixed4 cSand    = tex2D(_MainTexTopSand,  coords.xz * _MainTexTopSand_ST.xy + _MainTexTopSand_ST.zw);
            fixed4 cDefault = tex2D(_MainTexTop,      coords.xz * _MainTexTop_ST.xy + _MainTexTop_ST.zw);
            fixed4 cSnow    = tex2D(_MainTexTopSnow,  coords.xz  * _MainTexTopSnow_ST.xy + _MainTexTopSnow_ST.zw);

            // First blend from Sand -> Default
            //   at y < _SandStart => 100% sand
            //   at y > _SandEnd   => 100% default
            fixed4 cSandDefault = lerp(cSand, cDefault, sandToDefault);

            // Then blend from that result into Snow
            //   at y < _SnowStart => no snow
            //   at y > _SnowEnd   => full snow
            fixed4 cTop = lerp(cSandDefault, cSnow, defaultToSnow);

            // Combine top & sides based on the slope blend factors
            fixed4 c = cSideX * blend.x + cTop * blend.y + cSideZ * blend.z;

            // Multiply by any occlusion
            c *= tex2D(_OcclusionMap, IN.uv_OcclusionMap);

            o.Albedo  = c.rgb;
            o.Alpha   = c.a;
            o.Emission= 0;

            // --- Compute a per-pixel dither value (unchanged) ---
            float2 screenUV   = IN.screenPos.xy / IN.screenPos.w;
            float2 pixelCoord = screenUV * _ScreenParams.xy;
            pixelCoord /= _DitherScale;

            o.Dither = half2(floor(pixelCoord.x), floor(pixelCoord.y));
        }

        // 4x4 Bayer matrix for dithering
        static const float ditherMatrix[16] = {
             0.0,  8.0,  2.0, 10.0,
            12.0,  4.0, 14.0,  6.0,
             3.0, 11.0,  1.0,  9.0,
            15.0,  7.0, 13.0,  5.0
        };

        half4 LightingDitheredLightingCustom(SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten)
        {
            // Basic Lambert term, then quantize
            half ndotl = saturate(dot(s.Normal, lightDir));
            ndotl *= atten;

            // Dither threshold calculation
            uint modX = (int)s.Dither.x % 4;
            uint modY = (int)s.Dither.y % 4;
            if (modX < 0) modX += 4;
            if (modY < 0) modY += 4;

            int   index     = modY * 4 + modX;
            float weight    = ditherMatrix[index];
            float threshold = (weight + 0.5) / 16.0;
            float result    = ndotl > threshold ? 1.0 : 0.0;

            // Multiply by Unity's light color, so the shader responds to scene lighting
            half3 color = s.Albedo * _LightColor0.rgb * result;

            half4 c;
            c.rgb = color;
            c.a   = s.Alpha;
            return c;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
