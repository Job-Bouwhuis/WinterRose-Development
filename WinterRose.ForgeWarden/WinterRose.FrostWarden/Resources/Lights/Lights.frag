#version 330 core

uniform int lightCount;
uniform vec2 lightPositions[16];
uniform vec3 lightColors[16];
uniform float lightIntensities[16];
uniform float lightRadii[16];

uniform vec2 screenSize;

in vec2 fragTexCoord;
out vec4 outColor;

void main()
{
    vec2 pixelPos = fragTexCoord * screenSize; // Convert UV to screen space pixels

    vec3 lightAccum = vec3(0.0);

    for(int i = 0; i < lightCount; i++)
    {
        float dist = distance(pixelPos, lightPositions[i]);
        float intensity = clamp(1.0 - dist / lightRadii[i], 0.0, 1.0) * lightIntensities[i];
        lightAccum += lightColors[i] * intensity;
    }

    outColor = vec4(lightAccum, 1.0);
}
