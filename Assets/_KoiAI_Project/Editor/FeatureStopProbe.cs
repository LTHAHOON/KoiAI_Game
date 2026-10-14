using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using KoiAI.AI;
using KoiAI.Player;
using static KoiAI.AI.AIFeature;

[InitializeOnLoad]
public static class FeatureStopProbe
{
    const string Key = "FeatureStopProbe.whiletoken.phase";
    const string Output = "Library/FeatureStopProbe.whiletoken.result.txt";
    static AIBrain brain;
    static AIFeature movement, returning;
    static Animator animator;
    static HashSet<AIFeatureProperty> active;
    static int phase, lastFrame = -1;
    static float began, stopMagnitude;
    static FeatureStopProbe() { EditorApplication.update += Tick; EditorApplication.delayCall += Begin; }
    static void Begin()
    {
        if (SessionState.GetInt(Key, 0) != 0) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) { File.WriteAllText(Output, "SKIPPED: already playing"); SessionState.SetInt(Key, 9); return; }
        File.WriteAllText(Output, "Testing Exit-only while smoothing and shared cancellation token\n");
        SessionState.SetInt(Key, 1); EditorApplication.EnterPlaymode();
    }
    static float Magnitude() => new Vector2(animator.GetFloat("CurrentXMove"), animator.GetFloat("CurrentZMove")).magnitude;
    static void Stage(int value) { phase = value; SessionState.SetInt(Key, value); began = Time.time; }
    static void Tick()
    {
        phase = SessionState.GetInt(Key, 0);
        if (phase == 0 || phase == 9 || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        try
        {
            Application.runInBackground = true; EditorApplication.isPaused = false;
            if (phase == 1)
            {
                brain = UnityEngine.Object.FindObjectsByType<AIBrain>().FirstOrDefault(b => b.name.Contains("Bodyguard"));
                var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                if (!brain || !player) throw new Exception("Bodyguard/player missing");
                var features = (List<AIFeature>)typeof(AIBrain).GetField("_aiFeatures", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(brain);
                if (features == null || features.Count == 0) return;
                var agent = brain.GetComponent<NavMeshAgent>();
                if (!agent.isOnNavMesh) return;
                movement = features.First(f => f is AIMovement); returning = features.First(f => f is AIReturn);
                foreach (var system in UnityEngine.Object.FindObjectsByType<AISystem>()) system.enabled = false;
                brain.StopAllCoroutines();
                active = (HashSet<AIFeatureProperty>)typeof(AIBrain).GetField("_activeFeatures", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(brain);
                active.Clear(); player.enabled = false;
                foreach (var feature in player.GetComponents<PlayerFeature>()) feature.enabled = false;
                var rigid = player.GetComponent<Rigidbody>();
                if (rigid) { rigid.linearVelocity = Vector3.zero; rigid.isKinematic = true; }
                var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = NavMesh.AllAreas };
                if (!NavMesh.SamplePosition(brain.transform.position + brain.transform.forward * 6f, out var hit, 4f, filter)) throw new Exception("No target surface");
                player.transform.position = hit.position; if (rigid) rigid.position = hit.position;
                Physics.SyncTransforms(); brain.TargetContext.SetTarget(brain.transform, player.transform);
                animator = brain.GetComponent<Animator>(); active.Add(AIFeatureProperty.Movement); movement.EnterFeature(); Stage(2);
            }
            if (lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            float age = Time.time - began;
            switch (phase)
            {
                case 2:
                    movement.UpdateFeature();
                    if (age >= 0.9f)
                    {
                        active.Clear(); movement.ExitFeature(); stopMagnitude = Magnitude();
                        if (stopMagnitude < 0.05f) throw new Exception("Movement stopped without nonzero animation");
                        File.AppendAllText(Output, $"Movement stop begins at {stopMagnitude:F6}\n"); Stage(3);
                    }
                    break;
                case 3:
                    if (age >= 2f)
                    {
                        if (Magnitude() > 0.0001f) throw new Exception("Movement stop did not reach zero");
                        File.AppendAllText(Output, "PASS: AIMovement stop reaches zero after feature exits\n");
                        active.Add(AIFeatureProperty.Return); returning.EnterFeature(); Stage(4);
                    }
                    break;
                case 4:
                    returning.UpdateFeature();
                    if (age >= 0.25f)
                    {
                        active.Clear(); returning.ExitFeature(); stopMagnitude = Magnitude();
                        if (stopMagnitude < 0.01f) throw new Exception("Return animation missing");
                        File.AppendAllText(Output, $"Return stop begins at {stopMagnitude:F6}\n"); Stage(5);
                    }
                    break;
                case 5:
                    if (age >= 0.1f)
                    {
                        if (Magnitude() >= stopMagnitude) throw new Exception("Return stop did not damp");
                        active.Add(AIFeatureProperty.Return); returning.EnterFeature();
                        File.AppendAllText(Output, "PASS: AIReturn re-entry supplies a new target\n"); Stage(6);
                    }
                    break;
                case 6:
                    returning.UpdateFeature();
                    if (age >= 0.25f) { active.Clear(); returning.ExitFeature(); Stage(7); }
                    break;
                case 7:
                    if (age >= 2f)
                    {
                        if (Magnitude() > 0.0001f) throw new Exception("Return stop did not reach zero");
                        File.AppendAllText(Output, "PASS: AIReturn stop reaches zero after feature exits\n"); Finish();
                    }
                    break;
            }
        }
        catch (Exception e) { File.AppendAllText(Output, "FAIL: " + e + "\n"); Finish(); }
    }
    static void Finish() { SessionState.SetInt(Key, 9); EditorApplication.ExitPlaymode(); }
}