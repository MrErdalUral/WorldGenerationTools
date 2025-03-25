Shader "Custom/PostProcessing/BayerMatrixDitherPostProcess"
{
    Properties
    {
        _MainTex ("Source Texture", 2D) = "white" {}
        _DitherScale ("Dither Scale", Float) = 1.0
        _Brightness ("Brightness", Float) = 2.0
        _DarkTone ("Dark Tone", Range(0,1)) = 0
		_Steps("Steps",Range(1,64)) = 4
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Overlay" }
        Cull Off ZTest Always ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float _DitherScale;
            float _Brightness;
            float _DarkTone;
            float _Steps;
            
            // Define a constant 4x4 Bayer matrix.
            // [  0,  8,  2, 10 ]
            // [ 12,  4, 14,  6 ]
            // [  3, 11,  1,  9 ]
            // [ 15,  7, 13,  5 ]
            static const float ditherMatrix[16] = {
                0.0,  8.0,  2.0, 10.0,
                12.0, 4.0, 14.0, 6.0,
                3.0,  11.0, 1.0,  9.0,
                15.0, 7.0, 13.0, 5.0
            };

            fixed4 frag(v2f_img i) : SV_Target
            {
                // Sample the rendered scene.
                fixed4 col = tex2D(_MainTex, i.uv);
                // Compute the luminance (brightness) using standard perceptual weights.
                float brightness = dot(col.rgb, float3(0.299, 0.587, 0.114) * _Brightness);
				if (_Steps > 1)
				{
					brightness = ceil(brightness * _Steps) / (_Steps - 1.0);
				}
                // Calculate the pixel coordinate from the full-screen UV.
                // _ScreenParams.xy contains the screen resolution in pixels.
                float2 pixelCoord = i.uv * _ScreenParams.xy;
                // Optionally scale the pattern frequency.
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
                // Normalize the threshold: add 0.5 and divide by 16.
                float threshold = (weight + 0.5) / 16.0;
                
                // Compare brightness against the calculated threshold.
                float result = brightness > threshold ? 1.0 : _DarkTone;
                
                return fixed4(col.rgb * result, col.a);
            }
            ENDCG
        }
    }
    FallBack Off
}
