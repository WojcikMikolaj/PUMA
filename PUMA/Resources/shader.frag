#version 330 core

in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D sampler;
uniform vec3 color;

void main() {
    FragColor = texture(sampler, TexCoord);
    FragColor.xyz = color;
}