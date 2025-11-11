using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ta.UIKit.Inputs;

namespace ta.UIKit.Nodes;

public class Node : IDisposable
{
    private static readonly Dictionary<Type, long> _byNodeTypeCounters = new();

    private readonly List<Node> _children = [];
    private Node[]? _childrenSnapshot;

    public Node(ILogger<Node>? logger = null)
    {
        Type thisType = GetType();
        NodeTypeIndex = _byNodeTypeCounters.GetOrAdd(thisType, 1);
        _byNodeTypeCounters[thisType] = NodeTypeIndex + 1;

        Name = thisType.Name + NodeTypeIndex;
        Children = _children.AsReadOnly();
        Logger = logger ?? UiKitPlugin.Instance?.Utils.GetLogger(GetType()) ?? NullLogger<Node>.Instance;
    }

    public static long AllNodesCounter { get; private set; } = 1;

    public static IReadOnlyDictionary<Type, long> NodeTypeCounters { get; } = new ReadOnlyDictionary<Type, long>(_byNodeTypeCounters);

    public long GlobalIndex { get; } = AllNodesCounter++;

    public long NodeTypeIndex { get; }

    public bool IsInitialized { get; private set; }

    public bool IsDisposed { get; private set; }

    public ProcessMode DrawMode { get; set; } = ProcessMode.AsParent;

    public ProcessMode UpdateMode { get; set; } = ProcessMode.AsParent;

    public bool IsVisible { get; private set; } = true;

    public bool IsPaused { get; private set; }

    public Node? Parent { get; private set; }

    public Node Root => Parent?.Root ?? this;

    public SceneRootNode? Scene => Root as SceneRootNode;

    public IReadOnlyList<Node> Children { get; }

    public string Name { get; set; }

    protected ILogger Logger { get; }

    public void AddChild(Node child)
    {
        ThrowIfDisposed();

        if (child == null) throw new ArgumentNullException(nameof(child));

        if (_children.Contains(child)) return;

        Logger.LogTrace("Adding child '{Child}' to node '{Node}'", child, this);

        // validate no circular references
        Node? current = this;
        while (current != null)
        {
            if (child == current) throw new InvalidOperationException("Circular reference detected.");

            current = Parent;
        }

        child.Parent?.RemoveChild(child);

        _children.Add(child);
        _childrenSnapshot = null;

        child.Parent = this;
        child.OnAttachedToParent(this);
        if (Scene != null) child.PropagateAttachedToScene(Scene);
    }

    public void AttachToParent(Node? parent)
    {
        ThrowIfDisposed();
        ThrowIfSceneRootProhibitedOperation();

        if (Parent == parent) return;

        Parent?.RemoveChild(this);
        parent?.AddChild(this);
    }

    public void DetachFromParent()
    {
        ThrowIfSceneRootProhibitedOperation();

        Parent?.RemoveChild(this);
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        Logger.LogTrace("Disposing node '{Node}'", this);

        DetachFromParent();

        Node[] snapshot = GetChildrenSnapshot();
        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            Node child = snapshot[i];
            child.Dispose();
        }

        try { OnDispose(); }
        catch (Exception e) { Logger.LogError(e, "Error while disposing node '{Node}': {Message}", this, e.Message); }

        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    public void Hide()
    {
        ThrowIfSceneRootProhibitedOperation();
        IsVisible = false;
    }

    public void Initialize()
    {
        ThrowIfDisposed();

        if (IsInitialized) return;

        Logger.LogTrace("Initializing node '{Node}'", this);
        OnInitialize();

        IsInitialized = true;
    }

