using System.Collections.Generic;
using Story.GraphToolkit.Runtime;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Story.GraphToolkit.Editor
{
    [ScriptedImporter(4, StoryGraph.AssetExtension)]
    public class StoryGraphImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            Graph editorGraph = GraphDatabase.LoadGraphForImporter<StoryGraph>(context.assetPath);
            
            StoryRuntimeGraph runtimeGraph = ScriptableObject.CreateInstance<StoryRuntimeGraph>();

            List<StoryRuntimeNode> runtimeNodes = StoryGraphRuntimeCreator.CreateRuntimeNodes(editorGraph);
            
            runtimeGraph.Initialize(runtimeNodes);
            
            context.AddObjectToAsset("StoryRuntimeGraph", runtimeGraph);
            context.SetMainObject(runtimeGraph);
        }
    }
}
