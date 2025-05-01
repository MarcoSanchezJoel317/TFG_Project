using TMPro;
using UnityEngine;

public enum TextKey
{
    //Global Menu
    OPTIONS,

    //Main Menu
    MM_START,
    MM_EXIT,
    
    //Pause Menu
    PS_RESUME,
    PS_MAINEXIT,


    // Sección OPCIONES
    OP_SOUND,
    OP_SOUND_MASTER,
    OP_SOUND_MUSIC,
    OP_SOUND_MUTE,

    OP_GRAPHICS,

    OP_GAME,

    OP_RETURNMENU,
    OP_LANGUAGE

}

[System.Serializable]
public class TranslatableText
{
    public TextKey textKey; // Clave del texto (ej: MM_START)
    public TextMeshProUGUI textElement; // Referencia al texto en la UI
    
}