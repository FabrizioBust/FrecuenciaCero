# Cierre del capítulo I y prólogo del capítulo II

Implementación para Unity 6000.6.3f1 / URP 2D, reutilizando `RetenMission.Session`, `RetenTravel`, Leo, su cámara, su inventario, el lector y los cuadros narrativos existentes. La escena inicial del Build continúa siendo MainMenu. No se crea otro administrador persistente.

## Recorrido

Escapar del Escarbador → salida segura del Claro de las Raíces → cinemática de cierre → **CONTINUAR** → **CAPÍTULO II — LA CABAÑA DEL GUARDABOSQUES** → llegada caminando al nuevo sendero → conversación por radio → descubrimiento de la cabaña y apagado de la ventana → E en la puerta → conversación con el desconocido → apertura → entrada a la sala principal.

La escena exterior nueva es `Assets/Scenes/Nivel02/N2_SenderoCabana.unity`. La sala está en `N2_CabanaGuardabosques.unity`, conectada ahora con seis habitaciones independientes. La lista del Build conserva las escenas anteriores y su orden.

## Cinemática de cierre

`RootsEncounter` conserva la primera aparición, el tutorial y la condición original de evasión. Al alcanzar la salida con la criatura retirada, entrega el cierre a `ChapterOneEnding`.

La secuencia dura aproximadamente **18 segundos**. Bloquea temporalmente los controles, deja caminar a Leo unos pasos mediante su Rigidbody2D y sus animaciones originales, y detiene la cámara con un desplazamiento suave. Después Leo se detiene y mira hacia el ruido de tierra, unas ramas se desplazan ligeramente, suena estática y se dispara el evento de susurro «Leo...». La criatura permanece oculta y congelada. Sigue una pausa y un fundido gradual.

El cierre utiliza ahora una versión limpia de la imagen de bosque proporcionada, con el Escarbador distante entre la niebla, una viñeta suave y textos/Button reales de Unity. Se retiró el encabezado FRECUENCIA CERO sólo de esta pantalla. Conserva la frase «Hay cosas en este bosque que prefieren permanecer ocultas». El antiguo rótulo de victoria y la instrucción para volver al menú se eliminan de la pantalla. El rediseño artístico se detalla en [ReconstruccionCabana.md](ReconstruccionCabana.md).

CONTINUAR es un Button de Unity con selección y resaltado. Enter o el clic lo activan mediante el EventSystem/Input System. Se bloquea al primer intento y `RetenTravel.IsLoading` añade la protección de carga existente. El fundido y el rótulo de capítulo son un overlay visual transitorio; `RetenTravel` sigue siendo el único cargador. No se reinicia la sesión.

El cierre registra su inicio en `MissionProgress.Chapters` y su final con `CompleteLevelOne`, incluido el marcador de PlayerPrefs ya existente. Si se recarga un cierre iniciado o completado, se recupera directamente la presentación final y se evita repetir la secuencia larga.

## Sendero y cabaña

El sendero tiene curvas, terreno irregular, árboles y pinos agrupados, vegetación baja, hojas, raíces, rocas, troncos y colisiones en sus bases. Los límites físicos están en torno a x ±17 y y ±15; el terreno decorado y la cámara cubren un área mayor. La niebla reutiliza `ForestFogController` y sus bancos con un campo visual propio, `CabinTrailField`, que despeja el sendero y la zona de la cabaña.

La llegada segura está en (-12, -13). Leo camina brevemente antes de recuperar WASD, la linterna y el HUD. La radio se activa cerca de (-8, -6), con un respaldo al aproximarse a la cabaña para que una ruta alternativa no omita la conversación. Se usan cuatro recursos `DialogueSequence` y `NarrativeDirector`, con pausas y estática entre las intervenciones. E/Enter revela y continúa las líneas como antes. No se introducen explicaciones sobre el origen de la señal ni sobre la identidad de la voz.

La cabaña está en torno a (7, 8.5). Reutiliza madera, techo, ventana, cercas, leña y herramientas de los atlas del proyecto. La ventana usa una capa de cristales cálidos y una Light2D tenue: ambas se apagan gradualmente al acercarse después de la radio. El objetivo pasa a «Investigar la cabaña del guardabosques».

