Menu System README

Este documento describe el sistema de menús implementado en el proyecto Unity de tu TFG.

Descripción general

El Menu System gestiona todas las pantallas de interfaz (menú principal, submenús, opciones, pausa, etc.) de forma modular:

Entrada unificada: permite navegación por ratón y gamepad con cambio automático de modo.

Transiciones: animaciones de fundido (fade) entre menús.

Localización: carga y muestra texto en distintos idiomas usando LanguageManager y TranslatableText.

Opciones: controla ajustes de audio y juego (volúmenes, idioma, etc.) a través de SettingsManager, OptionsAudioSettings y OptionsGameSettings.

Personalización: carga dinámicamente cursores y música según nivel usando Addressables.

Assets/
  _Project/
    _Scripts/
      _Menu/
        Cursor.cs
        MenuManager.cs
        LanguageData.cs
        LanguageManager.cs
        TextKey.cs
        TranslatableText.cs
        
        SettingsManager.cs
        OptionsAudioSettings.cs
        OptionsGameSettings.cs
        CustomizeCursor.cs
        CustomizeMusic.cs
    Editor/
        LanguageDataEditor.cs
        EnumKeyEditor.cs


Cómo integrar el sistema

Añade MenuManager a un GameObject vacío en la escena principal.

Configura el fadeOverlay: asigna un SpriteRenderer delante de la cámara.

Crea y configura LanguageData (Assets → Crear → Idiomas → LanguageData) con todas las claves TextKey definidas.

Añade un LanguageManager en la escena: arrastra todos los assets LanguageData al array languages.

Usa TranslatableText en cada TextMeshProUGUI para que actualice automáticamente según el idioma.

Configura SettingsManager con tu AudioMixer y asegúrate de marcarlo con DontDestroyOnLoad.

Coloca en el canvas los controladores de opciones:

OptionsAudioSettings: sliders de volumen.

OptionsGameSettings: dropdown de idioma.

Configura Addressables para las direcciones MusicLevel1 y MusicLevel2 (o las que necesites) y añade CustomizeMusic.

Personalización de cursor: añade CustomizeCursor a un GameObject con el cursor alternativo.

Uso en la escena

Al iniciarse, MenuManager gestiona el fade y la navegación entre _fatherMenu y _childrenMenu.

Cursor.cs reemplaza el cursor del sistema y envía eventos UI cuando el usuario hace clic con gamepad.

Todas las cadenas de texto se obtienen desde LanguageManager.GetText(key).

Los cambios en opciones se guardan automáticamente en PlayerPrefs y se aplican al AudioMixer.

La música de fondo se carga asíncronamente al nivel actual.