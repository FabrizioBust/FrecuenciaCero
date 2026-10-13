# Menú principal de Frecuencia Cero

Implementado para Unity 6000.6.3f1. Abre `Assets/Scenes/MainMenu.unity` y pulsa Play. La escena también es la primera escena habilitada del Build; se conservaron SampleScene (deshabilitada), exterior, interior y generador. El perfil Windows existente usa esa lista global.

## Presentación e interacción

El fondo limpio `Assets/Art/Menu/Menu_FondoLimpio.png` conserva la composición de `Menu.png`, sin textos ni botones dibujados. El título es una única imagen independiente con transparencia, `Menu_Titulo.png`. Ambos recursos se adaptaron a partir de la referencia; el original se conserva. Los cuatro botones son objetos Button de Unity con texto visible, borde, selección roja y triángulo. La jerarquía completa se puede editar en el Canvas de la escena.

Ratón: señalar y hacer clic. Teclado: flechas arriba/abajo o WASD para navegar; Enter para activar; izquierda/derecha para ajustar deslizadores. Escape vuelve al menú desde opciones o créditos. El EventSystem utiliza InputSystemUIInputModule y acciones UI independientes de las de Leo.

CanvasScaler usa una referencia 1920 × 1080. La composición conserva 16:9 con bandas negras cuando cambia la proporción, sin recortar los controles. Las texturas visuales utilizan filtro Point; la niebla se mueve muy lentamente y la luz de la torre pulsa. El título tiene desplazamientos de un píxel durante interferencias breves, sin mover la cámara. No se añadieron textos ni iconos en las esquinas inferiores del menú.

## Audio y opciones

El único AudioSource del menú reproduce el archivo existente `Assets/Audio/Music/MusicaMenu.mp3`, en 2D y bucle, con volumen predeterminado del 45 %, entrada gradual y salida gradual al iniciar partida. La escena y su AudioSource se descargan al entrar al nivel; no hay un objeto de música persistente.

Los deslizadores guardan valores de 0 a 1 y muestran porcentajes. Música cambia el volumen real del AudioSource del menú. Efectos se aplica al AudioSource existente de TreeFallEvent cuando se abre el interior, conservando su volumen relativo de los clips. Ese es el sistema de efectos sonoros actualmente presente en el proyecto; no se inventaron pistas ni se descargó música adicional.

La pantalla de opciones permite pantalla completa en modo FullScreenWindow o Windowed y resoluciones enumeradas por Unity, incluyendo el tamaño actual de ventana. Las preferencias se guardan con estas claves PlayerPrefs:

- `FC.Audio.Music` y `FC.Audio.Effects`.
- `FC.Display.Fullscreen`, `FC.Display.Width` y `FC.Display.Height`.

Al abandonar opciones, comenzar partida o destruir el menú, se guardan los ajustes de audio. Los cambios de pantalla se guardan al aplicarlos. Una resolución guardada que ya no esté disponible utiliza el tamaño actual. Nueva partida conserva estas claves y no borra otras preferencias del usuario.

## Nueva partida y estados narrativos

Tras el fundido, Nueva partida llama a los métodos de reinicio de sesión existentes de RetenMission, RetenTravel y HudVisibility y carga `N1_ExteriorReten` mediante LoadSceneAsync en modo Single. Los métodos conservan su reinicio automático al iniciar Play; únicamente se hizo pública su llamada.

Se restablecen los objetivos, estados del supervisor, accidente, reacción, descubrimiento, fusible y generador, además de sus temporizadores y la llegada pendiente entre escenas. La camioneta original comienza visible y el árbol/vehículo destruido permanece oculto. El accidente puede activarse otra vez al terminar la conversación del supervisor. La linterna y batería pertenecen a la escena recién cargada y conservan su inicialización existente.

No se modificaron escenas de gameplay, límites de cámara, seguimiento, movimiento, colisiones, HUD, diálogos ni animaciones durante esta implementación. Se conservaron los cambios de la etapa 6 que ya estaban pendientes.

## Créditos

La pantalla contiene exactamente los cuatro integrantes solicitados, sin cargos individuales, y Unity como motor.

La atribución de SCP-x5x (Outer Thoughts), Kevin MacLeod, corresponde a Creative Commons Attribution 4.0 International. Verificada el 11 de octubre de 2026 en la ficha oficial de Incompetech, ISRC USUAN2000011:

- Origen: https://incompetech.com/music/royalty-free/index.html?isrc=USUAN2000011
- Licencia: https://creativecommons.org/licenses/by/4.0/

El archivo musical se conserva sin cambios. Los créditos indican su reproducción en bucle y ajuste de volumen.

## Archivos de esta implementación

Nuevos, con sus archivos .meta:

