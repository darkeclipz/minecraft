#version 330 core

out vec4 FragColor;

in vec3 normal;
in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
    FragColor = texture(texture0, texCoord);
    // FragColor = vec4(normal * 0.5 + 0.5, 1.0);
}