    /// <summary>
    /// If child is not in the list, inserts a child into the list at the specified index.<br />
    /// If child is already in the list, moves the child to the specified index.
    /// </summary>
    /// <param name="child"> Child to insert. </param>
    /// <param name="index">
    /// List index where to insert the child.<br />
    /// If index is greater than the size of the list, child will be appended to the end of list.<br />
    /// If index is less than zero, child will be inserted relative to the end of list.<br />
    /// If index is less than zero and absolute value of index is greater than the size of the list, child will be inserted at the beginning of the
    /// list.
    /// </param>
    public void InsertChild(Node child, int index)
    {
        ThrowIfDisposed();

        if (child == null) throw new ArgumentNullException(nameof(child));

        if (index > _children.Count) index = _children.Count;
        if (index < 0) index = _children.Count + index + 1;
        if (index < 0) index = 0; // in case if index was negative and larger than the list size

        if (!_children.Contains(child))
        {
            // validate no circular references
            Node? current = this;
            while (current != null)
            {
                if (child == current) throw new InvalidOperationException("Circular reference detected.");

                current = Parent;
            }

            Logger.LogTrace("Inserting child '{Child}' to node '{Node}' at index {Index}", child, this, index);

            child.Parent?.RemoveChild(child);

            _children.Insert(index, child);
            _childrenSnapshot = null;

            child.Parent = this;
            child.OnAttachedToParent(this);
            if (Scene != null) child.PropagateAttachedToScene(Scene);
        }
        else
        {
            // handle "move child" case
            int childIdx = _children.IndexOf(child);
            if (childIdx == index) return; // already at the right index

            Logger.LogTrace("Moving child '{Child}' to node '{Node}' from index {OldIndex} to index {Index}", child, this, childIdx, index);

            _children.Remove(child);
            if (childIdx < index) index--; //after removing child, indexes will be shifted by 1

            _children.Insert(index, child);
            _childrenSnapshot = null;
        }
    }

    public void Pause()
    {
        ThrowIfSceneRootProhibitedOperation();
        IsPaused = true;
    }

