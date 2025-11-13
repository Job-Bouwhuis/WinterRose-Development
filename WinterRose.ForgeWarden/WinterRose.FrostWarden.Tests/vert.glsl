#version 330 core

// Explicit attribute locations to match typical mesh attribute bindings.
// Adjust locations if your mesh uses different attribute indices.
layout(location = 0) in vec3 vertexPosition;
layout(location = 1) in vec3 vertexNormal;
layout(location = 2) in vec2 vertexTexCoord;

uniform mat4 mvp;
uniform mat4 matModel;

out vec3 fragNormal;
out vec3 fragWorldPos;
out vec2 fragTexCoord;

void main()
{
    // Transform to world space
    vec4 worldPos = matModel * vec4(vertexPosition, 1.0);
    fragWorldPos = worldPos.xyz;

    // Compute normal matrix and normalize the transformed normal
    mat3 normalMatrix = mat3(transpose(inverse(matModel)));
    fragNormal = normalize(normalMatrix * vertexNormal);

    // Pass through texcoord (if fragment shader will use it)
    fragTexCoord = vertexTexCoord;

    // Final clip-space position
    gl_Position = mvp * worldPos;
}
