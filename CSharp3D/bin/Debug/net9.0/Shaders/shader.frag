﻿#version 330 core

out vec4 FragColor;

in vec3 fragPos;
in vec3 normal;
in vec3 barycentric;
in vec2 texCoord;
in float lightLevel;

uniform sampler2D texture0;
uniform vec3 camPos;
uniform vec4 fogColor;
uniform float fogDensity;
uniform float fogNear;
uniform float fogFar;
uniform float opacity;
uniform int enableFog;
uniform int enableLight;

float linearFog(float d, float near, float far)
{
    if (d < near) return 0.0;
    if (d > far) return 1.0;
    return clamp(1.0 - (far - d) / (far - near), 0.0, 1.0);
}

void main()
{
    vec4 color = texture(texture0, texCoord);
    
    if (enableLight == 1)
    {
        vec3 light = vec3(lightLevel * 0.7 + 0.3);
        color *= vec4(light, opacity);    
    }
    
    if (enableFog == 1)
    {
        float d = distance(camPos, fragPos);
        float fogFactor = linearFog(d, fogNear, fogFar);
        color = mix(color, fogColor, fogFactor);    
    }
    
    FragColor = color; 
}