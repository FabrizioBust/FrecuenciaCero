# Introducción cinematográfica — Frecuencia Cero

Flujo: `MainMenu → IntroHistoria → N1_ExteriorReten`. Unity 6000.6.3f1, URP 2D.

## Secuencia

- 0–12 s: fundido desde negro, carretera nocturna, camioneta de mantenimiento, árboles en distintas capas, niebla ligera y faros. Aparecen progresivamente «CIMA DEL ÁGUILA» y «02:30 A. M.».
- 12–23 s: SUPERVISOR: «Leo, llevamos horas sin recibir señal de la estación. Necesito que revises el retén».
- 23–28 s: LEO: «Entendido. Estoy llegando».
- 28–36 s: desaceleración continua; aparece una cerca deteriorada con una abertura y un sendero hacia el bosque. La camioneta sigue horizontalmente y se detiene, sin entrar al sendero.
- 36–40 s: se apagan los faros, fundido a negro y carga del exterior. La transición final tarda aproximadamente 0,8 s adicionales.

No hay personajes visibles fuera del vehículo, fenómenos paranormales, interferencias de radio, sustos, objetivos ni HUD de gameplay.

La camioneta utiliza el mismo sprite, material, orientación y color del objeto `Camioneta_Mantenimiento` del exterior. Se escala exclusivamente su representación cinematográfica. Árboles, cerca, arbustos, rocas y niebla reutilizan recursos locales. Cielo, montañas, carretera, sendero y torre son geometría 2D local, sin fondos estáticos externos ni modelos 3D. Cámara pixel perfect: 32 PPU, referencia 640 × 360; composición de interfaz 16:9 escalada sin deformación.

Los dos faros delanteros usan luces `Light2D` blancas ligeramente cálidas y dos haces transparentes con bordes suaves y atenuación longitudinal. Ambos pertenecen a la camioneta y comparten su movimiento y apagado originales. El shader exclusivo de la introducción ilumina visualmente la carretera sin sustituir materiales del gameplay.

La abertura de la cerca coincide con un claro entre los árboles. El sendero pierde contraste hacia el bosque, con piedras pequeñas y niebla existente. Una montaña y una torre de transmisión con luz roja discreta tienen parallax distante. La casa, el cartel, el poste y su lámpara se quitaron únicamente de la cinemática; el retén jugable sigue intacto.

## Omitir

Mantener Espacio durante un segundo llena la pequeña barra bajo «Mantén ESPACIO para omitir». Soltar antes cancela el progreso. Si Espacio estaba pulsado al entrar, debe soltarse antes de iniciar la omisión, para evitar que la tecla usada en el menú salte la introducción. Un bloqueo único impide iniciar cargas repetidas. Omitir también desvanece imagen y audio antes de cargar el exterior.

## Estado de partida

`MainMenuController.NewGame` conserva sus tres llamadas existentes de reinicio: `RetenMission.ResetSession`, `RetenTravel.Reset` y `HudVisibility.ResetSession`. Solo cambia la escena destino a `IntroHistoria`. La introducción conserva esa misma instancia de partida y carga el exterior con `LoadSceneMode.Single`; no reinicia de nuevo ni completa la conversación del supervisor del gameplay.

El objetivo del sistema existente sigue siendo «Investigar el retén». La camioneta empieza intacta y la representación del árbol caído permanece oculta. El accidente sigue condicionado a la conversación original dentro del retén. No se modifican escenas jugables, posiciones iniciales, límites de cámara ni sistemas de movimiento, linterna, HUD, diálogos, generador o transiciones.

## Grabaciones pendientes

En `Assets/Audio` solo existe `MusicaMenu.mp3`. No se reutiliza como ambientación de la introducción y no se descargaron efectos ni se generaron voces.

En el Inspector de **Introducción — secuencia de 40 segundos**, asignar cuando estén disponibles grabaciones propias o con licencia verificada a:

- `Engine Loop`: motor de camioneta circulando.
- `Road Loop`: ruedas sobre carretera, moderadas.
- `Wind Loop`: viento nocturno discreto.
- `Forest Loop`: ambiente natural del bosque.
- `Supervisor Voice` y `Leo Voice`: exactamente las líneas indicadas, con comunicación clara y normal.
- `Engine Stop`: apagado del motor.

