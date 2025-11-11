using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ta.UIKit.Nodes;

public abstract class NodeFactory<TNode>
where TNode : Node
{
    public abstract TNode Create();
}

public class NodeFactory
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly Dictionary<Type, object> _parameterlessFactories = new();

    public NodeFactory(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
    }

    public virtual TNode Create<TNode>()
    where TNode : Node
    {
        return ResolveFactory<TNode>().Create();
    }

    protected virtual NodeFactory<TNode> ResolveFactory<TNode>()
    where TNode : Node
    {
        if (_serviceProvider != null) return _serviceProvider.GetRequiredService<NodeFactory<TNode>>();

        Type nodeType = typeof(TNode);
        if (nodeType.GetConstructors().All(c => c.GetParameters().Length != 0))
            throw new InvalidOperationException("No parameterless constructor found for node type " + nodeType.FullName + ".");

        object factory = _parameterlessFactories.GetOrAdd(nodeType,
            () =>
            {
                Type factoryType = typeof(ParameterlessConstructorNodeFactory<>).MakeGenericType(nodeType);
                return Activator.CreateInstance(factoryType);
            });

        return (NodeFactory<TNode>)factory;
    }
}
