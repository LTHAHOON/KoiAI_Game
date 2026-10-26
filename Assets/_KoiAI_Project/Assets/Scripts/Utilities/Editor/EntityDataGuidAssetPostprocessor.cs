using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using KoiAI.Core;

public class EntityDataGuidAssetPostprocessor : AssetPostprocessor
{
    private static readonly HashSet<string> ImportedPaths = new();
    private static bool _validationQueued;

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (AssetDatabase.LoadAssetAtPath<EntityData>(path))
            {
                ImportedPaths.Add(path);
            }
        }

        if (ImportedPaths.Count == 0 || _validationQueued)
        {
            return;
        }

        _validationQueued = true;
        EditorApplication.delayCall += ResetDuplicateGuids;
    }

    private static void ResetDuplicateGuids()
    {
        _validationQueued = false;

        var importedEntities = new List<EntityData>();
        foreach (string path in ImportedPaths)
        {
            EntityData entityData = AssetDatabase.LoadAssetAtPath<EntityData>(path);
            if (entityData)
            {
                importedEntities.Add(entityData);
            }
        }
        ImportedPaths.Clear();

        string[] assetGuids = AssetDatabase.FindAssets("t:ScriptableObject");
        var allEntityData = new List<EntityData>();
        foreach (string assetGuid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGuid);
            EntityData entityData = AssetDatabase.LoadAssetAtPath<EntityData>(path);
            if (entityData)
            {
                allEntityData.Add(entityData);
            }
        }

        bool changed = false;
        foreach (EntityData importedEntity in importedEntities)
        {
            var entityGuid = importedEntity.GetEntityID();
            if (entityGuid == System.Guid.Empty)
            {
                continue;
            }

            foreach (EntityData existingEntity in allEntityData)
            {
                if (existingEntity == importedEntity || existingEntity.GetEntityID() != entityGuid)
                {
                    continue;
                }

                importedEntity.ResetGuid();
                EditorUtility.SetDirty(importedEntity);
                changed = true;
                break;
            }
        }

        if (changed)
        {
            AssetDatabase.SaveAssets();
        }
    }
}
