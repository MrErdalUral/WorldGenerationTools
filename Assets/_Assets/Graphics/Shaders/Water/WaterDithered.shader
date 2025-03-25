Shader "Custom/WaterDithered"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _DitherScale ("Dither Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        // Use the Standard lighting model with full forward shadows and enable fade blending.
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        float _DitherScale;

        struct Input
        {
            float2 uv_MainTex;
            // We need screen position to compute our dither threshold.
            float4 screenPos : SCREEN_POSITION;
        };
		static const float ditherMatrix[16] = {
                0.0,  8.0,  2.0, 10.0,
                12.0, 4.0, 14.0, 6.0,
                3.0,  11.0, 1.0,  9.0,
                15.0, 7.0, 13.0, 5.0
            };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample the main texture tinted by the _Color property.
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            // Normally we would set transparency as c.a:
            // o.Alpha = c.a;

            // --- Dithered Transparency ---
            // Convert the screen position to normalized coordinates.
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            // Convert to pixel coordinates using the built-in _ScreenParams (xy = resolution).
            float2 pixelCoord = screenUV * _ScreenParams.xy;
            // Scale the frequency of the dither pattern.
            pixelCoord /= _DitherScale;
            
            // Determine the cell position within a 4x4 grid.
            int modX = (int)floor(pixelCoord.x) % 4;
            int modY = (int)floor(pixelCoord.y) % 4;
            // Ensure indices are positive.
            if(modX < 0) modX += 4;
            if(modY < 0) modY += 4;
            int index = modY * 4 + modX;
                
            // Retrieve the corresponding weight from the 4x4 Bayer matrix.
            float weight = ditherMatrix[index];
			float threshold = (weight + 0.5) / 16.0;
            
            // Compare the original alpha (c.a) with the dither threshold.
            // If c.a exceeds the threshold, output full opacity (1.0); otherwise, fully transparent (0.0).
            o.Alpha = c.a > threshold ? 1.0 : 0.0;

        }
        ENDCG
    }
    FallBack "Transparent/VertexLit"
}
