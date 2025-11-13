#version 330

in vec3 fragNormal;
in vec3 fragWorldPos;

uniform vec3 lightDir;
uniform vec3 lightColor;
uniform vec3 ambientColor;

out vec4 finalColor;

void main()
{
    vec3 N = normalize(fragNormal);
    vec3 L = normalize(-lightDir);

    //vec3 debugNormal = fragNormal * 0.5 + 0.5;
    vec3 litColor = ambientColor + lightColor * max(dot(N, L), 0.001);
    finalColor = vec4(1, 1, 1, 1.0);
}
