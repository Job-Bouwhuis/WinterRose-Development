// Fragment Shader (basic.frag)
#version 330 core

in vec2 TexCoord;
in vec4 ourColor;

out vec4 FragColor;

uniform sampler2D texture1;

void main()
{
    FragColor = ourColor * texture(texture1, TexCoord);
}