// This shader adds tessellation in URP
Shader "Example/URPUnlitShaderTessallated"
{
 
	// The properties block of the Unity shader. In this example this block is empty
	// because the output color is predefined in the fragment shader code.
	Properties
	{
		_Tess("Tessellation", Range(1, 32)) = 20
		_MaxTessDistance("Max Tess Distance", Range(1, 32)) = 20
		_Noise("Noise", 2D) = "gray" {}
		_ColorMap("Color Map", 2D) = "white" {}
		_UseColor("Use Flat Color", Float) = 0
		_FlatColor("Flat Color", Color) = (1,1,1,1)
		_Alpha("Alpha", Range(0,1)) = 1
 
		_Weight("Displacement Amount", Range(0, 1)) = 0
		_ColorMap_ST("Color Map Tiling", Vector) = (1,1,0,0)
		_Noise_ST("Noise Tiling", Vector) = (1,1,0,0)
	}
 
	// The SubShader block containing the Shader code. 
	SubShader
	{
		// SubShader Tags define when and under which conditions a SubShader block or
		// a pass is executed.
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
 
		Pass
		{
			Tags{ "LightMode" = "UniversalForward" "RenderType" = "Transparent" "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
 
			// The HLSL code block. Unity SRP uses the HLSL language.
			HLSLPROGRAM
			// The Core.hlsl file contains definitions of frequently used HLSL
			// macros and functions, and also contains #include references to other
			// HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"    
			#include "CustomTessellation.hlsl"
 
			// This line defines the name of the hull shader. 
			#pragma hull hull
			// This line defines the name of the domain shader. 
			#pragma domain domain
			// This line defines the name of the vertex shader. 
			#pragma vertex TessellationVertexProgram
			// This line defines the name of the fragment shader. 
			#pragma fragment frag
 
			sampler2D _Noise;
			float _Weight;
			sampler2D _ColorMap;
			float _UseColor;      // 0: usa textura, 1: usa color plano
			float4 _FlatColor;    // color plano con alpha
			float _Alpha;         // opacidad global
			float4 _ColorMap_ST;
			float4 _Noise_ST;
 
			// pre tesselation vertex program
			ControlPoint TessellationVertexProgram(Attributes v)
			{
				ControlPoint p;
 
				p.vertex = v.vertex;
				p.uv = v.uv;
				p.normal = v.normal;
				p.color = v.color;
 
				return p;
			}
 
			// after tesselation
			Varyings vert(Attributes input)
			{
				Varyings output;
 
				float2 noiseUV = input.uv * _Noise_ST.xy + _Noise_ST.zw;
				float4 Noise = tex2Dlod(_Noise, float4(noiseUV + (_Time.x * 0.1), 0, 0));
				input.vertex.xyz += normalize(input.normal) * Noise.r * _Weight;

				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.color = input.color;
				output.normal = input.normal;
				output.uv = input.uv;
 
				return output;
			}
 
			[UNITY_domain("tri")]
			Varyings domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
			{
				Attributes v;
				// interpolate the new positions of the tessellated mesh
				Interpolate(vertex)
				Interpolate(uv)
				Interpolate(color)
				Interpolate(normal)
 
				return vert(v);
			}
 
			// The fragment shader definition.            
			half4 frag(Varyings IN) : SV_Target
			{
				/*float4 Noise = tex2D(_Noise, IN.uv + (_Time.x * 0.1));
				return Noise;*/

				float2 tiledUV = IN.uv * _ColorMap_ST.xy + _ColorMap_ST.zw;
				float4 texColor = tex2D(_ColorMap, tiledUV + (_Time.x * 0.1));
			    float4 finalColor = lerp(texColor, _FlatColor, _UseColor);
				finalColor.a *= _Alpha;
				return finalColor;
			}
			ENDHLSL
		}
	}
}