# Frecuencia Cero — Nivel01_Bosque

Primera etapa: movimiento, cámara y colisiones. Proyecto inspeccionado: Unity 6000.6.3f1, Universal 2D, URP 17.6.0, Input System 1.20.0; Active Input Handling ya era Input System. Se reutiliza `Assets/Settings/InputSystem_Actions.inputactions`, acción `Player/Move`, con WASD y los demás bindings existentes. No se han cambiado Packages ni ProjectSettings.

## Archivos

Modificado:
- `Assets/Scenes/Nivel01_Bosque.unity`: conserva la cámara y la luz originales; incorpora el prototipo, configura la cámara y conecta los scripts.

Creados, con sus `.meta` en Assets:
- `Assets/Scripts/Player/LeoMovement.cs`: lectura en Update; velocidad en FixedUpdate; límite de magnitud 1 para diagonales. Cada Leo posee una copia independiente de la acción de movimiento.
- `Assets/Scripts/Camera/SmoothCameraFollow.cs`: seguimiento con SmoothDamp en LateUpdate, manteniendo Z = -10.
- `Assets/Scripts/Rendering/YSort.cs`: orden de SpriteRenderer según la Y de los pies o base del tronco.
- `Assets/Editor/BosquePrototypeBuilder.cs`: menú opcional de preparación mediante APIs del Editor. No duplica un prototipo existente ni lo reconstruye encima de modificaciones.
- `Assets/Art/Placeholders/{Leo,Suelo,Sendero,Arbol,Roca}.png`: formas provisionales reemplazables, sin animaciones.
- `Assets/Art/Placeholders/ProvisionalUnlit.mat`: material URP 2D sin iluminación para esta etapa.
- `Assets/Art/Placeholders/SinFriccion.physicsMaterial2D`: fricción y rebote cero para Leo.
- `Docs/Nivel01_Bosque.original.unity.txt`: copia de la escena previa a esta intervención.
- Este documento.

Las carpetas nuevas en Assets también tienen `.meta`. En `Temp/FrecuenciaCeroValidation` quedan archivos temporales de preparación y comprobación; no forman parte del juego.

## Configuración incorporada

- Leo empieza en (0, -8), con velocidad 3 unidades/segundo, Rigidbody2D dinámico, gravedad 0, rotación congelada, interpolación y colisión continua.
- CapsuleCollider2D horizontal en los pies. Árboles y rocas usan BoxCollider2D estáticos, sin triggers. Las copas no bloquean toda su superficie.
- Terreno de 33 × 25 unidades, un sendero, árboles, cuatro rocas y cuatro límites físicos invisibles. Las baldosas del sendero y el suelo comparten escala; todos los Transform tienen escala 1.
- Cámara ortográfica, sin inclinación 3D. La perspectiva elevada se sugiere en los sprites, mostrando cuerpo y cabeza de Leo.
- Pixel Perfect Camera de URP: Assets PPU 32, referencia 640 × 360, Grid Snapping = Upscale Render Texture, Filter Mode = Point. HDR y MSAA desactivados únicamente en esta cámara.
- Sprites: Point, 32 PPU, sin mipmaps, sin compresión; pivotes en la base. El suelo queda detrás de las entidades y la ordenación Y permite pasar detrás de árboles.
- Los paquetes Sprite y Tilemap ya están instalados. Esta primera escena no necesita Tilemaps; pueden sustituir el terreno provisional más adelante manteniendo 32 PPU.
- La luz global original se conserva. El material provisional no necesita iluminación; no se implementaron oscuridad dinámica, sombras ni linterna.

## Abrir y probar

