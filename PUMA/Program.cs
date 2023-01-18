using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TemplateProject.ImGuiUtils;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace TemplateProject;

public class Program : GameWindow
{
    public bool IsLoaded { get; private set; }

    private Shader defaultShader;
    private Shader pointerShader;
    private ImGuiController controller;
    private Mesh rectangle;
    private Camera camera;
    private Texture texture;

    private PUMA _puma;
    private PUMA _puma2;

    private Pointer _startingPointer;
    private Pointer _endingPointer;

    private ApplicationState _state;

    private float _dt = 0.2f;
    private float _currAnimTime = 0;
    private float _animTime = 5;
    
    public static void Main(string[] args)
    {
        using var program = new Program(GameWindowSettings.Default, NativeWindowSettings.Default);
        program.Title = "Project Title";
        program.Size = new Vector2i(1280, 800);
        program.Run();
    }

    public Program(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(
        gameWindowSettings, nativeWindowSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        defaultShader = new Shader(("shader.vert", ShaderType.VertexShader),
            ("shader.frag", ShaderType.FragmentShader));
        pointerShader = new Shader(("PointerShader.vert", ShaderType.VertexShader),
            ("PointerShader.frag", ShaderType.FragmentShader));
        controller = new ImGuiController(ClientSize.X, ClientSize.Y);

        camera = new Camera(new FirstPersonControl(), new PerspectiveView());

        float[] vertices =
        {
            0.5f, 0.5f, 2.0f,
            0.5f, -0.5f, 2.0f,
            -0.5f, -0.5f, 2.0f,
            -0.5f, 0.5f, 2.0f
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
        _puma = new PUMA(false);
        _puma2 = new PUMA(true);
        _startingPointer = new Pointer();
        _endingPointer = new Pointer();
        _startingPointer.Pos = (6, 0, 0);
        
        _puma.MoveToPoint(_startingPointer.Pos, _startingPointer.Rot);
        _puma.SetEndPoint(_endingPointer.Pos, _endingPointer.Rot);
        _puma.SetStartPoint(_startingPointer.Pos, _startingPointer.Rot);

        _puma2.MoveToPoint(_startingPointer.Pos, _startingPointer.Rot);
        _puma2.SetEndPoint(_endingPointer.Pos, _endingPointer.Rot);
        _puma2.SetStartPoint(_startingPointer.Pos, _startingPointer.Rot);

        //camera.Move(0,-10,0);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        rectangle.Dispose();
        controller.Dispose();
        texture.Dispose();
        defaultShader.Dispose();

        IsLoaded = false;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        base.OnResize(e);
        controller.WindowResized(ClientSize.X, ClientSize.Y);
        camera.Aspect = (float) Size.X / 2 / Size.Y;
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
        
        if (_state == ApplicationState.Animation)
        {
            _currAnimTime += _dt;
            _puma.CalculateCurrentConfiguration(_currAnimTime/_animTime);
            _puma2.CalculateCurrentConfiguration(_currAnimTime/_animTime);
            if (_currAnimTime > _animTime)
            {
                _state = ApplicationState.Default;
            }
        }

        

        //Lewa strona - dużo IK
        {
            GL.Viewport(0, 0, Size.X / 2, Size.Y);
            defaultShader.Use();
            texture.Use();
            defaultShader.LoadInteger("sampler", 0);
            defaultShader.LoadMatrix4("mvp", camera.GetProjectionViewMatrix());
            _puma.Render(defaultShader, pointerShader, camera.GetProjectionViewMatrix());


            pointerShader.Use();
            _startingPointer.Render(pointerShader, camera.GetProjectionViewMatrix());
            _endingPointer.Render(pointerShader, camera.GetProjectionViewMatrix());
        }

        //Prawa strona - 2 IK
        {
            GL.Viewport(Size.X / 2, 0, Size.X / 2, Size.Y);
            //GL.Clear(ClearBufferMask.DepthBufferBit); może?
            defaultShader.Use();
            texture.Use();
            defaultShader.LoadInteger("sampler", 0);
            defaultShader.LoadMatrix4("mvp", camera.GetProjectionViewMatrix());
            _puma2.Render(defaultShader, pointerShader, camera.GetProjectionViewMatrix());
            
            pointerShader.Use();
            _startingPointer.Render(pointerShader, camera.GetProjectionViewMatrix());
            _endingPointer.Render(pointerShader, camera.GetProjectionViewMatrix());
        }
        GL.Viewport(0, 0, Size.X, Size.Y);
        RenderGui();

        Context.SwapBuffers();
    }

    private void RenderGui()
    {
        //ImGui.ShowDemoWindow();

        ImGui.Begin("PUMA");
        if (ImGui.CollapsingHeader("Animacja"))
        {
            ImGui.Text("Pozycja startowa");
            System.Numerics.Vector3 sPos = _startingPointer.Pos.ToNumerics();
            if (ImGui.InputFloat3("sPos", ref sPos))
            {
                _startingPointer.Pos = sPos.ToOpenTK();
                _puma.MoveToPoint(_startingPointer.Pos, _startingPointer.Rot);
                _puma2.MoveToPoint(_startingPointer.Pos, _startingPointer.Rot);
                
                _puma.SetStartPoint(_startingPointer.Pos, _startingPointer.Rot);
                _puma2.SetStartPoint(_startingPointer.Pos, _startingPointer.Rot);
            }

            ImGui.Text("Rotacja startowa");
            System.Numerics.Vector3 sRot = _startingPointer.Rot.ToNumerics();
            if (ImGui.SliderFloat3("sRot", ref sRot, 0, 360))
            {
                _startingPointer.Rot = sRot.ToOpenTK();
                _puma.MoveToPoint(_startingPointer.Pos, _startingPointer.Rot);
                _puma2.MoveToPoint(_startingPointer.Pos, _startingPointer.Rot);
                
                _puma.SetStartPoint(_startingPointer.Pos, _startingPointer.Rot);
                _puma2.SetStartPoint(_startingPointer.Pos, _startingPointer.Rot);
            }

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.Text("Pozycja koncowa");
            System.Numerics.Vector3 ePos = _endingPointer.Pos.ToNumerics();
            if (ImGui.InputFloat3("ePos", ref ePos))
            {
                _endingPointer.Pos = ePos.ToOpenTK();
                _puma.SetEndPoint(_endingPointer.Pos, _endingPointer.Rot);
                _puma2.SetEndPoint(_endingPointer.Pos, _endingPointer.Rot);
            }

            ImGui.Text("Rotacja koncowa");
            System.Numerics.Vector3 eRot = _endingPointer.Rot.ToNumerics();
            if (ImGui.SliderFloat3("eRot", ref eRot, 0, 360))
            {
                _endingPointer.Rot = eRot.ToOpenTK();
                _puma.SetEndPoint(_endingPointer.Pos, _endingPointer.Rot);
                _puma2.SetEndPoint(_endingPointer.Pos, _endingPointer.Rot);
            }

            // var pumaSolutionNumber = _puma.SolutionNumber;
            // if (ImGui.SliderInt("num", ref pumaSolutionNumber, 0, 7))
            // {
            //     _puma.SolutionNumber = pumaSolutionNumber;
            //     _puma2.SolutionNumber = pumaSolutionNumber;
            // }

            ImGui.Text(_puma.endPosition.ToString());

            if (ImGui.Button("Start"))
            {
                _currAnimTime = 0;
                _state = ApplicationState.Animation;
            }

            ImGui.SliderFloat("predkosc animacji", ref _dt, 0.01f, 1);
            ImGui.SliderFloat("dlugosc animacji", ref _animTime, 0.01f, 10);
        }

        if (ImGui.CollapsingHeader("Parametry"))
        {
            float alpha1 = _puma.Alpha1;
            if (ImGui.SliderFloat("alpha1", ref alpha1, 0, 360))
            {
                _puma.Alpha1 = alpha1;
                _puma2.Alpha1 = alpha1;
            }

            float q2 = _puma.Q2;
            if (ImGui.SliderFloat("q2", ref q2, 1, 5))
            {
                _puma.Q2 = q2;
                _puma2.Q2 = q2;
            }

            float alpha2 = _puma.Alpha2;
            if (ImGui.SliderFloat("alpha2", ref alpha2, 0, 360))
            {
                _puma.Alpha2 = alpha2;
                _puma2.Alpha2 = alpha2;
            }

            float alpha3 = _puma.Alpha3;
            if (ImGui.SliderFloat("alpha3", ref alpha3, 0, 360))
            {
                _puma.Alpha3 = alpha3;
                _puma2.Alpha3 = alpha3;
            }

            float alpha4 = _puma.Alpha4;
            if (ImGui.SliderFloat("alpha4", ref alpha4, 0, 360))
            {
                _puma.Alpha4 = alpha4;
                _puma2.Alpha4 = alpha4;
            }

            float alpha5 = _puma.Alpha5;
            if (ImGui.SliderFloat("alpha5", ref alpha5, 0, 360))
            {
                _puma.Alpha5 = alpha5;
                _puma2.Alpha5 = alpha5;
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
                _puma2.L1 = l1;
            }

            if (ImGui.SliderFloat("l3", ref l3, 1, 5))
            {
                _puma.L3 = l3;
                _puma2.L3 = l3;
            }

            if (ImGui.SliderFloat("l4", ref l4, 1, 5))
            {
                _puma.L4 = l4;
                _puma2.L4 = l4;
            }

            float r = _puma.R;
            if (ImGui.SliderFloat("promien", ref r, 0.1f, 1))
            {
                _puma.R = r;
                _puma2.R = r;
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