Texture2D tex : TEXTURE : register(t0);

SamplerState splr : SAMPLER : register(s0);

float4 main(float2 tc : TEXCOORD) : SV_TARGET{
	float3 color = tex.Sample(splr, tc);
	return float4(color, 1.0f);
}