Los cuatro ambientes usan fuentes en bucle separadas; voces y apagado usan una fuente sin bucle. Todos pertenecen únicamente a esta escena y respetan el volumen de efectos del menú. Entrada, frenado y salida ajustan los niveles; al llegar se detienen motor y ruedas. Las fuentes se descargan al entrar al gameplay. Sin clips asignados, la introducción es silenciosa y presenta subtítulos completos; no hay audio provisional extraño ni voces automáticas.

## Archivos

Creados, además de sus `.meta`:

- `Assets/Scenes/IntroHistoria.unity`.
- `Assets/Scripts/Intro/IntroHistoriaController.cs`: secuencia, subtítulos, audio opcional, omisión y salida.
- `Assets/Scripts/Intro/IntroParallaxLayer.cs`: desplazamiento continuo de capas reutilizables.
- `Assets/Art/Intro/IntroPaisaje.asset`: geometría 2D original del paisaje, conservada.
- `Assets/Art/Intro/IntroPaisaje.shader` y `.mat`: renderizado de esa geometría con colores por vértice, independiente del shader de sprites del gameplay.
- `Assets/Editor/IntroHistoriaSceneBuilder.cs`: constructor reproducible que rechaza sobrescribir una introducción ya existente y solo lee el vehículo de gameplay.
- Este documento.

Modificados:

- `Assets/Scripts/Menu/MainMenuController.cs`: valida y carga la introducción manteniendo el reinicio y el fundido de música.
- `ProjectSettings/EditorBuildSettings.asset`: añade IntroHistoria después de MainMenu y mantiene todas las escenas previas.

El constructor no debe ejecutarse para probar una escena existente. La escena ya está guardada y puede editarse normalmente en Unity.

## Prueba manual

1. Abrir `Assets/Scenes/MainMenu.unity` y pulsar Play. Comprobar opciones, créditos y música; ajustar volumen de efectos.
2. Pulsar NUEVA PARTIDA. La música debe desaparecer suavemente; observar el recorrido completo de unos 40 segundos, los textos y los dos mensajes en orden.
3. Al entrar al exterior, comprobar movimiento WASD, cámara, HUD, linterna F y batería; camioneta intacta y objetivo inicial. Entrar al retén: el accidente no debe empezar antes de terminar la conversación original del supervisor.
4. Reiniciar desde MainMenu. Mantener Espacio brevemente y soltar: la barra vuelve a cero. Mantener un segundo: fundido y mismo estado inicial del exterior.
5. Probar Game View 1920 × 1080 y 1280 × 960, y un ejecutable Windows. Comprobar legibilidad, continuidad del movimiento y encuadre. La escucha de los nuevos efectos y voces requiere asignar primero las grabaciones pendientes.

Las comprobaciones automatizadas se ejecutan sobre una copia temporal con preferencias aisladas del juego. Los resultados y capturas están en `Temp/Intro`; esa carpeta está excluida de Git. La validación automatizada no sustituye una revisión del movimiento y del audio en el equipo del jugador ni una prueba de un ejecutable Windows.

El editor de la copia temporal presenta una excepción de indexación al arrancar (`UnityEditor.Search.SearchDatabase`, `ArgumentOutOfRangeException`) que ya aparecía antes de añadir el menú y la introducción. Se registra por separado y no se alteraron paquetes ni cachés para ocultarla. La comprobación de nuevos errores de ejecución excluye únicamente esa excepción conocida del editor.

## Comprobaciones realizadas

Pruebas en Play Mode de una copia aislada, con gráficos activos:

- Recorrido completo de unos 40 segundos y salida natural al exterior.
- Aparición progresiva de lugar/hora, orden y contenido exacto de los dos subtítulos.
- Movimiento del parallax, frenado, detención, apagado de faros y fundidos.
- Cambio efectivo de los niveles de las fuentes de audio al modificar el volumen de efectos; los clips pendientes siguen sin asignarse.
- Omisión mediante estados del teclado del Input System: progreso parcial, cancelación al soltar, un segundo de pulsación y salida única.
- Misma instancia de partida desde la introducción hasta el gameplay tanto al terminar como al omitir: no se reinicia dos veces.
- Preferencias de volumen conservadas, camioneta intacta, representación del árbol caído oculta y objetivo inicial en ambos recorridos.
- Movimiento de Leo mediante el Input System y seguimiento de cámara después de la introducción; entrada al retén sin activar el accidente antes de la conversación original.
- Fuentes del menú y de la introducción descargadas al cambiar de escena.
- Capturas reales de URP a 1920 × 1080 y 1280 × 960, revisadas visualmente.
- Sin errores nuevos de compilación ni errores de ejecución del juego en esa prueba; excepción conocida de indexación del editor registrada aparte.

