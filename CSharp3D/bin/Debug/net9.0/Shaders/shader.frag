#version 330 core

out vec4 FragColor;

in vec3 fragPos;
in vec3 normal;
in vec2 texCoord;

uniform sampler2D texture0;
uniform vec3 camPos;

float linearFog(float d, float near, float far)
{
    if (d < near) return 0.0;
    if (d > far) return 1.0;
    return clamp(1.0 - (far - d) / (far - near), 0.0, 1.0);
}

void main()
{
    vec3 lightColor = vec3(1.0, 1.0, 1.0);
    vec3 lightPos = vec3(0.0, 1.0, 0.0);
    
    float ambientStrength = 0.7;
    vec3 ambient = ambientStrength * lightColor;
    
    vec3 lightDir = normalize(vec3(0.3, 1.0, 0.4));
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * lightColor * (1.0 - ambientStrength);
    
    vec4 color = texture(texture0, texCoord); // * (ambient + diffuse);
    float d = distance(camPos, fragPos);
    
    vec4 fogColor = vec4(135.0 / 255.0, 206. / 255., 245. / 255., 1.0);
    
    float fogDensity = 0.008;
    float fogFactor = linearFog(d, 100.0, 500.0);
    color = mix(color, fogColor, fogFactor);
    
    FragColor = color; 
    // FragColor = vec4(normal * 0.5 + 0.5, 1.0);
}