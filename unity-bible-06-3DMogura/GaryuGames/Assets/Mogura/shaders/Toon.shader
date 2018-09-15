Shader "Custom/ToonShader" {
	Properties {
	    _MainTex("MainTex(RGB)", 2D) = "white" {}
        _RampTex ("Shadow(RGB)", 2D) = "white"{}

        _OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(.0001, 0.5)) = 0.005

		_shadow ("Diffuse border", Range (0.01, 1)) = 0.2				// 影の境界
		_shadowBlur ("Diffuse border blur", Range (0.01, 0.2)) = 0.01	// 影のボケ具合
	}
	SubShader {





		// セルシェーダー
		pass {
			// Unityのライトオブジェクトを使ったLightMode
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			// Cgプログラムで使う変数
			sampler2D _MainTex;
		    sampler2D _RampTex;
			float  	_shadow;
			float  	_shadowBlur;

			// 頂点に入力されるデータ
		    struct appdata {
	            float4 vertex 	: POSITION;
	            float2 uv 		: TEXCOORD0;
	            float3 normal 	: NORMAL;
	        };

			// 頂点からピクセルに転送されるデータ
			struct v2f {
				float2 uv 		: TEXCOORD0; // カラーテクスチャ
				UNITY_FOG_COORDS(4)
				float4 pos : SV_POSITION; // 座標変換後の位置
				float3 L		: TEXCOORD1; // ライトベクトル
				float3 N 		: TEXCOORD2; // 法線ベクトル
			};


			// 頂点毎の処理
			v2f vert(appdata v) {
				v2f o;

				o.pos = UnityObjectToClipPos (v.vertex);	// 頂点位置をオブジェクト空間からカメラのクリップ空間へ変換
				UNITY_TRANSFER_FOG(o,o.pos);
				o.uv = v.uv;								// UVを取得

				// 法線ベクトル
				o.N = normalize(v.normal);

				// ライトベクトル
				o.L = normalize(-ObjSpaceLightDir(v.vertex));
				return o;
			}


			// ピクセル毎の処理
			float4 frag(v2f i) : COLOR {
				// 拡散反射の度合
				//half I_d = clamp( dot(i.L, i.N), 0, 1 );
				half I_d = dot(i.L, i.N);

				fixed4 col   = tex2D(_MainTex, i.uv);
				fixed4 shadow= tex2D(_RampTex, i.uv);

				// 影を塗る( I_d > _shadowであれば影色で塗る / __SpecularBorderBlurで影をボカす)
				fixed t_d = smoothstep( _shadow - _shadowBlur, _shadow + _shadowBlur, I_d);
				fixed4 c = lerp(col, col * shadow, t_d);

				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}
			ENDCG
		}


		// アウトライン描画
		Pass {
			Tags { "RenderType" = "Transparent" "Queue"="Transparent" }

			Cull Front
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				fixed4 color  : COLOR;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				UNITY_FOG_COORDS(1)
			};

			float _Outline;
			float4 _OutlineColor;


			v2f vert(appdata v)
			{
			    v2f o;
       			o.pos =UnityObjectToClipPos(v.vertex);										// 頂点を「オブジェクト空間 ⇒ ビュー空間」へ点を変換
        		float3 norm   = normalize(mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal));	// 頂点の法線方向をモデルビュー転置行列の逆行列に変換
       			float2 offset = TransformViewToProjection(norm.xy);							// ビューから見たx

      			o.pos.xy += offset * _Outline;	// 法線の拡大を行う
      			UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
					
			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_APPLY_FOG(i.fogCoord, _OutlineColor);
				return _OutlineColor;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}