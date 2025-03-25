Shader "Custom/PostProcessing/BlueNoiseDitherPostProcess"
{
    Properties
    {
        _MainTex ("Source Texture", 2D) = "white" {}
        _BlueNoiseTex ("Blue Noise Texture", 2D) = "white" {}
        _BlueNoiseScale ("Blue Noise Scale", Float) = 1.0
        _Brightness ("Brightness", Float) = 2.0
        _DarkTone ("Dark Tone", Range(0,1)) = 0
		_Steps("Steps",Range(1,64)) = 4

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off ZTest Always ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BlueNoiseTex;
            float _BlueNoiseScale;
            float _Brightness;
            float _DarkTone;
            float _Steps;
            
            fixed4 frag(v2f_img i) : SV_Target
            {
                // Sample the source rendered scene.
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Compute luminance (brightness) using standard perceptual weights.
                float brightness = dot(col.rgb, float3(0.299, 0.587, 0.114)) * _Brightness;
				 if (_Steps > 1)										   
				 {
				 	brightness = floor(brightness * _Steps) / (_Steps - 1.0);
				 }
                // Calculate blue noise UV coordinates.
                // Multiplying by _BlueNoiseScale controls the tiling frequency of the blue noise pattern.
                float2 noiseUV = i.uv * _BlueNoiseScale;
                // Sample the blue noise texture. It should ideally be a grayscale texture.
                float noiseValue = tex2D(_BlueNoiseTex, noiseUV).r;											 
                
                // Compare the brightness against the noise value.
                // If brightness exceeds the noise threshold, the pixel is considered lit (result=1), otherwise dark (_DarkTone).
                float result = brightness > noiseValue ? 1.0 : _DarkTone;
                
                return fixed4(col.rgb * result, col.a);
            }
            ENDCG
        }
    }
    FallBack Off
}