- `Assets/Scenes/MainMenu.unity`.
- `Assets/Art/Menu/Menu_FondoLimpio.png` y `Menu_Titulo.png`.
- `Assets/Scripts/Menu/MainMenuController.cs`: acciones, pantallas y transiciones.
- `Assets/Scripts/Menu/MenuPreferences.cs`: preferencias de audio y pantalla.
- `Assets/Scripts/Menu/MenuButtonHighlight.cs`: respuesta visual a selección y ratón.
- `Assets/Scripts/Menu/MenuBorderGraphic.cs`: borde de botones.
- `Assets/Scripts/Menu/MenuAtmosphereGraphic.cs`: niebla y baliza de torre.
- `Assets/Scripts/Menu/MenuTitleInterference.cs`: interferencia del título.
- `Assets/Scripts/Menu/MenuDesignScale.cs`: escalado de composición.
- `Assets/Editor/MainMenuSceneBuilder.cs`: creación de la escena solo si no existe; protege una escena ya creada de sobrescrituras.
- `Docs/MenuPrincipal.md`.

Modificados:

- `ProjectSettings/EditorBuildSettings.asset`: MainMenu al principio, conservando el resto de entradas y configuración.
- `Assets/Scripts/Narrative/RetenMission.cs`: acceso público al reinicio existente.
- `Assets/Scripts/World/RetenTravel.cs`: acceso público al reinicio existente.
- `Assets/Scripts/UI/HudVisibility.cs`: acceso público al reinicio existente.
- `Assets/Scripts/World/TreeFallEvent.cs`: volumen de su AudioSource según la preferencia de efectos.

## Comprobaciones y prueba manual

Las pruebas se ejecutaron en una copia aislada bajo `Temp/CameraVerificationUnity`, usando Unity 6000.6.3f1 nativo. La copia usa nombres de compañía/producto diferentes para evitar tocar los PlayerPrefs reales. Los programas de prueba y capturas están en Temp y están excluidos de Git.

Comprobados: orden del Build y conservación de escenas; cámara y AudioListener únicos; acciones UI; raycasts de ratón y eventos de navegación/activación; transiciones; reproducción automática y bucle; volumen real; resolución y pantalla completa guardadas; créditos; ausencia de texto en esquinas; recarga sin duplicar música; preferencias persistentes al reiniciar Unity; Nueva partida desde una sesión con accidente completo; estado inicial de camioneta, árbol y objetivos; carga del exterior y transición al interior; volumen de efectos aplicado al AudioSource del accidente. También se renderizaron menú, opciones y créditos, incluyendo proporciones 16:9 y 4:3.

La aceptación final del menú pasó 78 comprobaciones. Incluye saltar al final de MusicaMenu y verificar que la reproducción vuelve al principio por el bucle, además de activar SALIR y comprobar que Play Mode se detiene. Las capturas están en `Temp/Menu/main.png`, `main-4x3.png`, `options.png` y `credits.png`.

Después de integrar el menú se repitió la regresión de la etapa 6: 62 comprobaciones de accidente, diálogos, objetivos, rutas físicas, cámara, batería, linterna y viajes exterior/interior/generador pasaron. Los hashes de las escenas de gameplay y de los recursos originales permanecen iguales a los del comienzo de esta tarea.

No hubo errores de compilación en los scripts finales ni excepciones de los sistemas del juego durante las pruebas finales. Al arrancar la copia, UnityEditor.Search mostró una ArgumentOutOfRangeException al crear su índice; la misma excepción está registrada en las pruebas de la etapa 6 anteriores a este menú. No se modificó el buscador ni su caché para ocultarla.

Para completar la comprobación en tu equipo:

1. Abre MainMenu y Play. Comprueba el fundido, niebla, baliza y legibilidad del título. Navega con ratón y teclado.
2. Escucha MusicaMenu, espera al final de la pista para comprobar el empalme y prueba volumen 0 %, 50 % y 100 %.
3. Ajusta efectos y, tras iniciar partida y terminar la radio dentro del retén, escucha el accidente. Comprueba silencio al 0 %.
4. En un ejecutable Windows, cambia resolución y pantalla completa; cierra y vuelve a abrir para verificar los ajustes. El editor no reproduce todos los comportamientos de pantalla de un ejecutable.
5. Inicia Nueva partida, comprueba camioneta intacta y objetivo inicial; entra/sal del retén y completa la radio para verificar de nuevo la secuencia del árbol. Una nueva sesión debe permitir repetirla.
6. Comprueba los cuatro nombres en créditos. SALIR detiene Play en el editor y llama a Application.Quit en el ejecutable Windows.

La escucha real, el empalme audible de la canción, las teclas físicas y el cierre/pantalla completa de un ejecutable Windows requieren esa prueba manual. No se realizó commit ni push.
