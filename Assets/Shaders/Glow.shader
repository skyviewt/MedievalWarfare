Shader "Glow" {
	Properties 
	{
		_ColorTint("Color Tint", Color) = (1, 1, 1, 0.2)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "Bump" {}
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
		_RimPower("Rim Power", Range(0.1, 1) ) = 0.5
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input 
		{
			float4 color : Color;
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 ViewDir;
		};

		float4 _ColorTint;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		float4 _RimColor;
		float _RimPower;
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			IN.color = _ColorTint;
			o.Albedo = tex2D ( _MainTex, IN.uv_MainTex).rgb * IN.color;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			//rim light creates color based on cam position
			//based com cam normal surface 
			// 1 - : means we are not geting the color on perpendicular one
			
			half rim = 1.0 - saturate(dot(normalize(IN.ViewDir), o.Normal));
			o.Emission = _RimColor.rgb * pow(rim, _RimPower);
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
