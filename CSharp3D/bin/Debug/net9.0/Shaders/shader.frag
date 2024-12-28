#version 330 core

out vec4 FragColor;

in vec2 texCoord;

uniform sampler2D texture0;
uniform int blockId;

void main()
{
    vec2 texSize = vec2(16.0, 16.0);
    vec2 texOffset = vec2(blockId, (16.0 - 1.0));

    FragColor = texture(texture0, (texCoord + texOffset) / texSize);
}