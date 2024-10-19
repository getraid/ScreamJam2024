Shader "Custom/LightThroughFogShaderWithBloom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Base texture
        _Color ("Color", Color) = (1,1,1,1) // Base color
        _EmissionColor ("Emission Color", Color) = (1,1,1,1) // Emission color
        _EmissionStrength ("Emission Strength", Range(0,100)) = 1 // Emission intensity
        _BloomThreshold ("Bloom Threshold", Range(0,100)) = 1.2 // Threshold for bloom effect
        _BloomTimeDivider ("Bloom Time Multiplier", Range(0,100)) = 1 // higher is faster
        _BloomClampMin ("Bloom Clamp Min", Range(0,1)) = 0
        _BloomClampMax ("Bloom Clamp Min", Range(0,1)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
        } // Renders after opaque objects
        LOD 100

        Pass
        {
            // Disabling fog
            Fog
            {
                Mode Off
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc" //to use _Time

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _EmissionColor;
            float _EmissionStrength;
            float _BloomThreshold;
            float _BloomTimeDivider;
            float _BloomClampMin;
            float _BloomClampMax;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the base texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Calculate emission color
                fixed4 emission = _EmissionColor * _EmissionStrength;

                // _BloomThreshold = abs(sin(_Time.w / _BloomTimeDivider ));
                _BloomThreshold = abs(sin(_Time.y * _BloomTimeDivider ));
                
                
                 _BloomThreshold = clamp(_BloomThreshold, _BloomClampMin, _BloomClampMax);
                // Simulate bloom by boosting colors beyond a certain threshold
                if (_BloomThreshold > 0.01f)
                {
                    float bloomFactor = max(emission.r, max(emission.g, emission.b)) / _BloomThreshold;
                    emission.rgb *= bloomFactor;
                }

                // Combine the color with the "bloomed" emission
                return col + emission;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}