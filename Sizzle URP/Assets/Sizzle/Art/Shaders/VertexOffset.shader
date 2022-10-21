Shader "Unlit/Vertex Offset"
{
	Properties
	{
		_ColorA("Color A", Color) = (1,1,1,1)
		_ColorB("Color B", Color) = (1,1,1,1)
		_ColorStart("Color Start", Range(0,1)) = 0
		_ColorEnd("Color End", Range(0,1)) = 1

		_BrickDry("BrickDry", 2D) = "white"
		_BrickWet("BrickWet", 2D) = "white"
	}
		SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"



			float4 _ColorA;
			float4 _ColorB;

			sampler2D _BrickDry;
			sampler2D _BrickWet;

			float _ColorStart;
			float _ColorEnd;

			struct meshData
			{
				float4 vertex : POSITION;  // Vertex position 
				float3 normals: NORMAL;
				float4 tangents : TANGENT;
				float2 uv0 : TEXCOORD0; // uv coordinates
			};

			struct v2f
			{
				//float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 normals: NORMAL;
				float4 vertex : SV_POSITION;
				float2 uv : TEXCORD0;
			};


			v2f vert(meshData v)
			{
				v2f o;

				//float wave = abs(frac(v.uv0.y - _Time.y * .1) * 2 - .5);
				//v.vertex.y = wave;

				o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)); // object to world 
				o.vertex = UnityObjectToClipPos(v.vertex); // Local space to Clip space 
				o.normals = UnityObjectToWorldNormal(v.normals);
				o.uv = v.uv0; //(v.uv0 + _Offset) * _Scale; // Passthrough 
				return o;
			}

			float InverseLerp(float a, float b, float v)
			{
				return (v - a) / (b - a);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 topDownProjection = i.worldPos.xz;
				float4 col = tex2D(_BrickDry, topDownProjection );

				return col;
				//float t = saturate(InverseLerp(float4 (0, 1, 0, 1), float4(0, .5, 0, 1), float4(i.normal, 0)));
			//return float4(i.normal, 0);
			//return lerp(_ColorA, _ColorB, t);

				// Blend between the two colors 
				//float4 outColor = lerp(_ColorA, _ColorB, i.uv.x);

				//return outColor;
				// 


				//fixed4 colA = tex2D(_BrickDry, i.uv);
				//fixed4 colB = tex2D(_BrickWet, i.uv);

				//float t = saturate( InverseLerp(_ColorStart, _ColorEnd, i.uv.y));
				//float4 col = lerp(colA, colB, t);

				float wave = abs(frac(i.uv.y - _Time.y * .1) * 2 - .5);
				return wave;
				/*
				float topBottomRemover = (abs(i.normal.y) < 0.999f);
				float waves = t * topBottomRemover;

				float4 gradient = lerp(_ColorA, _ColorB, i.uv.y);

				return gradient * waves;
				*/
				//return col;
			}
			ENDCG
		}
	}
}