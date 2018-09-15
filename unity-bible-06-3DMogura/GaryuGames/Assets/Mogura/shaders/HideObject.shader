Shader "Custom/HideObject" {
    SubShader {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1"}
        Pass {
            ColorMask 0				// 何も描画しない

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 頂点座標をそのまま返す
            float4 vert(float4 v:POSITION) : SV_POSITION {
                return UnityObjectToClipPos(v); 
            }

            // 最低限の色だけを返す
            fixed4 frag() : COLOR {
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    } 
}