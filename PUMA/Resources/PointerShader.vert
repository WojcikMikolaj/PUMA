#version 440 core

layout (location = 0) in vec3 in_Position;
//in vec3 in_Color;  
out vec4 vertexColor;
out vec4 localPos;

uniform mat4 mvp;

void main()
{
	localPos = vec4(in_Position,1.0);
	gl_Position = mvp * vec4(in_Position, 1.0);
	gl_PointSize = 5;
	vertexColor = vec4(1.0,1.0,0.0,1.0);
}