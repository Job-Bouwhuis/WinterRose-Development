#version 330

in vec3 fragNormal;
in vec3 fragWorldPos;

uniform vec3 lightDir;
uniform vec3 lightColor;
uniform vec3 ambientColor;

out vec4 finalColor;

void main()
{
    //vec3 lightColor = vec3(1.0, 1.0, 1.0);
    //vec3 ambientColor = vec3(0.5, 0.5, 0.5);

    vec3 N = normalize(fragNormal);
    vec3 L = normalize(-lightDir);

    //vec3 debugNormal = fragNormal * 0.5 + 0.5;
    vec3 litColor = ambientColor + lightColor * max(dot(N, L), 0.001);
    finalColor = vec4(litColor, 1.0);
    //finalColor = vec4(fragNormal, 1.0);
}
