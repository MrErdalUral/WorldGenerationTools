Shader "Custom/WaterDitheredDepth"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ReflectionTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _FlowMap ("Flow (RG, A noise)", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _DitherScale ("Dither Scale", Float) = 1.0
        _WaterFogDensity ("Water Fog Density", Range(0, 2)) = 0.1
        _ShoreFogDensity ("Shore Fog Density", Range(0, 10)) = 2
		_Fresnel("Reflection", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  "Queue" = "Geometry+1" }
        LOD 200

        CGPROGRAM
        // Use the Standard lighting model with full forward shadows and enable fade blending.
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0
		#include "DepthFog.cginc"
		#include "Flow.cginc"
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		sampler2D _ReflectionTex, _FlowMap;
		float _UJump, _VJump, _Tiling, _Speed, _FlowStrength, _FlowOffset;
		float _HeightScale, _HeightScaleModulated;

        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        float _DitherScale;
		float _Fresnel;

        struct Input
        {
			float2 uv_MainTex;
            // We need screen position to compute our dither threshold.
            float4 screenPos : SCREEN_POSITION;
			float3 worldPos : TEXCOORD0;
        };

		static const float ditherMatrix[16] = {
                0.0,  8.0,  2.0, 10.0,
                12.0, 4.0, 14.0, 6.0,
                3.0,  11.0, 1.0,  9.0,
                15.0, 7.0, 13.0, 5.0
        };
		
		float3 UnpackDerivativeHeight (float4 textureData) {
			float3 dh = textureData.agb;
			dh.xy = dh.xy * 2 - 1;
			return dh;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float noise = tex2D(_FlowMap, IN.worldPos.xz).a;
			float time = _Time.x * 5 + noise;
			float3 flow = tex2D(_FlowMap, IN.worldPos.xz*0.31 + time*0.1).rgb + 
				tex2D(_FlowMap, IN.worldPos.xz*0.11 + time*0.3).rgb + 
				tex2D(_FlowMap, IN.worldPos.xz*0.23 + time*0.2).rgb;
			flow.xy = (flow.xy) * 2 /3	 - 1;
			
			IN.screenPos.xy += flow.xy * 0.005;

            // Use _Color for the water's base properties (RGB values remain untouched).
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            fixed4 c = _Color ;

            // --- Dithered Transparency ---
            float2 pixelCoord = screenUV * _ScreenParams.xy;
            pixelCoord /= _DitherScale;
            
            uint modX = (int)floor(pixelCoord.x) % 4;
            uint modY = (int)floor(pixelCoord.y) % 4;
            if(modX < 0) modX += 4;
            if(modY < 0) modY += 4;
            int index = modY * 4 + modX;
                
            float weight = ditherMatrix[index];
			float threshold = (weight + 0.5) / 16.0;
			
            // Existing water fog factor (could be influenced by depth, waves, etc.).
			float fogFactor = CalculateWaterFactor(IN.screenPos, o.Normal);
			fogFactor += (sin((_Time.w *1.1 + IN.worldPos.x) / 11)* 
				sin((_Time.w *2.3 + IN.worldPos.x) / 13) * 
				sin((_Time.w *0.7 + IN.worldPos.z) / 7) * 
				sin((_Time.w *3.1 + IN.worldPos.z+IN.worldPos.x) / 17)) * 0.2+0.2;
            
            // --- Fresnel Term for Viewing Angle ---
            // Compute the view direction from the water surface point to the camera.
            float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
            // The Fresnel term increases reflection when the view angle is grazing.
            float fresnel = pow(1.0 - saturate(dot(viewDir, o.Normal)), 3.0);
            
            // Combine alpha modifiers: base alpha * fogFactor
            float finalAlpha = c.a * (fogFactor);
			c = lerp(c,tex2D(_ReflectionTex,float2(1-screenUV.x,screenUV.y)), fresnel * _Fresnel);
            o.Albedo = c.rgb;
			// Use the dither pattern threshold to determine full or no alpha.
			o.Alpha = finalAlpha > threshold ? 1.0 : 0.0;
        }
        ENDCG
    }
    FallBack "Transparent/VertexLit"
}