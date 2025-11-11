using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ta.UIKit.Nodes;

namespace ta.UIKit.Test;

public class TestRegistration
{
    public static void Register()
    {
        UiKitPlugin.OnConfigureServices += services => { services.AddTransient<MyService>(); };
        UiKitPlugin.RebuildServiceProvider();

        var scene = UiKitPlugin.InstanceUtils.NodeFactory.Create<MyTestScene>();

        UiKitPlugin.InstanceUtils.SceneManager.SwitchScene(scene);
    }
}

public class MyService
{
    public string GetHello()
    {
        return "Hello";
    }
}

public class MyTestScene : SceneRootNode
{
    /// <inheritdoc />
    protected override void OnInitialize()
    {
        base.OnInitialize();

        var myHelloNode = UiKitPlugin.InstanceUtils.NodeFactory.Create<MyHelloNode>();
        AddChild(myHelloNode);
    }
}

public class MyHelloNode : Node
{
    private readonly MyService _myService;
    private TimeSpan _deltaElapsed = TimeSpan.Zero;

    /// <inheritdoc />
    public MyHelloNode(MyService myService, ILogger<MyHelloNode>? logger = null) : base(logger)
    {
        _myService = myService;
    }

    /// <inheritdoc />
    protected override void OnUpdate(TimeSpan deltaTime)
    {
        _deltaElapsed += deltaTime;
        if (_deltaElapsed > TimeSpan.FromSeconds(5))
        {
            var message = _myService.GetHello();
            Logger.LogInformation("Message from service: {Message}", message);
            _deltaElapsed = TimeSpan.Zero;
        }

        base.OnUpdate(deltaTime);
    }
}
