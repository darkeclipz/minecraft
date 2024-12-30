#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aBarycentric;
layout (location = 3) in vec2 aTexCoord;
layout (location = 4) in float aLight;

out vec3 fragPos;
out vec3 normal;
out vec3 barycentric;
out vec2 texCoord;
out float lightLevel;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    fragPos = (model * vec4(aPosition, 1.0)).xyz;
    normal = aNormal;
    barycentric = aBarycentric;
    texCoord = aTexCoord;
    lightLevel = aLight;
}