La puerta usa un Sprite independiente recortado del atlas original, un umbral oscuro y una bisagra visual. El PNG original no se modifica. Tiene contorno blanco y E. Al llamar, hay una pausa inicial sin respuesta; después se presentan las tres líneas exactas de DESCONOCIDO / LEO / DESCONOCIDO. La hoja se abre durante 1.5 segundos y sigue el fundido hacia el vestíbulo. Al regresar al sendero, la luz continúa apagada y la puerta permanece abierta; las conversaciones y la llegada no se repiten.

## Continuidad y controles

Vida, botiquines, baterías, botellas, IDs recogidos y progreso siguen perteneciendo a la misma sesión. La carga y el encendido de la linterna viajan mediante `RetenTravel` / `RetenArrival`; la carga sigue su consumo normal cuando está encendida.

Los checkpoints del capítulo II usan la misma recuperación existente, con coordenadas de su propia escena: (-12, -13) en el sendero y (-1, -1.6) en el vestíbulo. Reintentar restaura los tres corazones como antes, conserva consumos e inventario y no teletransporta al campamento del capítulo I. Nueva partida también limpia los nuevos indicadores de capítulo.

`CinematicWalk` sólo mueve el Rigidbody durante secuencias autorizadas, con consultas de colisión; los controladores desactivan temporalmente `LeoMovement` y conservan sus ajustes. `LeoSpriteAnimator.Face` añade una pose de giro en reposo sin cambiar las animaciones, los flips ni los sprites originales. La linterna ignora F y el ratón únicamente cuando `SurvivalPlayer.Cinematic` está activo; mantiene su estado y recupera el comportamiento habitual al terminar. El HUD usa scopes temporales que conservan la preferencia de Tab y se liberan al viajar o interrumpir una secuencia.

No se modifica la IA, el generador, la recogida ni el uso de botiquines, el proyectil de botella, el sistema de ruido o los límites de las cámaras anteriores.

## Audio definitivo

No hay grabaciones de voz ni efectos narrativos adecuados en Assets: sólo está la música del menú. No se descargan ni sintetizan voces. Los diálogos se presentan en los cuadros existentes y el susurro tiene su propio cuadro del mismo estilo. La estática, el corte de radio y los golpes de puerta son efectos locales discretos; la tierra del cierre usa el sonido existente de `RootsEncounter`. Todos respetan el volumen de efectos.

`ChapterAudio.recordings` permite asignar AudioClips por identificador en el Inspector:

- `whisper_leo`: susurro final.
- `radio_warning`: voz entrecortada del sendero.
- `stranger_station`, `leo_technician`, `stranger_unsaid`: líneas de la puerta.

`onCue` y `CuePlayed` permiten conectar audio definitivo o sincronización adicional. Los componentes están en el controlador del cierre y en las raíces narrativas del capítulo II. Las grabaciones deben añadirse posteriormente; el prólogo funciona con subtítulos y eventos mientras tanto.

## Cabaña y alcance pendiente

La escena de entrada es ahora una sala principal decorada, conectada con seis habitaciones independientes: cocina, comedor, baño, despacho, dormitorio y almacén. La entrada tiene un breve movimiento y recupera el control. No se muestra al desconocido. Las puertas utilizan fundidos y llegadas emparejadas, manteniendo la sesión.

Quedan para el desarrollo posterior las misiones y documentos narrativos definitivos, el evento de desbloqueo del almacén, el personaje del desconocido y las revelaciones del capítulo II. Las habitaciones ya tienen arte, colisiones e interacciones neutrales. El objetivo «Explorar la cabaña del guardabosques» deja esa continuación preparada; no hay un segundo capítulo completo ni una victoria adicional.

La continuidad es de sesión, como en el sistema actual. Se conserva el marcador de capítulo I completado en disco. No se añade un guardado completo ni un botón Continuar al menú principal.

## Archivos

- Escena modificada: `Scenes/Nivel01/N1_BosqueAbandonado.unity`, con controlador, presentación y ramas del cierre.
- Escenas creadas: `Scenes/Nivel02/N2_SenderoCabana.unity`, `N2_CabanaGuardabosques.unity`, con sus metas.
- Scripts modificados: `World/RootsEncounter`, `Narrative/MissionProgress`, `Player/LeoSpriteAnimator`, `Lighting/FlashlightController`, `UI/HudVisibility`, `World/ForestFogController`.
- Scripts creados: `Narrative/ChapterProgress`, `Chapters/ChapterOneEnding`, `ChapterTwoPrologue`, `CabinDoor`, `CabinVestibule`, `CinematicWalk`, `ChapterAudio`, `ChapterTransitionOverlay`, `UI/ChapterVignetteGraphic`, `World/CabinTrailField`.
- Receta de autoría: `Editor/ChapterPrologueBuilder`, explícita y protegida contra sobrescribir escenas nuevas ya existentes.
- Recursos: `Art/Nivel02` (terreno, niebla, puerta, cristales cálidos, umbral y suelo del vestíbulo); `Narrative/Nivel02` (radio y desconocido); metas correspondientes.
- Configuración: `ProjectSettings/EditorBuildSettings.asset`, con los dos destinos nuevos.
- Documentación: este informe y actualización del cierre en `Nivel1Final.md`.

