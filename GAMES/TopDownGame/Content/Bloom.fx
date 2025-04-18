#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// === UNIFORMS ===
float2 Resolution;
float Threshold = 0.6f;

Texture2D InputTexture;
sampler InputSampler = sampler_state
{
    Texture = <InputTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

// === VERTEX INPUT/OUTPUT ===
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// === VERTEX SHADER ===
VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = input.Position;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

// === PIXEL SHADERS ===
float4 BrightPassPS(VertexShaderOutput input) : COLOR0
{
    float4 color = tex2D(InputSampler, input.TexCoord);
    float luminance = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
    return (luminance > Threshold) ? color : float4(0, 0, 0, 1);
}

float4 BlurH_PS(VertexShaderOutput input) : COLOR0
{
    float2 offset = float2(1.0 / Resolution.x, 0.0);
    float4 color = float4(0, 0, 0, 0);
    color += tex2D(InputSampler, input.TexCoord + offset * -4.0) * 0.05;
    color += tex2D(InputSampler, input.TexCoord + offset * -2.0) * 0.09;
    color += tex2D(InputSampler, input.TexCoord) * 0.62;
    color += tex2D(InputSampler, input.TexCoord + offset * 2.0) * 0.09;
    color += tex2D(InputSampler, input.TexCoord + offset * 4.0) * 0.05;
    return color;
}

float4 BlurV_PS(VertexShaderOutput input) : COLOR0
{
    float2 offset = float2(0.0, 1.0 / Resolution.y);
    float4 color = float4(0, 0, 0, 0);
    color += tex2D(InputSampler, input.TexCoord + offset * -4.0) * 0.05;
    color += tex2D(InputSampler, input.TexCoord + offset * -2.0) * 0.09;
    color += tex2D(InputSampler, input.TexCoord) * 0.62;
    color += tex2D(InputSampler, input.TexCoord + offset * 2.0) * 0.09;
    color += tex2D(InputSampler, input.TexCoord + offset * 4.0) * 0.05;
    return color;
}

float4 CombinePS(VertexShaderOutput input) : COLOR0
{
    float4 baseColor = tex2D(InputSampler, input.TexCoord);
    float4 bloomColor = tex2D(InputSampler, input.TexCoord);
    return saturate(baseColor + bloomColor);
}

// === TECHNIQUES ===
technique BrightPass
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BrightPassPS();
    }
}

technique BlurHorizontal
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BlurH_PS();
    }
}

technique BlurVertical
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BlurV_PS();
    }
}

technique Combine
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL CombinePS();
    }
}
