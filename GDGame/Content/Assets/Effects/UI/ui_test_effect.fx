#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Practical game UI effect parameters
float Saturation; // 0.0 = grayscale, 1.0 = full color
float Brightness; // 0.0 = black, 1.0 = normal, >1.0 = brighter
float4 FlashColor; // Color for damage/heal flashes
float FlashAmount; // 0.0 = no flash, 1.0 = full flash
float FadeAlpha; // Global alpha multiplier (for fade in/out)

// Vignette (darkening at edges)
float VignetteStrength; // 0.0 = no vignette, 1.0 = strong
float VignetteSize; // Controls vignette radius

// Texture (SpriteBatch provides this automatically)
sampler s0;

// Vertex shader input
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// Vertex shader output / Pixel shader input
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// Helper: Convert color to grayscale
float GetLuminance(float3 color)
{
    // Standard luminance calculation
    return dot(color, float3(0.299, 0.587, 0.114));
}

// Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR0
{
    // Sample the texture
    float4 texColor = tex2D(s0, input.TexCoord);
    
    // Apply vertex color (SpriteBatch tint)
    float4 color = texColor * input.Color;
    
    // Early out if fully transparent (avoid unnecessary calculations)
    if (color.a < 0.001)
        return color;
    
    // Work with unpremultiplied color to avoid artifacts
    float3 rgb = color.rgb / color.a;
    
    // 1. SATURATION CONTROL
    float luminance = GetLuminance(rgb);
    rgb = lerp(float3(luminance, luminance, luminance), rgb, Saturation);
    
    // 2. BRIGHTNESS ADJUSTMENT
    rgb *= Brightness;
    
    // 3. FLASH EFFECT
    rgb = lerp(rgb, FlashColor.rgb, FlashAmount * FlashColor.a);
    
    // 4. VIGNETTE
    float2 centered = input.TexCoord - 0.5;
    float dist = length(centered);
    float vignette = smoothstep(VignetteSize, VignetteSize - 0.3, dist);
    rgb *= lerp(1.0, vignette, VignetteStrength);
    
    // 5. GLOBAL FADE
    float alpha = color.a * FadeAlpha;
    
    // Re-premultiply alpha
    return float4(rgb * alpha, alpha);
}

// Technique for SpriteBatch
technique SpriteBatch
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

