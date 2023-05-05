
struct VSOut {
	float2 tex : TEXCOORD;
	float4 pos : SV_POSITION;
};

VSOut main(float3 pos : POSITION, float2 tex : TEXCOORD) {
	VSOut output;
	output.pos = float4(pos, 1.0f);
	output.tex = tex;
	return output;
}
