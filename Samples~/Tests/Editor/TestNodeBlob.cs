using System.Linq;
using EntitiesBT.Editor;
using EntitiesBT.Nodes;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace EntitiesBT.Test
{
    public class TestNodeBlob : BehaviorTreeTestBase
    {
        [Test]
        public unsafe void should_able_to_create_and_fetch_data_from_node_blob()
        {
            Debug.Log($"sizeof NodeA: {sizeof(NodeDataA)}");
            Debug.Log($"sizeof NodeB: {sizeof(NodeDataB)}");
            
            var size = sizeof(NodeDataA) + sizeof(NodeDataB);
            using (var blobBuilder = new BlobBuilder(Allocator.Temp))
            {
                ref var blob = ref blobBuilder.ConstructRoot<NodeBlob>();
                var types = blobBuilder.Allocate(ref blob.Types,  3);
                types[0] = 123;
                types[1] = 234;
                types[2] = 345;
                
                var endIndices = blobBuilder.Allocate(ref blob.EndIndices, 3);
                endIndices[0] = 3;
                endIndices[1] = 2;
                endIndices[2] = 3;
                
                var offsets = blobBuilder.Allocate(ref blob.Offsets,  3);
                var unsafePtr = (byte*) blobBuilder.Allocate(ref blob.DataBlob, size).GetUnsafePtr();
                var offset = 0;
                offsets[0] = offset;
                offsets[1] = offset;
                UnsafeUtilityEx.AsRef<NodeDataA>(unsafePtr + offset).A = 111;
                offset += sizeof(NodeDataA);
                offsets[2] = offset;
                ref var local2 = ref UnsafeUtilityEx.AsRef<NodeDataB>(unsafePtr + offset);
                local2.B = 222;
                local2.BB = 2222;
                var blobRef = blobBuilder.CreateBlobAssetReference<NodeBlob>(Allocator.Persistent);
                try
                {
                    Assert.IsTrue(blobRef.IsCreated);
                    Assert.AreEqual(blobRef.Value.DataBlob.Length, size);
                    Assert.AreEqual(blobRef.Value.Count, 3);
                    
                    Assert.AreEqual(blobRef.Value.Types[0], 123);
                    Assert.AreEqual(blobRef.Value.Types[1], 234);
                    Assert.AreEqual(blobRef.Value.Types[2], 345);
                    
                    Assert.AreEqual(blobRef.Value.EndIndices[0], 3);
                    Assert.AreEqual(blobRef.Value.EndIndices[1], 2);
                    Assert.AreEqual(blobRef.Value.EndIndices[2], 3);
                    
                    Assert.AreEqual(blobRef.Value.GetNodeData<NodeDataA>(1).A, 111);
                    ref var b = ref blobRef.Value.GetNodeData<NodeDataB>(2);
                    Assert.AreEqual(b.B, 222);
                    Assert.AreEqual(b.BB, 2222);
                } finally
                {
                    if (blobRef.IsCreated) blobRef.Dispose();
                }
            }
        }

        [Test]
        public void should_create_behavior_tree_objects_from_single_line_of_string()
        {
            var root = CreateBTNode("!seq>yes|yes|b:1,1|a:111");
            Assert.AreEqual(root.name, "BTSequence");
            Assert.AreEqual(root.transform.childCount, 4);
            
            var children = root.Children<BTNode>().ToArray();
            
            Assert.AreEqual(children[0].name, "BTTestNodeState");
            Assert.AreEqual(children[0].transform.childCount, 0);
            
            Assert.AreEqual(children[1].name, "BTTestNodeState");
            Assert.AreEqual(children[1].transform.childCount, 0);
            
            Assert.AreEqual(children[2].name, "BTTestNodeB");
            Assert.AreEqual(children[2].transform.childCount, 0);
            
            Assert.AreEqual(children[3].name, "BTTestNodeA");
            Assert.AreEqual(children[3].transform.childCount, 0);
        }

        [Test]
        public void should_generate_blob_from_nodes()
        {
            var root = CreateBTNode("!seq>yes|no|b:1,1|a:111|run");
            var rootNode = root.GetComponent<BTNode>();
            var blobRef = rootNode.ToBlob(Factory);
            Assert.True(blobRef.IsCreated);
            Assert.AreEqual(blobRef.Value.Count, 6);
            
            var types = new[] { typeof(SequenceNode), typeof(TestNode), typeof(TestNode), typeof(NodeB), typeof(NodeA), typeof(TestNode) };
            Assert.AreEqual(blobRef.Value.Types.ToArray(), types.Select(Factory.GetTypeId).ToArray());
            Assert.AreEqual(blobRef.Value.Offsets.ToArray(), new [] { 0, 0, 4, 8, 16, 20 });
            Assert.AreEqual(blobRef.Value.EndIndices.ToArray(), new [] { 6, 2, 3, 4, 5, 6 });
            Assert.AreEqual(blobRef.Value.DataBlob.Length, 24);
            Assert.AreEqual(blobRef.Value.GetNodeData<NodeDataB>(3), new NodeDataB {B = 1, BB = 1});
            Assert.AreEqual(blobRef.Value.GetNodeData<NodeDataA>(4), new NodeDataA {A = 111});
        }
    }
}