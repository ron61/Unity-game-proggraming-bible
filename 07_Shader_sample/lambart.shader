Shader "shader/lambart"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags { "LightMode" = "ForwardBase" } //……①

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc" //……②

			sampler2D _MainTex;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 normal : NORMAL; //……③
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 diffuse :COLOR0; //……④
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				float4 lightDir = dot(v.normal, ObjSpaceLightDir(v.vertex));
				o.diffuse = max(0, lightDir); //……⑤
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.diffuse * tex2D(_MainTex, i.uv); //……
				return col;
			}
			ENDCG
		}
	}
} 
