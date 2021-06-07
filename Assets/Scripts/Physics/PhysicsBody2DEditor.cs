using UnityEditor;

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
                script.mass = EditorGUILayout.FloatField("Mass", script.mass);
                script.gravityScale = EditorGUILayout.FloatField("Gravity Scale", script.gravityScale);
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Drag", EditorStyles.boldLabel);
                script.dragAxisFactor = EditorGUILayout.Vector2Field("Drag Axis Factor", script.dragAxisFactor);
                script.dragCoefficient = EditorGUILayout.FloatField("Drag Coefficient", script.dragCoefficient);
                script.materialDensity = EditorGUILayout.FloatField("Material Density", script.materialDensity);
                script.frontalArea = EditorGUILayout.FloatField("Frontal Area", script.frontalArea);
                script.useQuickDrag = EditorGUILayout.Toggle("Use Quick Drag", script.useQuickDrag);
                script.quickDrag = EditorGUILayout.Slider("Quick Drag", script.quickDrag, -1.0f, 2.0f);
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Bounciness", EditorStyles.boldLabel);
                script.bounciness = EditorGUILayout.FloatField("Bounciness (CoR)", script.bounciness);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debugging", EditorStyles.boldLabel);
            script.showDebugValues = EditorGUILayout.Toggle("Show Debug Values", script.showDebugValues);

            if (script.showDebugValues)
            {
                script.currentVelocity = EditorGUILayout.Vector2Field("Current Velocity", script.currentVelocity);
                script.terminalVelocityXReached =
                    EditorGUILayout.Toggle("Terminal Velocity X Reached", script.terminalVelocityXReached);
                script.terminalVelocityYReached =
                    EditorGUILayout.Toggle("Terminal Velocity Y Reached", script.terminalVelocityYReached);
                script.addVelocityToBody = EditorGUILayout.Toggle("Add Velocity To Body", script.addVelocityToBody);
                script.velocityToAdd = EditorGUILayout.Vector2Field("Velocity To Add", script.velocityToAdd);
                EditorGUILayout.Vector2Field("Base velocity", script.BaseVelocity);
            }
        }
    }
}