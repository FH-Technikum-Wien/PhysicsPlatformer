using UnityEditor;
using UnityEngine;

namespace Physics
{
    [CustomEditor(typeof(PhysicsBody2D)), CanEditMultipleObjects]
    public class PhysicsBody2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PhysicsBody2D script = (PhysicsBody2D) target;

            if (script.BodyType == PhysicsBody2D.Type.Dynamic)
            {
                script.Mass = EditorGUILayout.FloatField(new GUIContent("Mass", "The mass of this physics body"),
                    script.Mass);
                script.GravityScale = EditorGUILayout.FloatField(
                    new GUIContent("Gravity Scale", "Factor applied to the global gravity"), script.GravityScale);
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Drag", EditorStyles.boldLabel);
                script.DragAxisFactor = EditorGUILayout.Vector2Field(
                    new GUIContent("Drag Axis Factor",
                        "A factor applied to the drag on each axis. Can be used to customise the drag for each axis (not realistic)"),
                    script.DragAxisFactor);
                script.DragCoefficient = EditorGUILayout.FloatField(
                    new GUIContent("Drag Coefficient", "The drag coefficient of the object/shape"),
                    script.DragCoefficient);
                script.ElementDensity = EditorGUILayout.FloatField(
                    new GUIContent("Element Density", "The density of the element the physics body is currently in"),
                    script.ElementDensity);
                script.FrontalArea = EditorGUILayout.FloatField(
                    new GUIContent("Frontal Area", "The area affected by the material density creating the drag"),
                    script.FrontalArea);
                script.UseQuickDrag =
                    EditorGUILayout.Toggle(
                        new GUIContent("Use Quick Drag", "Use quickDrag instead of correct drag [0-1]"),
                        script.UseQuickDrag);
                script.QuickDrag =
                    EditorGUILayout.Slider(
                        new GUIContent("Quick Drag",
                            "QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates"), script.QuickDrag,
                        -1.0f, 2.0f);
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(
                    new GUIContent("Bounciness",
                        "The bounciness of the physics body. < 0 -> 'Phasing' | 0-1 -> Bounce | > 1 -> Explosion"),
                    EditorStyles.boldLabel);
                script.Bounciness = EditorGUILayout.FloatField("Bounciness (CoR)", script.Bounciness);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debugging", EditorStyles.boldLabel);
            script.ShowDebugValues =
                EditorGUILayout.Toggle(new GUIContent("Show Debug Values", "Display debug values and buttons"),
                    script.ShowDebugValues);

            if (script.ShowDebugValues)
            {
                script.CurrentVelocity =
                    EditorGUILayout.Vector2Field(new GUIContent("Current Velocity", "The current velocity"),
                        script.CurrentVelocity);
                script.TerminalVelocityXReached =
                    EditorGUILayout.Toggle(
                        new GUIContent("Terminal Velocity X Reached", "Whether we reached terminal velocity on Y"),
                        script.TerminalVelocityXReached);
                script.TerminalVelocityYReached =
                    EditorGUILayout.Toggle(
                        new GUIContent("Terminal Velocity Y Reached", "Whether we reached terminal velocity on Y"),
                        script.TerminalVelocityYReached);
                script.AddVelocityToBody = EditorGUILayout.Toggle("Add Velocity To Body", script.AddVelocityToBody);
                script.VelocityToAdd = EditorGUILayout.Vector2Field("Velocity To Add", script.VelocityToAdd);
                EditorGUILayout.Vector2Field("Base velocity", script.BaseVelocity);
            }
        }
    }
}