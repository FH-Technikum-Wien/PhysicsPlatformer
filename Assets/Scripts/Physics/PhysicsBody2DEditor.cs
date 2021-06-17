using UnityEditor;
using UnityEngine;

namespace Physics
{
    [CustomEditor(typeof(PhysicsBody2D), true), CanEditMultipleObjects]
    public class PhysicsBody2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PhysicsBody2D script = (PhysicsBody2D) target;

            if (script.BodyType == PhysicsBody2D.Type.Dynamic)
            {
                script.useCustomGravity = EditorGUILayout.Toggle(new GUIContent("Use Custom Gravity",
                    "Whether to use the global gravity or a custom one"), script.useCustomGravity);

                if (script.useCustomGravity)
                {
                    script.customGravity = EditorGUILayout.Vector2Field(new GUIContent("Custom Gravity",
                        "Custom gravity applied to the body"), script.customGravity);
                }
                else
                {
                    script.gravityScale = EditorGUILayout.FloatField(new GUIContent("Gravity Scale",
                        "Factor applied to the global gravity"), script.gravityScale);
                }

                script.mass = EditorGUILayout.FloatField(new GUIContent("Mass",
                    "The mass of this physics body"), script.mass);

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Drag", EditorStyles.boldLabel);

                script.dragAxisFactor = EditorGUILayout.Vector2Field(new GUIContent("Drag Axis Factor",
                    "A factor applied to the drag on each axis. " +
                    "Can be used to customise the drag for each axis (not realistic)"), script.dragAxisFactor);

                script.dragCoefficient = EditorGUILayout.FloatField(new GUIContent("Drag Coefficient",
                    "The drag coefficient of the object/shape"), script.dragCoefficient);

                script.elementDensity = EditorGUILayout.FloatField(new GUIContent("Element Density",
                    "The density of the element the physics body is currently in"), script.elementDensity);

                script.frontalArea = EditorGUILayout.FloatField(new GUIContent("Frontal Area",
                    "The area affected by the material density creating the drag"), script.frontalArea);

                script.useQuickDrag = EditorGUILayout.Toggle(new GUIContent("Use Quick Drag",
                    "Use quickDrag instead of correct drag [0-1]"), script.useQuickDrag);
                script.quickDrag = EditorGUILayout.Slider(new GUIContent("Quick Drag",
                        "QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates"), script.quickDrag,
                    -1.0f, 2.0f);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(new GUIContent("Bounciness",
                        "The bounciness of the physics body. < 0 -> 'Phasing' | 0-1 -> Bounce | > 1 -> Explosion"),
                    EditorStyles.boldLabel);

                script.bounciness = EditorGUILayout.FloatField("Bounciness (CoR)", script.bounciness);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debugging", EditorStyles.boldLabel);
            script.showDebugValues = EditorGUILayout.Toggle(new GUIContent("Show Debug Values",
                "Display debug values and buttons"), script.showDebugValues);

            if (script.showDebugValues)
            {
                script.currentVelocity = EditorGUILayout.Vector2Field(new GUIContent("Current Velocity",
                    "The current velocity"), script.currentVelocity);

                script.terminalVelocityXReached = EditorGUILayout.Toggle(new GUIContent("Terminal Velocity X Reached",
                    "Whether we reached terminal velocity on Y"), script.terminalVelocityXReached);

                script.terminalVelocityYReached = EditorGUILayout.Toggle(new GUIContent("Terminal Velocity Y Reached",
                    "Whether we reached terminal velocity on Y"), script.terminalVelocityYReached);

                script.addVelocityToBody = EditorGUILayout.Toggle("Add Velocity To Body", script.addVelocityToBody);
                script.velocityToAdd = EditorGUILayout.Vector2Field("Velocity To Add", script.velocityToAdd);
                EditorGUILayout.Vector2Field("Base velocity", script.BaseVelocity);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }
    }
}