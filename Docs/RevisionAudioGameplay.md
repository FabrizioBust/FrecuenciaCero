# Revisión del audio de gameplay — 14 de octubre de 2026

## Causa demostrada y corrección

El Unity abierto utiliza `C:/Users/fabri/Downloads/FrecuenciaCero/FrecuenciaCero`, editor 6000.6.3f1. Se consultó ese mismo editor, su escena cargada y sus preferencias; no se basó el diagnóstico únicamente en una copia de pruebas.

`MenuPreferences.EffectsVolume` devolvía **0**, leído de la preferencia `FC.Audio.Effects`. Los scripts multiplican sus ganancias por ese valor, por lo que tanto pisadas como camioneta, agua y cues narrativos quedan silenciados. La previsualización de un AudioClip en el Inspector no utiliza esta preferencia del videojuego.

Se reprodujo el fallo antes de corregirlo: en Play Mode real Leo generó siete eventos de pisada, se seleccionó un clip válido y el contador de reproducción llegó a siete, pero la salida de las pisadas, el vehículo y el AudioListener fue exactamente cero. Los clips y componentes existían; `PlayedCount` y la compilación por sí solos no demostraban sonido.

Se restauró **solamente el volumen de efectos a 0,8 (80 %)** mediante `MenuPreferences.SetEffects(.8f)` y `PlayerPrefs.Save()` dentro del editor original. La música permaneció en `0.3097712`. No se borraron preferencias ni se cambiaron resolución, progreso, dispositivos de audio o configuraciones de Unity. `EditorMasterMute` ya era false y permaneció false; el AudioListener tenía volumen 1 y no estaba pausado.

No se añadió un restablecimiento automático del volumen al comenzar una partida: cero sigue siendo una opción válida si el jugador decide silenciar los efectos.

## Ubicación de las conexiones

El editor confirmó el GameObject `Sistemas_Leo/Leo` en `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`. Tiene `AudioSource` y **Leo Footstep Audio**, habilitado, con `PaletaSonidos`, `Superficies reales para pisadas` y ganancia 0,72. El componente aparece al final del Inspector; no faltaba en la escena cargada. La misma asociación está guardada en las doce escenas jugables.

| Sonido | Conexión real |
| --- | --- |
| Tierra: 4 variantes | `LeoFootstepAudio` en Leo; `FootstepSurfaceMap` reconoce el camino en exterior, generador, bosque y sendero de cabaña. |
| Césped: 6 variantes | El mismo componente cuando el mapa identifica vegetación fuera del sendero. |
| Retén V2: 8 variantes | Leo en `Assets/Scenes/Nivel01/N1_InteriorReten.unity`, mapa `RetenInterior`. |
| Cabaña V1: 5 variantes | Leo en las siete escenas `Assets/Scenes/Nivel02/N2_Cabana*.unity`, mapa `CabinInterior`. |
| Motor en marcha y apagado | `IntroHistoriaController` en `Assets/Scenes/IntroHistoria.unity`; clips procesados asignados a `engineLoop` y `engineStop`. |
| Apagado pendiente al omitir intro y puerta de camioneta | `VehicleArrivalSound` en `Llegada sonora única de la camioneta`, exterior del retén; evita repetir la llegada dentro de la misma sesión. |
| Agua | `LocalizedRiverAudio` en `Agua sólo junto al río existente`, exterior; referencias a los colliders de `Agua_Norte` y `Agua_Sur`, distancia audible 6. |
| Radio | `ChapterAudio`, grabaciones `radio_static` y `radio_cut`; conexiones a los cues narrativos del interior, generador y capítulo II. |

La paleta común es `Assets/Audio/Procesados/PaletaSonidos.asset`. Se revisaron los GUID, los propietarios de los componentes, las referencias de escenas y paleta, los AudioSources y los AudioListeners: no hay referencias de audio sin resolver ni IDs duplicados en las escenas. AudioManager tiene audio habilitado y volumen global 1. Cada escena correspondiente conserva un AudioListener en Main Camera.

Hay **32 WAV procesados**. No todos se eligen para reproducirse actualmente: los recortes V1 `MaderaCabana_06.wav` y `MaderaCabana_07.wav` se excluyeron del banco por su nivel/calidad; `Interferencia_02.wav` está preparado en la paleta, pero los cues actuales asignan la primera interferencia y el corte. Las pisadas utilizan 23 variantes. Tener archivos preparados o referencias en una paleta no significa que cada uno se reproduzca en todos los niveles.

Un AudioSource sin clip en Edit Mode es normal aquí: el script asigna el clip al caminar o al iniciar su secuencia. No hace falta arrastrar los originales al AudioSource ni activar un bucle de pisadas.

