using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PlayerAnimatorControllerSetup
{
    private const string ControllerPath = "Assets/Sprite/Player.controller";

    [InitializeOnLoadMethod]
    private static void EnsurePlayerAnimatorParameters()
    {
        EditorApplication.delayCall += AddMissingParameters;
    }

    private static void AddMissingParameters()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            return;
        }

        bool changed = false;
        changed |= EnsureParameter(controller, "moveX", AnimatorControllerParameterType.Float);
        changed |= EnsureParameter(controller, "moveY", AnimatorControllerParameterType.Float);
        changed |= EnsureParameter(controller, "isMoving", AnimatorControllerParameterType.Bool);

        if (!changed)
        {
            return;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    private static bool EnsureParameter(
        AnimatorController controller,
        string parameterName,
        AnimatorControllerParameterType parameterType)
    {
        bool exists = controller.parameters.Any(parameter =>
            parameter.name == parameterName && parameter.type == parameterType);

        if (exists)
        {
            return false;
        }

        controller.AddParameter(parameterName, parameterType);
        return true;
    }
}
