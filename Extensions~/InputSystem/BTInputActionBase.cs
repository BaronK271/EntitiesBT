using System;
using EntitiesBT.Components;
using EntitiesBT.Core;
using EntitiesBT.Nodes;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EntitiesBT.Extensions.InputSystem
{
    public class BTInputActionBase<TNode> : BTNode<TNode> where TNode : struct, IInputActionNodeData
    {
        [SerializeField] private InputActionReference InputAction;
        
        protected override void Build(ref TNode data, BlobBuilder _, ITreeNode<INodeDataBuilder>[] __)
        {
            data.ActionId = InputAction.action.id;
        }

        public override INodeDataBuilder Self => new BTVirtualDecorator<RunOnMainThreadNode>(this);
    }

    public interface IInputActionNodeData : INodeData
    {
        Guid ActionId { get; set; }
    }
}