    public void RemoveChild(Node child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));

        Logger.LogTrace("Removing child '{Child}' from node '{Node}'", child, this);

        if (Scene != null) child.PropagateDetachedFromScene(Scene);
        child.OnDetachedFromParent(this);
        child.Parent = null;

        _children.Remove(child);
        _childrenSnapshot = null;
    }

    public void Show()
    {
        ThrowIfSceneRootProhibitedOperation();
        IsVisible = true;
    }

    public void TogglePause()
    {
        ThrowIfSceneRootProhibitedOperation();
        IsPaused = !IsPaused;
    }

    public void ToggleVisibile()
    {
        ThrowIfSceneRootProhibitedOperation();
        IsVisible = !IsVisible;
    }

    public override string ToString()
    {
        return Name;
    }

    public void Unpause()
    {
        ThrowIfSceneRootProhibitedOperation();
        IsPaused = false;
    }

    internal void ProcessDraw(TimeSpan deltaTime)
    {
        if (IsDisposed) return;

        if (ShouldProcessDraw())
        {
            try { OnDraw(deltaTime); }
            catch (Exception e) { Logger.LogError(e, "Error while drawing node '{Node}': {Message}", this, e.Message); }
        }

        foreach (Node child in GetChildrenSnapshot()) { child.ProcessDraw(deltaTime); }
    }

    internal void ProcessInputEvent(InputEvent inputEvent)
    {
        if (IsDisposed) return;

        if (inputEvent == null) throw new ArgumentNullException(nameof(inputEvent));

        Node[] snapshot = GetChildrenSnapshot();
        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            Node child = snapshot[i];
            child.ProcessInputEvent(inputEvent);
        }

        if (ShouldProcessUpdate())
        {
            if (!inputEvent.IsAlreadyHandled)
            {
                try { OnUnhandledInputEvent(inputEvent); }
                catch (Exception e)
                {
                    Logger.LogError(e,
                        "Error while processing unhandled input event '{InputEvent}' in node '{Node}': {Message}",
                        inputEvent,
                        this,
                        e.Message);
                }
            }

            try { OnInputEvent(inputEvent); }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while processing input event '{InputEvent}' in node '{Node}': {Message}", inputEvent, this, e.Message);
            }
        }
    }

    internal void ProcessUpdate(TimeSpan deltaTime)
    {
        if (IsDisposed) return;

        Node[] snapshot = GetChildrenSnapshot();
        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            Node child = snapshot[i];
            child.ProcessUpdate(deltaTime);
        }

        if (ShouldProcessUpdate())
        {
            try { OnUpdate(deltaTime); }
            catch (Exception e) { Logger.LogError(e, "Error while updating node '{Node}': {Message}", this, e.Message); }
        }
    }

    internal void NotifySceneIsNowCurrent()
    {
        OnSceneEnter();

        Node[] snapshot = GetChildrenSnapshot();
        foreach (Node child in snapshot) { child.NotifySceneIsNowCurrent(); }
    }

    internal void NotifySceneNoLongerCurrent()
    {
        OnSceneExit();

        Node[] snapshot = GetChildrenSnapshot();
        foreach (Node child in snapshot) { child.NotifySceneNoLongerCurrent(); }
    }

    protected virtual void OnAttachedToParent(Node parent) { }

    protected virtual void OnAttachedToScene(SceneRootNode scene) { }

    protected virtual void OnDetachedFromParent(Node parent) { }

    protected virtual void OnDetachedFromScene(SceneRootNode scene) { }

    protected virtual void OnDispose() { }

    protected virtual void OnDraw(TimeSpan deltaTime) { }

    protected virtual void OnInitialize() { }

    protected virtual void OnInputEvent(InputEvent inputEvent) { }

    protected virtual void OnSceneEnter() { }

    protected virtual void OnSceneExit() { }

    protected virtual void OnUnhandledInputEvent(InputEvent inputEvent) { }

    protected virtual void OnUpdate(TimeSpan deltaTime) { }

    protected void ThrowIfDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
    }

    protected void ThrowIfNotInitialized()
    {
        if (!IsInitialized) throw new InvalidOperationException("Node is not initialized.");
    }

    protected void ThrowIfSceneRootProhibitedOperation()
    {
        if (this is SceneRootNode) throw new InvalidOperationException("Scene root node does not support this operation.");
    }

    private Node[] GetChildrenSnapshot()
    {
        _childrenSnapshot ??= _children.ToArray();
        return _childrenSnapshot;
    }

    private void PropagateAttachedToScene(SceneRootNode scene)
    {
        Logger.LogTrace("Node '{Node}' attached to scene '{Scene}'", this, scene);
        OnAttachedToScene(scene);

        Node[] snapshot = GetChildrenSnapshot();
        foreach (Node child in snapshot) { child.PropagateAttachedToScene(scene); }
    }

    private void PropagateDetachedFromScene(SceneRootNode scene)
    {
        Logger.LogTrace("Node '{Node}' detached from scene '{Scene}'", this, scene);
        OnDetachedFromScene(scene);

        Node[] snapshot = GetChildrenSnapshot();
        foreach (Node child in snapshot) { child.PropagateDetachedFromScene(scene); }
    }

    private bool ShouldProcessDraw()
    {
        if (this is SceneRootNode) return true;
        if (!IsVisible) return false;

        return DrawMode switch
        {
            ProcessMode.Never => false,
            ProcessMode.Always => true,
            ProcessMode.AsParent => Parent?.ShouldProcessDraw() ?? false,
            _ => throw new ArgumentOutOfRangeException(nameof(DrawMode), DrawMode, null),
        };
    }

    private bool ShouldProcessUpdate()
    {
        if (this is SceneRootNode) return true;
        if (IsPaused) return false;

        return UpdateMode switch
        {
            ProcessMode.Never => false,
            ProcessMode.Always => true,
            ProcessMode.AsParent => Parent?.ShouldProcessUpdate() ?? false,
            _ => throw new ArgumentOutOfRangeException(nameof(UpdateMode), UpdateMode, null),
        };
    }
}