1. Vuelve a Unity 6000.6.3f1 y espera que termine la importación/compilación. Si no detecta los archivos, usa **Assets > Refresh**.
2. Si tenías `Nivel01_Bosque` abierto antes de estos cambios, abre primero `SampleScene` y después vuelve a abrir `Nivel01_Bosque`. Evita guardar encima de la escena modificada en disco una versión antigua que siga cargada en memoria. Si Unity pregunta sobre cambios propios sin guardar, consérvalos en otra escena antes de recargar.
3. En **Project > Assets > Scenes**, haz doble clic en **Nivel01_Bosque**. En Hierarchy deben aparecer **Main Camera**, **Global Light 2D** y **Prototipo_ElSenderoDelReten**, con Leo y las carpetas del escenario.
4. Abre **Window > General > Console** y comprueba que no haya errores rojos ni Missing Script.
5. En **Game**, selecciona **640 × 360**, **1280 × 720** o **1920 × 1080**. Si una resolución no figura, pulsa **+** en el selector de resoluciones y añádela como **Fixed Resolution**. Estos tamaños son múltiplos enteros de la resolución de referencia y facilitan evaluar los píxeles.
6. Pulsa **Play** y haz clic dentro de Game para dar foco a los controles.
7. Usa W/S/A/D y combina W+D, W+A, S+D y S+A. El cuerpo debe moverse por XY; la cámara debe seguirlo manteniendo orientación y distancia.
8. Desde el inicio, acércate a la roca situada en (1.4, -4); intenta caminar contra ella y deslizarte por sus bordes. Haz lo mismo con los troncos y con los límites del terreno. El personaje no debería atravesarlos.
9. Rodea un árbol por arriba y por abajo: Leo debe quedar detrás o delante de su sprite según la posición de sus pies. El collider solo representa la base.
10. Para comparar velocidades con precisión, selecciona **Leo** durante Play. En Rigidbody2D compara la magnitud de Linear Velocity en una zona libre de obstáculos: derecha ≈ (3, 0); diagonal ≈ (2.121, 2.121); magnitud ≈ 3 en ambos casos. La colisión modifica la velocidad efectiva al tocar un obstáculo.
11. Detén Play. Los cambios hechos durante Play no se guardan. No es necesario tocar Active Input Handling ni añadir componentes manualmente.

El menú **Frecuencia Cero > Preparar Nivel01 Bosque** es una herramienta opcional; la escena ya está escrita en disco. Si detecta el root del prototipo, no realiza cambios. No hace falta usarlo para la prueba normal.

Para sustituir el arte, selecciona cada objeto y cambia **Sprite Renderer > Sprite**. Importa las nuevas imágenes con Sprite (2D and UI), Point, 32 PPU y pivote coherente con los pies/base. Ajusta únicamente el collider cuando cambie el tamaño físico del objeto. La forma de Leo es una silueta provisional; las direcciones no tienen animaciones todavía.

## Verificación y límites

Se comprobó la compilación C# con Roslyn incluido en esta instalación de Unity, contra las bibliotecas Unity 6 y los paquetes ya instalados. La comprobación final terminó sin errores ni advertencias. Se revisaron las referencias locales de la escena y sus GUIDs, los importadores de sprites y la configuración serializada de componentes.

No se ejecutó Play ni una prueba visual o física dentro de Unity. El Editor estaba abierto y no había una sesión conectada que permitiera controlarlo. La compilación independiente no demuestra que la escena importe ni que movimiento, contactos y cámara funcionen durante la ejecución: esos puntos requieren la prueba anterior en el Editor.

El seguimiento es suave a nivel de Transform; Pixel Perfect cuantiza la imagen a píxeles. A resoluciones que no sean múltiplos enteros de 640 × 360 pueden apreciarse píxeles de grosor desigual con Point. Usa los tamaños indicados para la primera comprobación.

Build Profiles sigue incluyendo la escena original SampleScene. Para Play basta abrir Nivel01_Bosque. Si posteriormente quieres generar un ejecutable, abre **File > Build Profiles > Scene List** y añade Nivel01_Bosque como escena inicial; no se cambió el listado de compilación en esta etapa.

No se desarrollaron enemigos, inventario, puzles, otros niveles, animaciones ni mecánicas de linterna. La siguiente etapa queda pendiente de tu confirmación del prototipo.
