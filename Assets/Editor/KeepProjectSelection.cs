using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class KeepProjectSelection
{
    static Object[] previousSelection;

    static KeepProjectSelection()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Store current project selection before entering play
            previousSelection = Selection.objects;
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Restore selection after exiting play
            if (previousSelection != null && previousSelection.Length > 0)
            {
                Selection.objects = previousSelection;
            }
        }
    }
}