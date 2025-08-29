#version 330

#define MAX_LIGHTS 8  

uniform sampler2D sceneTexture;
uniform vec2 screenSize;

uniform int lightCount;
uniform vec2 lightPositions[MAX_LIGHTS];
uniform vec3 lightColors[MAX_LIGHTS];
uniform float lightRadii[MAX_LIGHTS];
uniform float lightIntensities[MAX_LIGHTS];

in vec2 fragTexCoord;
out vec4 fragColor;

void main()
{
    vec2 pixelPos = fragTexCoord * screenSize;
    vec4 sceneColor = texture(sceneTexture, fragTexCoord);

    vec3 totalLight = vec3(0.0);

    for (int i = 0; i < lightCount; i++)
    {
        float dist = distance(pixelPos, lightPositions[i]);
        float attenuation = clamp(1.0 - dist / lightRadii[i], 0.0, 1.0);
        totalLight += lightColors[i] * attenuation * lightIntensities[i];
    }

    // Clamp to max brightness so no overflow
    totalLight = clamp(totalLight, 0.0, 1.0);

    vec3 finalColor = sceneColor.rgb * totalLight;

    fragColor = vec4(finalColor, sceneColor.a);
}
