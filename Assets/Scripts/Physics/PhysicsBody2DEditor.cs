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
            if (!script.showDebugValues)
                return;

            script.currentVelocity = EditorGUILayout.Vector2Field("Current Velocity", script.currentVelocity);
            script.terminalVelocityXReached =
                EditorGUILayout.Toggle("Terminal Velocity X Reached", script.terminalVelocityXReached);
            script.terminalVelocityYReached =
                EditorGUILayout.Toggle("Terminal Velocity Y Reached", script.terminalVelocityYReached);
            script.addVelocityToBody = EditorGUILayout.Toggle("Add Velocity To Body", script.addVelocityToBody);
            script.velocityToAdd = EditorGUILayout.Vector2Field("Velocity To Add", script.velocityToAdd);
        }
    }
}