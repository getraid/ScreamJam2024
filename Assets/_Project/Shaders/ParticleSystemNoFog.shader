Shader "Custom/CustomStandardShaderUnlit" {
    Properties {
        _MainTex ("Particle Texture", 2D) = "white" {}  // Texture of the particles
        _Color ("Main Color", Color) = (1,1,1,1)        // Tint color for particles
    }

    SubShader {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 200

        // Set the blend mode to support transparency (use Alpha Blending)
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"  // Includes Unity's built-in CG libraries

            // Define input structure from the mesh/particles system
            struct appdata_t {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            // Output structure from the vertex shader to the fragment shader
            struct v2f {
                float4 pos : SV_POSITION;  // Clip space position
                half4 color : COLOR;       // Vertex color * Tint color
                float2 uv : TEXCOORD0;     // Texture coordinates
            };

            // Define texture and color inputs for the particle material
            sampler2D _MainTex;  // Particle texture
            fixed4 _Color;       // Tint color

            // Vertex Shader: Transforms vertex positions and passes along data
            v2f vert (appdata_t v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);  // Convert to clip space
                o.uv = v.texcoord.xy;                    // Pass texture UV coordinates
                o.color = v.color * _Color;              // Combine vertex and tint colors
                return o;
            }

            // Fragment Shader: Calculates the final pixel color
            fixed4 frag (v2f i) : SV_Target {
                // Sample the texture and multiply by the color
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col;  // Output the final color without fog logic
            }
            ENDCG
        }
    }
}
