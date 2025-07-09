using TMPro;
using UnityEngine;


/// <summary>
/// Claves usadas para obtener los textos traducidos desde LanguageManager.
/// </summary>
/// 
public enum TextKey
{
    //Global Menu
    OPTIONS,

    //Main Menu
    MM_START,
    MM_EXIT,

    //Play Menu
    MM_PL_NEWGAME,
    MM_PL_LOADGAME,
    MM_PL_RETURN,
    
    //Pause Menu
    PS_RESUME,
    PS_MAINEXIT,
    PS_DESKTOP,


    // OPTIONS SECTION
    OP_SOUND,
    OP_GRAPHICS,
    OP_GAME,
    OP_RETURNMENU,


    //SOUND SECTION

    OP_SOUND_MASTER,
    OP_SOUND_MUSIC,
    OP_SOUND_EFFECTS,



    //GRAPHICS SECTION

    OP_GRAPHICS_DEV,

    // CONTROLS SECTION
    OP_CONTROLS_DEV,

    // GAME SECTION
    OP_GAME_LANGUAGE,
    DEVICE,
}