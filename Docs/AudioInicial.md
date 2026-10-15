# Audio inicial — capítulos I y II

Unity 6000.6.3f1 / URP 2D. Originales en `Assets/Audio/Sonidos`; copias derivadas y paleta en `Assets/Audio/Procesados`. No se añaden música de exploración, voces, crujidos misteriosos ni cambios de controles, velocidad o detección del Escarbador.

## Originales e importación

Se encontraron los nueve archivos: pasos_en_tierra.wav, pasos_en_grass.wav, pasos_en_retenv1.aiff, pasos_en_retenv2.wav, sonido_auto.wav, auto_apagandose.wav, puerta_cerrandose.wav, radio_interferencia.wav y agua_rio.flac. V1 apareció en la comprobación posterior y se incluyó antes de asignar escenas.

Unity cargó los nueve como AudioClip antes de modificar escenas en la copia de validación. El FLAC ya es compatible con esta versión del editor; además se creó un WAV de río procesado. La decodificación local verifica CRC de frames y MD5 PCM del FLAC conforme a la [especificación FLAC, RFC 9639](https://www.rfc-editor.org/rfc/rfc9639.html). No fue necesario instalar programas ni paquetes.

Los originales conservan sus bytes. `PreparacionAudio.json` registra SHA-256, intervalos de origen, duración, ganancia, filtro, fades y hashes de cada derivado.

## Recortes y niveles

Hay **32 WAV PCM16 mono a 44.1 kHz**. Los ataques tienen fades de unos milisegundos y las colas se desvanecen. Se eliminó DC, se aplicó un filtro de graves suave y se ajustó RMS con límite de pico mediante ganancia del clip completo, sin un limitador que aplaste el sonido.

| Categoría | Derivados | Uso |
| --- | --- | --- |
| Tierra | 4 pisadas individuales de 0.25 s | Senderos y superficies de tierra |
| Césped | 6 pisadas de 0.25 s | Vegetación fuera del camino |
| Retén V2 | 8 pisadas de 0.25 s | N1_InteriorReten |
| Cabaña V1 | 7 recortes de 0.25 s; 5 asignados | Habitaciones cuyo suelo real es madera; los dos últimos recortes se excluyen del banco por ser demasiado débiles |
| Motor en marcha | Origen 4–12 s, bucle procesado de 7.5 s | Conducción de la introducción; se descartó el transitorio inicial 0–4 s |
| Motor apagándose | Origen 0–2.23 s | Detención real y respaldo si se omite la introducción |
| Puerta de camioneta | Origen 4.65–5.30 s | Primer momento de Leo a pie en el exterior |
| Radio | Dos fragmentos de 1.1 s y corte de 0.18 s | Cues narrativos existentes, sin bucle |
| Río | Origen 20–44 s, bucle de 23.5 s con unión cruzada | Sólo proximidad a Agua_Norte / Agua_Sur del exterior |

Los rangos exactos de todas las pisadas están en el JSON. Los WAV procesados tienen margen de pico; no se altera el original para compensar un nivel de grabación. El volumen final respeta `MenuPreferences.EffectsVolume`.

## Pisadas y superficies

`GameSoundPalette` es un recurso de datos compartido, sin otro administrador persistente. `FootstepSurfaceMap` reconoce retén/cabaña y usa los campos de terreno existentes para senderos exteriores, generador, campamento/claro y sendero del capítulo II. La curva de acceso a la reja se serializa a partir de su receta real, sin inventar otra carretera.

`LeoFootstepAudio` escucha los eventos Walk/Run que ya emite `MovementNoise`: mantiene sus intervalos de 0.48/0.28 s, radios e intensidades. No emite eventos de ruido ni consulta/modifica la IA. Los desplazamientos de `CinematicWalk` tienen sonido separado, sin generar nuevos eventos para el enemigo. El selector evita repetir inmediatamente un clip y usa un RNG propio para no consumir el estado aleatorio del gameplay. Al detenerse, bloquear controles o abrir un cuadro, el sonido se desvanece rápidamente y deja de reproducirse.

V2 queda reservado al retén. V1 se aplica en las siete habitaciones de la casa, incluido el baño: sus suelos actuales son tablones. No se fuerza madera en otra superficie. Los crujidos contenidos en V1 permanecen ligados a los recortes de pisada; no existe un emisor aleatorio de crujidos ambientales.

## Camioneta, radio y agua

La conducción real está en IntroHistoria: el AudioSource existente del motor recibe el fragmento sostenido, baja de volumen durante su desaceleración original 28–36 s y se detiene en el evento de 36 s, cuando suena el apagado. No se usa el comienzo de sonido_auto como un arranque. No se cambia la duración, los subtítulos, la opción de omitir ni el movimiento de la cinemática.

`VehicleArrivalSound` termina los efectos en N1_ExteriorReten. Si la cinemática ya reprodujo el apagado, sólo suena la puerta una vez; si se omite antes o se entra directamente desde el editor, reproduce apagado y puerta. El alcance depende de la distancia de Leo a la camioneta. Una referencia a la sesión evita repetirlo al regresar y se renueva al iniciar Nueva partida. Los controles permanecen disponibles.

**Limitación visual existente:** no hay animación de Leo descendiendo de la camioneta en el exterior; Leo ya aparece a pie. La puerta se sincroniza con esa aparición. No se creó una bajada ficticia ni se bloqueó la jugabilidad para representarla.

Se conectan los cues radio_static/radio_cut ya existentes mediante `ChapterAudio` / `NarrativeDirector`. Se conservan los textos y tiempos de avance. La interferencia es breve y de nivel moderado; las voces definitivas siguen siendo slots opcionales sin sintetizar. El cierre y la radio del prólogo también usan el recurso real en lugar de la estática sintética anterior.

`LocalizedRiverAudio` reutiliza los colliders del agua que ya representan el río. Calcula la proximidad plana a esa geometría, evita la distancia Z de la cámara 2D, aplica panorama leve y fades, y deja de reproducir fuera del radio de 6 unidades. No existe otro río creado para justificar el sonido ni audio de agua global en el bosque. El agua no emite eventos hacia el Escarbador.

## Archivos y validación

Se configuran IntroHistoria, las cuatro escenas jugables del capítulo I y las ocho del capítulo II; MainMenu, su música y su logo conservan sus archivos. Las escenas sólo reciben componentes/referencias de audio. Los scripts de movimiento, ruido, IA, inventario, HUD y cámara no se editan.

Scripts nuevos: `Scripts/Audio/{GameSoundPalette,FootstepSurfaceMap,LeoFootstepAudio,LocalizedRiverAudio,VehicleArrivalSound,IntroVehicleSoundMarker}`. Recetas explícitas: `Editor/AudioSourceImportAudit`, `Editor/InitialSoundSetup`. Herramientas reproducibles sin dependencias externas: `Tools/Audio/prepare_audio.py`, `audio_formats.py`, `flac_decode.py`, `verify_audio.py`.

Los informes de importación, señal, protección de componentes y Play Mode están en `Temp/AudioInicial`. La copia `Temp/AudioVerificationUnity` tiene preferencias aisladas; sus arneses no se publican en Assets del proyecto.

Resultados definitivos:

- **131 comprobaciones en Play Mode:** movimiento real por Input System, tierra/césped, V2 del retén, V1 en las siete habitaciones, pasos detenidos, variantes, ritmo de correr, eventos de ruido intactos, preferencias, agua por proximidad, radio finita, inventario/linterna en transiciones, llegada única y conducción/detención original de 40 s.
- **14 escenas del Build abiertas sin scripts ausentes**; no se cambió la lista del Build.
- **9 originales intactos por SHA-256** y **32 WAV validados**: PCM16 mono/44.1 kHz, picos bajo 0.5, bordes de recorte sin discontinuidad y pasos de menos de 0.27 s.
- **32 clips importados comprobados con GetData en Unity**, conservando el margen de volumen; se desactivó la conversión/normalización a mono del importer porque los WAV ya son mono.
- **1.331 componentes de física, movimiento, IA y cámara comparados antes de copiar las escenas: sin diferencias**. Los archivos validados se incorporaron al proyecto y su comparación con la copia está en `Comparacion.json`.
- Sin errores nuevos de compilación ni gameplay en las pruebas definitivas. Se distinguen las excepciones de arranque anteriores de SearchDatabase / TutorialModalWindow.

Registros definitivos: `Pruebas-v3.log`, `Pruebas.txt`, `NivelesImportados.txt`, `ValidacionSenal.json`, `ProteccionJugabilidad.json`, `ImportacionOriginales.txt`. Para repetir la preparación: `py Tools/Audio/prepare_audio.py`; para comprobar los archivos: `py Tools/Audio/verify_audio.py`. Revisar la paleta antes de volver a ejecutar la receta de escenas; `InitialSoundSetup.Apply` rechaza una paleta ya existente.

**Pendiente de escucha humana:** la herramienta de este entorno no admite escuchar audio. Se verifican señal, niveles, duración, referencias y reproducción en Unity; no se presenta esa validación como una audición. Revisar con auriculares la naturalidad de cada pisada, el fragmento del motor, el golpe de puerta, los crujidos de V1 y el balance frente a futuras voces. Separar crujidos aislados sólo después de esa escucha, sin añadir disparos narrativos automáticos. El Build de Windows requiere una prueba final habitual. No se hacen commits ni pushes.
