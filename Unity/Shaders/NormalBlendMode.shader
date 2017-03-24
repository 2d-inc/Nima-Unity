Shader "Nima/Normal"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 	
		}
		
		BlendOp Add
		Cull Off
		Blend One OneMinusSrcAlpha, One OneMinusSrcAlpha
		Zwrite Off
		ZTest Always
		/*Stencil {
			Ref 1
			Comp Equal
		}*/
		Pass
		{
			CGPROGRAM
			
			#include "UnityCG.cginc"
			
			#pragma vertex ComputeVertex
			#pragma fragment ComputeFragment
			
			sampler2D _MainTex;
			fixed4 _Color;
			
			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord : TEXCOORD0;
			};
			
			VertexOutput ComputeVertex (VertexInput vertexInput)
			{
				VertexOutput vertexOutput;
				
				vertexOutput.vertex = mul(UNITY_MATRIX_MVP, vertexInput.vertex);
				vertexOutput.texcoord = vertexInput.texcoord;
				vertexOutput.color = vertexInput.color * _Color;
							
				return vertexOutput;
			}
			
			fixed4 ComputeFragment (VertexOutput vertexOutput) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, vertexOutput.texcoord) * vertexOutput.color;
				color *= vertexOutput.color.a;
				return color;
			}
			
			ENDCG
		}
	}

	Fallback "Unlit/Transparent"
}