#version 330 core

out vec4 FragColor;

in vec3 fragPos;
in vec3 normal;
in vec3 barycentric;
in vec2 texCoord;
in float lightLevel;

uniform sampler2D texture0;
uniform vec3 camPos;
uniform float lineThickness;

void main()
{
    float closestEdge = min(barycentric.x, min(barycentric.y, barycentric.z));
    float lineWidth = fwidth(closestEdge);
    float normalizedLineThickness = lineThickness / lineWidth;
    float alpha = smoothstep(normalizedLineThickness, normalizedLineThickness + lineWidth, closestEdge);
    FragColor = vec4(vec3(0), 1.0 - alpha);
}