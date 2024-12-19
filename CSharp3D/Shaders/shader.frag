#version 330 core

out vec4 FragColor;

in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
    FragColor = texture(texture0, texCoord);
    // FragColor = vec4(texCoord, 0.0, 1.0);
}