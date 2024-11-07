// BlurEffect.fx

matrix World;
matrix View;
matrix Projection;

texture SceneTexture;
sampler SceneSampler = sampler_state
{
    texture = <SceneTexture>;
};

float BlurAmount;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = 0;

    // Apply horizontal blur
    color += tex2D(SceneSampler, texCoord - float2(4 * BlurAmount, 0)) * 0.05;
    color += tex2D(SceneSampler, texCoord - float2(3 * BlurAmount, 0)) * 0.09;
    color += tex2D(SceneSampler, texCoord - float2(2 * BlurAmount, 0)) * 0.12;
    color += tex2D(SceneSampler, texCoord - float2(BlurAmount, 0)) * 0.15;
    color += tex2D(SceneSampler, texCoord) * 0.18;
    color += tex2D(SceneSampler, texCoord + float2(BlurAmount, 0)) * 0.15;
    color += tex2D(SceneSampler, texCoord + float2(2 * BlurAmount, 0)) * 0.12;
    color += tex2D(SceneSampler, texCoord + float2(3 * BlurAmount, 0)) * 0.09;
    color += tex2D(SceneSampler, texCoord + float2(4 * BlurAmount, 0)) * 0.05;

    return color;
}

technique BlurTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }

    pass Pass2
    {
        PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}