Informes de la implementación inicial: `Temp/Intro/Introduccion-Pruebas.txt`. Las capturas de esa versión se conservan en `Temp/Intro`. Los cambios iniciales en `MainMenuController.cs` y `EditorBuildSettings.asset` no se volvieron a modificar durante las mejoras visuales.

También se repitieron y superaron las pruebas existentes del menú (78 comprobaciones: botones, navegación, opciones, créditos, bucle de música, preferencias, Nueva partida y salir) y del gameplay (62 comprobaciones: conversación original, accidente, descubrimiento, colisiones, rutas, cámara, batería, linterna, HUD, transiciones y acceso al generador). Informes: `Temp/Intro/Menu-Regresion-Pruebas.txt` y `Temp/Intro/Gameplay-Regresion-Pruebas.txt`.

La introducción superó 48 comprobaciones de integración. No se probó todavía un ejecutable Windows ni la escucha real de los efectos/voces pendientes.

## Mejoras visuales de la entrada al bosque

Referencia de composición: «Carretera Nocturna hacia el Bosque.png», revisada antes de editar. Se reutilizan sprites y niebla locales; la referencia no se añade como fondo ni recurso del juego.

Archivos de esta revisión:

- `Assets/Scenes/IntroHistoria.unity`: dos faros, cerca abierta, sendero, árboles del acceso, montaña y torre distantes; retirada de los objetos de caseta, cartel, poste y lámpara de esta escena.
- `Assets/Scripts/Intro/IntroHistoriaController.cs`: únicamente referencias al segundo faro y haz, con el mismo apagado gradual del primero. Se conservan duración, subtítulos, omisión, movimiento, audio, reinicio y transición.
- `Assets/Editor/IntroHistoriaSceneBuilder.cs`: el constructor inicial utiliza la nueva composición.
- `Assets/Editor/IntroVisualComposition.cs`: construcción local y protegida de luces y entorno; no sobrescribe recursos de mejoras existentes ni guarda escenas jugables.
- `Assets/Art/Intro/IntroMejoras.asset`: mallas locales del sendero, detalles, montaña, torre, luz roja y haces.
- `Assets/Art/Intro/IntroFaros.shader` y `IntroFaros.mat`: color blanco cálido, bordes suaves y caída de intensidad con la distancia, compatibles con URP 2D.
- Este documento; archivos `.meta` de los nuevos recursos.

La revisión de introducción superó 56 comprobaciones en Play Mode de la copia aislada: secuencia completa, ambos faros, composición, movimiento horizontal, subtítulos, fundidos, carga normal y omisión mediante Input System, estado de partida, cámara de gameplay, movimiento de Leo y entrada al retén. Sin errores nuevos de ejecución. Capturas URP revisadas a 1920 × 1080 y 1280 × 960; el encuadre se conserva con las bandas del sistema pixel perfect existente. Informe: `Temp/IntroMejoras/Introduccion-Pruebas.txt`; capturas: `Temp/IntroMejoras/intro-34.png` e `intro-34-4x3.png`.

También se repitieron y superaron las 78 comprobaciones del menú y las 62 del gameplay, incluidas entrada/salida del retén, accidente y descubrimiento, colisiones, HUD, batería, linterna, cámara y generador. Informes: `Temp/IntroMejoras/Menu-Pruebas.txt` y `Gameplay-Pruebas.txt`. La excepción previa de indexación del editor permanece registrada por separado.

La comparación SHA-256 de todos los archivos que ya existían en `Assets` y `ProjectSettings` antes de esta revisión detecta cambios únicamente en `IntroHistoria.unity`, `IntroHistoriaController.cs` e `IntroHistoriaSceneBuilder.cs`. Se conservan sin modificaciones el sprite original de la camioneta, las escenas jugables, los límites de cámara, el menú y las preferencias/configuración del proyecto. Los recursos nuevos y la escena final coinciden con los usados en la copia de pruebas. No se realizaron commits ni publicaciones.

Queda pendiente probar un ejecutable Windows. Las grabaciones de ambientación y voces continúan sin asignar, tal como en la introducción anterior; esta revisión no incorpora ni sustituye audio.
