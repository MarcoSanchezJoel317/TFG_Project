using System.Runtime.CompilerServices;
using UnityEngine;

public static class CustomLogger
{
#if UNITY_EDITOR
    public static bool EnableLogs = true;
#else
    public static bool EnableLogs = false;
#endif

    public static void Log(
        MonoBehaviour context,
        string message,
#if UNITY_EDITOR
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0
#endif
    )
    {
#if UNITY_EDITOR
        if (!EnableLogs) return;

        string scriptName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        string objectName = context.gameObject.name;

        // Formato en una sola línea con separadores
        string formattedMessage =
    $"<color=green>▶ {scriptName}</color> | " + // ▶ es un triángulo Unicode
    $"<color=blue>⚓ {objectName}</color> | " + // ⚓ anclaje
    $"📏 Line: <color=magenta>{lineNumber}</color> | " +
    $"{message}";
        Debug.Log(formattedMessage, context.gameObject);
#endif
    }
}