## Verificación

Las pruebas nativas, capturas e informes están en `Temp/Capitulos`. Se ejecutan en una copia con Unity 6000.6.3f1, URP 2D, Physics2D, Game View, teclado y ratón del Input System y preferencias de prueba aisladas. Los arneses permanecen fuera de Assets del proyecto principal.

La prueba de capítulos comprueba el disparador, los pasos de Leo, el giro, la cámara, las ramas, el orden de estática/susurro, la duración, la presentación, Enter y clic, la protección de transiciones, la conservación de datos, el sendero recorrido con WASD, la radio, el apagado de los cristales y la luz, la puerta, el desconocido, la bisagra, el vestíbulo, el regreso, el botiquín, el checkpoint y la recuperación sin repetir cinemáticas. Se revisan capturas de sendero y cabaña en 16:9 y 4:3 y de las interfaces reales.

La regresión integral del capítulo I comprueba la evasión real y todas las mecánicas de supervivencia antes de continuar al II, además del menú, Nueva partida e introducción. Se repiten las regresiones del generador y de la orientación/linterna para validar los cambios de presentación y la pose cinematográfica.

Resultado de la implementación original del prólogo: **310 comprobaciones superadas**, distribuidas entre cuatro recorridos nativos. La revisión posterior del cierre y la casa tiene **335 comprobaciones adicionales**, detalladas en [ReconstruccionCabana.md](ReconstruccionCabana.md):

| Recorrido | Comprobaciones | Informe en `Temp/Capitulos` |
| --- | ---: | --- |
| Capítulo I, menú, Nueva partida e introducción | 113 | `Pruebas.txt` |
| Generador y transiciones anteriores | 72 | `Pruebas-RegresionGenerador.txt` |
| Orientación, diagonales y linterna independiente | 68 | `Pruebas-Leo.txt` |
| Cinemática, teclado/ratón, capítulo II y regreso | 57 | `PruebasCapitulos.txt` |

La prueba de capítulos se volvió a ejecutar después de alinear los cristales cálidos con la ventana original. El registro final es `PruebasCapitulos-VentanaFinal.log`. No se detectaron nuevos errores de compilación ni de gameplay. Las excepciones de arranque anteriores de SearchDatabase / TutorialModalWindow se distinguen de los errores del juego en los arneses.

Se copiaron al proyecto principal las escenas y los recursos validados. `Comparacion.json` registra la comparación SHA-256: **220 archivos** de scripts, metas, escenas, recursos nuevos, receta y lista del Build coinciden con la copia probada. La comparación contra la auditoría inicial no detecta archivos eliminados. Menú, introducción, exterior/interior del retén, generador, sprites originales, controles físicos, IA y sus configuraciones anteriores conservan su contenido previo a esta tarea.

Pendiente de revisión humana: ritmo y percepción de la secuencia en el equipo habitual, balance y dirección artística, voces definitivas y Build final de Windows. Para comprobarlo manualmente:

1. Abrir MainMenu, iniciar Nueva partida y completar el recorrido del capítulo I. Evadir al Escarbador y alcanzar la salida segura.
2. Revisar los pasos, giro, cámara, movimiento de ramas, estática y subtítulo; esperar el cierre de unos 18 segundos y seleccionar CONTINUAR con Enter o clic.
3. Recorrer el sendero con WASD, continuar los cuatro cuadros de radio y observar cómo se apaga la ventana al acercarse a la cabaña.
4. Acercarse al contorno blanco y pulsar E. Continuar los tres cuadros, esperar la apertura, entrar y regresar con E. Comprobar vida, existencias y linterna.
5. Crear y ejecutar el Build de Windows con el perfil existente. Revisar las interfaces en las resoluciones habituales y comprobar el volumen de efectos con el audio definitivo cuando se incorpore.

No se hacen commits ni pushes.
