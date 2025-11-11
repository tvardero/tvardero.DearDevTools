using Microsoft.Extensions.ObjectPool;

namespace ta.UIKit.Nodes;

public class NodePool<TNode>
where TNode : Node
{
    private readonly ObjectPool<TNode> _pool;

    public NodePool(NodeFactory<TNode> factory)
    {
        var policy = new NodePoolPolicy(factory);
        _pool = new DefaultObjectPool<TNode>(policy);
    }

    public virtual TNode Get()
    {
        return _pool.Get();
    }

    public virtual void Return(TNode obj)
    {
        _pool.Return(obj);
    }

    private sealed class NodePoolPolicy : IPooledObjectPolicy<TNode>
    {
        private readonly NodeFactory<TNode> _factory;

        public NodePoolPolicy(NodeFactory<TNode> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        public TNode Create()
        {
            return _factory.Create();
        }

        /// <inheritdoc />
        public bool Return(TNode obj)
        {
            if (obj is IResettable resettable) return resettable.TryReset();

            return true;
        }
    }
}
