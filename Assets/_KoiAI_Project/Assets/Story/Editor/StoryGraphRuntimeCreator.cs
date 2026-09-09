using System.Collections.Generic;
using System.Linq;
using Story.GraphToolkit.Runtime;
using Unity.GraphToolkit.Editor;

namespace Story.GraphToolkit.Editor
{
    public interface IRuntimeNodeCreatable
    {
        public StoryRuntimeNode CreateRuntimeInstance();
    }

    public interface IRuntimeBlockNodeCreatable
    {
        public StoryRuntimeBlockNode CreateRuntimeBlockInstance();
    }

    public static class StoryGraphRuntimeCreator
    {
        public static List<StoryRuntimeBlockNode> CreateRuntimeBlockNodes(List<StoryBlockNode> storyBlockNodes)
        {
            List<StoryRuntimeBlockNode> storyRuntimeBlockNodes = storyBlockNodes.OfType<IRuntimeBlockNodeCreatable>()
                .Select(node => node.CreateRuntimeBlockInstance())
                .Where(node => node != null)
                .ToList();

            return storyRuntimeBlockNodes;
        }

        public static List<StoryRuntimeNode> CreateRuntimeNodes(Graph storyGraph)
        {
            List<INode> editorNodes = new();
            List<StoryRuntimeNode> runtimeNodes = new();
            Dictionary<INode, int> dicRuntimeNodeIndices = new();

            if (storyGraph == null)
            {
                return runtimeNodes;
            }

            List<INode> allNodes = GetConnectedNodesFromStart(storyGraph.GetNodes());
            (int Index, ISubgraphNode SubGraphNode)[] subgraphNodes = allNodes
                                        .OfType<ISubgraphNode>()
                                        .Select(node => (allNodes.IndexOf(node), node))
                                        .ToArray();

            if (subgraphNodes.Length > 0)
            {
                List<(int Index, List<INode> Nodes)> subgraphNodeIndexAndNodes = new();

                for (int i = 0; i < subgraphNodes.Length; i++)
                {
                    Graph subgraph = subgraphNodes[i].SubGraphNode.GetSubgraph();
                    if (subgraph == null)
                    {
                        continue;
                    }

                    List<INode> connectedSubgraphNodes = GetConnectedSubgraphNodes(subgraph)
                            .Where(node => node is IRuntimeNodeCreatable)
                            .ToList();

                    if (connectedSubgraphNodes.Count == 0)
                    {
                        continue;
                    }

                    subgraphNodeIndexAndNodes.Add(
                        (subgraphNodes[i].Index, connectedSubgraphNodes));
                }

                int insertedNodeCount = 0;

                for (int i = 0; i < subgraphNodeIndexAndNodes.Count; i++)
                {
                    int insertionIndex = subgraphNodeIndexAndNodes[i].Index + 1 + insertedNodeCount;

                    List<INode> subgraphNodeList = subgraphNodeIndexAndNodes[i].Nodes;

                    allNodes.InsertRange(insertionIndex, subgraphNodeList);
                    insertedNodeCount += subgraphNodeList.Count;
                }
            }

            List<IRuntimeNodeCreatable> runtimeNodeCreators = allNodes
                .OfType<IRuntimeNodeCreatable>()
                .Distinct()
                .ToList();

            for (int i = 0; i < runtimeNodeCreators.Count; i++)
            {
                IRuntimeNodeCreatable creator = runtimeNodeCreators[i];
                if (creator is not INode editorNode)
                {
                    continue;
                }

                StoryRuntimeNode runtimeNode = creator.CreateRuntimeInstance();
                if (runtimeNode == null)
                {
                    continue;
                }

                int runtimeNodeIndex = runtimeNodes.Count;
                editorNodes.Add(editorNode);
                runtimeNodes.Add(runtimeNode);
                dicRuntimeNodeIndices.Add(editorNode, runtimeNodeIndex);
            }

            for (int i = 0; i < runtimeNodes.Count; i++)
            {
                if (i == runtimeNodes.Count - 1)
                {
                    runtimeNodes[i].NextNodeIndex = -1;
                    break;
                }
                runtimeNodes[i].NextNodeIndex = runtimeNodes.IndexOf(runtimeNodes[i + 1]);
            }

            return runtimeNodes;
        }

