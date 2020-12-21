Shader "Custom/ChunkShader"
{
    Properties
    {
		[NoScaleOffset] _TextureArray("Texture Array", 2DArray) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

			#include "SimplexNoise2D.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
                float3 UV : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 UV : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.UV = v.UV; //TRANSFORM_TEX(v.uv, _MainTex);
				if(v.normal.y > 0)
				{  // The vertex is a part of the Top quad.
					o.UV.z += 0;
				} else 
				if(v.normal.y < 0)
				{  // The vertex is a part of the Bot quad.
					o.UV.z += 2;
				}
				else
				{  // The vertex is a part of the Side quad.
					o.UV.z += 1;
				}

                return o;
            }

			UNITY_DECLARE_TEX2DARRAY(_TextureArray);

            fixed4 frag(v2f i) : SV_Target
            {
                return UNITY_SAMPLE_TEX2DARRAY(_TextureArray, i.UV);
            }

            ENDCG
        }
    }
}
