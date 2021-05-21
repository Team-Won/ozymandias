Shader "Custom/Leaves"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

		_Detail ("Detail Wind Texture", 2D) = "grey" {}
		_Strength ("Wind Strength", Float) = 1
		_DetailScale ("Detail Wind Scale", Float) = 1
		_DetailSpeed ("Detail Wind Speed", Float) = 1

		_Global ("Global Wind Texture", 2D) = "black" {}
		_Offset ("Global Wind Offset", Float) = 0
		_GlobalScale ("Global Wind Scale", Float) = 1
		_GlobalSpeed ("Global Wind Speed", Float) = 1

		[Toggle] _Debug ("Wind Debug", Float) = 1
		_Direction ("Wind Direction", Vector) = (1, 1, 1, 1)
        
    	// Seasonal effects
    	
    	_LerpTex ("Season Interpolation Texture", 2D) = "white" {}
    	
    	_BandTex ("Autumn Texture", 2D) = "white" {}
    	_NumBands ("Number of Colour Bands", Int) = 3
    	
    	_HueShiftTex ("Hue Shift Texture", 2D) = "white" {}

    	_Spring ("Spring", Range(0.0, 1.0)) = 0.0
    	    	
    	_SpringCol1 ("Spring Colour 1", Color) = (1, 1, 1, 1)
    	_SpringCol2 ("Spring Colour 2", Color) = (1, 1, 1, 1)
    	_SpringHueInfluence ("Spring Hue Influence", Range(0.0, 1.0)) = 0.3
        
    	_AutumnCol1 ("Autumn Colour 1", Color) = (1, 1, 1, 1)
    	_AutumnCol2 ("Autumn Colour 2", Color) = (1, 1, 1, 1)
    	_AutumnHueInfluence ("Autumn Hue Influence", Range(0.0, 1.0)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        	float4 world_position;
        	float4 autumn_uv;
        	float2 lerp_uv;
        	INTERNAL_DATA
        };

		#define PI 3.141592653589793238462

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

		sampler2D _Detail;
		half _DetailScale;
		half _DetailSpeed;
		half _Strength;

		sampler2D _Global;
		half _Offset;
		half _GlobalScale;
		half _GlobalSpeed;

		float4 _Direction;

		float _Debug;
        
		// Seasonal effects

        sampler2D _LerpTex;
        float4 _LerpTex_ST;
        
		sampler2D _BandTex;
        float4 _BandTex_ST;
        float _NumBands;
        
        sampler2D _HueShiftTex;
        float4 _HueShiftTex_ST;
        
        float _Spring;
        
        float _AutumnHueInfluence;
        float4 _AutumnCol1;
        float4 _AutumnCol2;

        float _SpringHueInfluence;
        float4 _SpringCol1;
        float4 _SpringCol2;
        
		
		float4 sample_wind(float2 detail_uv, float2 global_uv)
		{
			float3 detail_samp = tex2Dlod(_Detail, float4(detail_uv, 0, 0)).rgb;
			float global_samp = saturate(tex2Dlod(_Global, float4(global_uv, 0, 0)).r + _Offset);
			
			return float4(detail_samp, global_samp);
		}

		float3x3 angle_axis(float angle, float3 axis)
		{
			float c, s;
			sincos(angle, s, c);

			float t = 1 - c;
			float x = axis.x;
			float y = axis.y;
			float z = axis.z;

			return float3x3(
				t * x * x + c, t * x * y - s * z, t * x * z + s * y,
				t * x * y + s * z, t * y * y + c, t * y * z - s * x,
				t * x * z - s * y, t * y * z + s * x, t * z * z + c
				);
		}

		float rand(float3 co)
		{
			return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
		}

		void vert (inout appdata_full v, out Input o)
		{
			const float2 direction = normalize(float2(_Direction.x, _Direction.z));
			float4 world_pos = mul(unity_ObjectToWorld, v.vertex);
			const float4 origin_world = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));

			const float2 detail_uv = (float2(world_pos.x, world_pos.z) + direction * _Time[1] * _DetailSpeed) / _DetailScale;
			const float2 global_uv = (float2(world_pos.x, world_pos.z) + direction * _Time[1] * _GlobalSpeed) / _GlobalScale;

			const float4 wind_samp = sample_wind(detail_uv, global_uv);
			world_pos.xyz += wind_samp.rgb * wind_samp.a * _Strength;
			v.vertex = mul(unity_WorldToObject, world_pos);
			
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.world_position = world_pos;
			o.autumn_uv = float4(TRANSFORM_TEX(origin_world.xz, _BandTex), TRANSFORM_TEX(origin_world.xz, _HueShiftTex));
			o.lerp_uv = float2(TRANSFORM_TEX(origin_world.xz, _LerpTex));
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color PROBABLY UNNECESSARY WITH SEASONAL EFFECTS
            const fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			
			const fixed autumn_samp = round(tex2D(_BandTex, IN.autumn_uv.xy).r * _NumBands) / _NumBands;
			const fixed hue_samp = tex2D(_HueShiftTex, IN.autumn_uv.zw).r * 2 - 1;

			const fixed lerp_samp = smoothstep(_Spring, _Spring * 1.1, tex2D(_LerpTex, IN.lerp_uv).r);
			
			const fixed t = saturate(autumn_samp + hue_samp * lerp(_SpringHueInfluence, _AutumnHueInfluence, _Spring));
			
			const fixed3 autumn_c = lerp(_AutumnCol1, _AutumnCol2, t).rgb;
			const fixed3 spring_c = lerp(_SpringCol1, _SpringCol2, t).rgb;

			o.Albedo = lerp(spring_c, autumn_c, lerp_samp);

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