        private static List<INode> GetConnectedNodesFromStart(IEnumerable<INode> nodes)
        {
            List<INode> nodeList = nodes.ToList();
            HashSet<INode> validNodes = new(nodeList);
            HashSet<INode> visitedNodes = new();
            List<INode> connectedNodes = new();

            INode currentNode = nodeList
                .OfType<StoryStart_Node>()
                .FirstOrDefault();

            while (currentNode != null && validNodes.Contains(currentNode) && visitedNodes.Add(currentNode))
            {
                connectedNodes.Add(currentNode);

                IPort exitPort = GetStoryFlowOutputPort(currentNode);

                if (exitPort == null || !exitPort.IsConnected)
                {
                    break;
                }

                currentNode = exitPort.FirstConnectedPort?.GetNode();
            }

            return connectedNodes;
        }

        private static List<INode> GetConnectedSubgraphNodes(Graph subgraph)
        {
            List<INode> nodeList = subgraph.GetNodes().ToList();
            HashSet<INode> validNodes = new(nodeList);
            HashSet<INode> visitedNodes = new();
            List<INode> connectedNodes = new();

            IVariableNode enterNode = nodeList
                .OfType<IVariableNode>()
                .FirstOrDefault(node =>
                    node.Variable != null &&
                    node.Variable.VariableKind == VariableKind.Input &&
                    node.Variable.DataType == typeof(StoryFlow));

            if (enterNode == null || enterNode.OutputPortCount == 0)
            {
                return connectedNodes;
            }

            INode currentNode = enterNode.GetOutputPort(0)?.FirstConnectedPort?.GetNode();

            while (currentNode != null &&
                   validNodes.Contains(currentNode) &&
                   visitedNodes.Add(currentNode))
            {
                if (currentNode is IVariableNode exitNode &&
                    exitNode.Variable != null &&
                    exitNode.Variable.VariableKind == VariableKind.Output &&
                    exitNode.Variable.DataType == typeof(StoryFlow))
                {
                    break;
                }

                connectedNodes.Add(currentNode);

                IPort exitPort = currentNode.GetOutputPortByName(StoryFlow.EXIT);
                //연결되어 있지 않을 경우
                if (exitPort == null || !exitPort.IsConnected)
                {
                    break;
                }

                currentNode = exitPort.FirstConnectedPort?.GetNode();
            }

            return connectedNodes;
        }

        private static IPort GetStoryFlowOutputPort(INode node)
        {
            if (node is not ISubgraphNode subgraphNode)
            {
                return node.GetOutputPortByName(StoryFlow.EXIT);
            }

            Graph subgraph = subgraphNode.GetSubgraph();
            if (subgraph == null)
            {
                return null;
            }

            IVariable outputVariable = subgraph.GetVariables()
                .FirstOrDefault(variable =>
                    variable.VariableKind == VariableKind.Output &&
                    variable.DataType == typeof(StoryFlow));
                    
            IPort port = Enumerable.Range(0, subgraphNode.OutputPortCount)
                .Select(subgraphNode.GetOutputPort)
                .FirstOrDefault(outputPort => outputPort.DisplayName == outputVariable?.Name);
            for (int i = 0; i < subgraphNode.OutputPortCount; i++)
            {
                IPort outputPort = subgraphNode.GetOutputPort(i);
                UnityEngine.Debug.Log(outputPort);
            }
            return outputVariable == null
                ? null
                : port;
        }

        //보류
        private static int FindNextNodeIndex(INode currentNode, Dictionary<INode, int> dicRuntimeNodeIndices)
        {
            IPort exitPort = GetStoryFlowOutputPort(currentNode);
            if (exitPort == null || !exitPort.IsConnected)
            {
                return -1;
            }

            IPort nextPort = exitPort.FirstConnectedPort;
            INode nextNode = nextPort.GetNode();
            if (nextNode is ISubgraphNode subgraphNode)
            {
                var subgraph = subgraphNode.GetSubgraph();
                nextNode = subgraph == null
                    ? null
                    : GetConnectedSubgraphNodes(subgraph)
                        .FirstOrDefault(node => node is IRuntimeNodeCreatable);
            }
            int nextIndex = dicRuntimeNodeIndices.GetValueOrDefault(nextNode, -1);

            return nextIndex;
        }
    }
}
