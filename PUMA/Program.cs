using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TemplateProject.ImGuiUtils;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;

namespace TemplateProject;

public class Program : GameWindow
{
    public bool IsLoaded { get; private set; }
    
    private Shader shader;
    private ImGuiController controller;
    private Mesh rectangle;
    private Camera camera;
    private Texture texture;

    private PUMA _puma;

    public static void Main(string[] args)
    {
        using var program = new Program(GameWindowSettings.Default, NativeWindowSettings.Default);
        program.Title = "Project Title";
        program.Size = new Vector2i(1280, 800);
        program.Run();
    }

    public Program(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

    protected override void OnLoad()
    {
        base.OnLoad();

        shader = new Shader(("shader.vert", ShaderType.VertexShader), ("shader.frag", ShaderType.FragmentShader));
        controller = new ImGuiController(ClientSize.X, ClientSize.Y);

        camera = new Camera(new FirstPersonControl(), new PerspectiveView());

        float[] vertices = {
            0.5f,  0.5f, 2.0f,
            0.5f, -0.5f, 2.0f,
            -0.5f, -0.5f, 2.0f,
            -0.5f,  0.5f, 2.0f
        };
        float[] texCoords =
        {
            0.0f, 0.0f,
            0.0f, 1.0f,
            1.0f, 1.0f,
            1.0f, 0.0f
        };
        int[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };
        rectangle = new Mesh(PrimitiveType.Triangles, indices, (vertices, 0, 3), (texCoords, 1, 2));

        texture = new Texture("texture.jpg");

        GL.ClearColor(0.4f, 0.7f, 0.9f, 1.0f);
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        IsLoaded = true;

        InitializeScene();
    }

    private void InitializeScene()
    {
        _puma = new PUMA();
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        rectangle.Dispose();
        controller.Dispose();
        texture.Dispose();
        shader.Dispose();

        IsLoaded = false;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
        controller.WindowResized(ClientSize.X, ClientSize.Y);
        camera.Aspect = (float) Size.X / Size.Y;
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        controller.Update(this, (float) args.Time);

        if (ImGui.GetIO().WantCaptureMouse) return;

        var keyboard = KeyboardState.GetSnapshot();
        var mouse = MouseState.GetSnapshot();

        camera.HandleInput(keyboard, mouse, (float) args.Time);

        if (keyboard.IsKeyDown(Keys.Escape)) Close();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.Use();
        texture.Use();
        shader.LoadInteger("sampler", 0);
        shader.LoadMatrix4("mvp", camera.GetProjectionViewMatrix());
        //rectangle.Render();
        _puma.Render(shader, camera.GetProjectionViewMatrix());
        RenderGui();

        Context.SwapBuffers();
    }

    private void RenderGui()
    {
        //ImGui.ShowDemoWindow();

        ImGui.Begin("PUMA");

        if (ImGui.CollapsingHeader("Parametry"))
        {
            float alpha1 = _puma.Alpha1;
            if (ImGui.SliderFloat("alpha1", ref alpha1, 0, 360))
            {
                _puma.Alpha1 = alpha1;
            }
            
            float q2 = _puma.Q2;
            if(ImGui.SliderFloat("q2", ref q2, 1, 5))
            {
                _puma.Q2 = q2;
            }
            
            float alpha2 = _puma.Alpha2;
            if (ImGui.SliderFloat("alpha2", ref alpha2, 0, 360))
            {
                _puma.Alpha2 = alpha2;
            }
            
            float alpha3 = _puma.Alpha3;
            if (ImGui.SliderFloat("alpha3", ref alpha3, 0, 360))
            {
                _puma.Alpha3 = alpha3;
            }
            
            float alpha4 = _puma.Alpha4;
            if (ImGui.SliderFloat("alpha4", ref alpha4, 0, 360))
            {
                _puma.Alpha4 = alpha4;
            }
            
            float alpha5 = _puma.Alpha5;
            if (ImGui.SliderFloat("alpha5", ref alpha5, 0, 360))
            {
                _puma.Alpha5 = alpha5;
            }
        }

        if (ImGui.CollapsingHeader("Ustawienia"))
        {
            float l1 = _puma.L1;
            float l3 = _puma.L3;
            float l4 = _puma.L4;

            if (ImGui.SliderFloat("l1", ref l1, 1, 5))
            {
                _puma.L1 = l1;
            }
            if(ImGui.SliderFloat("l3", ref l3, 1, 5))
            {
                _puma.L3 = l3;
            }
            if(ImGui.SliderFloat("l4", ref l4, 1, 5))
            {
                _puma.L4 = l4;
            }

            float r = _puma.R;
            if (ImGui.SliderFloat("promien", ref r, 0.1f, 1))
            {
                _puma.R = r;
            }
        }

        ImGui.End();

        controller.Render();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        controller.PressChar((char) e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        controller.MouseScroll(e.Offset);
    }
}