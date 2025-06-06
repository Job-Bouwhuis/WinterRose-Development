#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform float fadeAmount;        // renamed from 'time' - fade progress 0.0 to 1.0
uniform bool fade;               // whether to run the animation
uniform bool reverseFade;        // reverse direction

out vec4 finalColor;

void main() {
    vec4 texColor = texture(texture0, fragTexCoord);
    
    // Early exit if fade is disabled
    if (!fade) {
        finalColor = texColor;
        return;
    }
    
    // Screen space Y coordinate (0.0 at top, 1.0 at bottom)
    float y = fragTexCoord.y;
    
    // Handle reverse direction
    if (reverseFade) {
        y = 1.0 - y;
    }
    
    // Calculate wave positions based on fadeAmount
    // Grayscale wave: moves from 0.0 to 1.0 over fadeAmount
    float grayscaleWave = fadeAmount;
    
    // Alpha wave: starts when grayscale reaches halfway (0.5)
    // Moves from 0.0 to 1.0, but offset by 0.5
    float alphaWave = max(0.0, fadeAmount - 0.5) * 2.0;
    
    // Apply grayscale effect with smooth transition
    vec3 finalRGB = texColor.rgb;
    
    if (y <= grayscaleWave + 0.05) {
        float gray = dot(texColor.rgb, vec3(0.299, 0.587, 0.114));
        vec3 grayColor = vec3(gray);
        
        // Smooth grayscale transition - areas above the wave are grayscaled
        float grayMix = smoothstep(grayscaleWave, grayscaleWave + 0.05, y);
        finalRGB = mix(grayColor, texColor.rgb, grayMix);
    }
    
    // Apply alpha fade effect with smooth transition
    float finalAlpha = texColor.a;
    
    // Smooth alpha transition - areas above the wave fade out
    float alphaMix = smoothstep(alphaWave, alphaWave + 0.05, y);
    finalAlpha = texColor.a * alphaMix;
    
    finalColor = vec4(finalRGB, finalAlpha);
}