## Pruebas en el editor original

Se utilizaron pulsaciones sintéticas de Input System para caminar y correr mediante los controles existentes. No se teletransportó a Leo, no se cambió su velocidad y no se emitieron eventos de ruido artificiales. Los dispositivos y ajustes temporales de entrada se retiraron/restauraron al terminar, y el editor volvió a la escena original sin guardar cambios.

| Caso | Pisadas / eventos de ruido | Pico de salida de pisadas | Pico de vehículo | Pico del AudioListener |
| --- | --- | --- | --- | --- |
| Antes, efectos 0 | 7 / 7 | 0 | 0 | 0 |
| Exterior directo, efectos 0,8 | 7 / 7 | 0,1444851 | 0,04793798 | 0,1418011 |
| MainMenu → botón Nueva partida → omitir introducción → exterior | 7 / 7 | 0,1443676 | 0,05079582 | 0,1559402 |

En los tres casos el contador dejó de avanzar al quedarse quieto. Se comprobó también que Nueva partida conserva el ajuste corregido. Las mediciones utilizan `AudioSource.GetOutputData` y `AudioListener.GetOutputData` mientras Play Mode está activo; muestran señal reproducida y mezclada, no solamente llamadas a `Play()`.

Las pruebas anteriores se realizaron con preferencias aisladas y volúmenes de prueba. No representaban el valor cero guardado en este editor. La carpeta abierta y sus componentes actuales sí contienen la integración; la diferencia comprobada es la configuración de volumen.

Registros: [antes](RevisionAudioGameplay/baseline_Play.txt), [exterior directo](RevisionAudioGameplay/direct_Play.txt), [desde el menú](RevisionAudioGameplay/menu_Play.txt), [corrección de preferencias](RevisionAudioGameplay/CorreccionVolumen.txt), [conexiones](RevisionAudioGameplay/ConexionesEscenas.json), [protección](RevisionAudioGameplay/Proteccion.json).

**Límite de la verificación:** no puedo escuchar la salida física del equipo. La señal PCM se verificó en el editor real, pero eso no certifica los altavoces, auriculares ni la percepción humana. Las otras habitaciones y todos los eventos narrativos se revisaron por sus conexiones; no se presenta esa revisión como una audición de cada escena.

## Archivos y estado

- Ningún script existente del gameplay ni escena se modificó: se compararon los 762 archivos existentes de Assets y ProjectSettings por SHA-256, sin diferencias. Incluye originales, WAV procesados, controles, IA, mecánicas y narrativa.
- Nuevo `Assets/Editor/GameplayAudioDiagnostics.cs`, con su `.meta`: herramienta de diagnóstico exclusiva del editor, selección de Leo y restauración explícita del volumen. No entra en el ejecutable ni cambia escenas automáticamente al jugar normalmente.
- Este informe y sus registros en `Docs/RevisionAudioGameplay/`.
- Sin commit ni push.

## Comprobación manual en Unity

1. Abre `Assets/Scenes/MainMenu.unity`, entra en Play y selecciona OPCIONES. Comprueba **Volumen de efectos: 80 %** y vuelve.
2. Inicia NUEVA PARTIDA; puedes omitir la introducción manteniendo Espacio. Camina y corre con WASD y SHIFT; al detenerte no deben empezar nuevas pisadas.
3. Prueba también `Assets/Scenes/Nivel01/N1_ExteriorReten.unity` directamente. Al iniciar una sesión nueva se reproducen apagado pendiente y puerta según la continuidad existente.
4. En Hierarchy selecciona `Sistemas_Leo → Leo`. Desplázate al final del Inspector y encuentra `AudioSource` y `Leo Footstep Audio`. Comprueba Sounds, Surface Map y Gain; el clip del AudioSource cambia durante la reproducción.
5. Puedes usar **Tools → Frecuencia Cero → Audio → Seleccionar Leo y sus pisadas** para localizar el objeto, o **Guardar diagnóstico de escena actual** para registrar su estado. Los diagnósticos de uso manual se guardan en `Temp/RevisionAudio`.
6. Escucha con auriculares tierra/césped, entra al retén con E para escuchar V2, y después comprueba V1 en la cabaña. Acércate al río y aléjate para revisar su volumen por proximidad. Activa las comunicaciones narrativas para escuchar radio.
7. Comprueba que el botón **Mute Audio** de Game View esté desactivado y que los altavoces/auriculares tengan volumen. Si no escuchas pese al 80 %, guarda el diagnóstico durante Play para comparar el estado exacto; no basta la previsualización del WAV.
