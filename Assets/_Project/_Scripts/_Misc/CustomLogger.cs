using System.Runtime.CompilerServices;
using UnityEngine;

public static class CustomLogger
{
#if UNITY_EDITOR
    public static bool EnableLogs = true;
#else
    public static bool EnableLogs = false;
#endif

    // Siempre disponible en compilación, pero con cuerpo vacío fuera del editor
    public static void Log(
        MonoBehaviour context,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0
    )
    {
#if UNITY_EDITOR
        if (!EnableLogs) return;

        string scriptName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        string objectName = context.gameObject.name;

        string formattedMessage =
            $"<color=green>▶ {scriptName}</color> | " +
            $"<color=blue>⚓ {objectName}</color> | " +
            $"📏 Line: <color=magenta>{lineNumber}</color> | " +
            $"{message}";

        Debug.Log(formattedMessage, context.gameObject);
#endif
    }
}


