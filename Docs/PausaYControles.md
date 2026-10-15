# Frecuencia Cero — Pausa y controles

Implementación del 13 de octubre de 2026. Sin commit ni push.

## Comportamiento

ESC abre la pausa durante la exploración y vuelve a reanudar desde cualquiera de sus pantallas. CONTINUAR también reanuda. Mientras se apunta una botella, ESC abre la pausa y conserva el apuntado; CLIC DERECHO cancela el apuntado. El menú contiene OPCIONES, CONTROLES y VOLVER AL MENÚ PRINCIPAL, con confirmación de salida y CANCELAR seleccionado inicialmente.

La pausa se instala automáticamente al cargar cualquier escena con un `SurvivalPlayer` activo. No hay que añadir componentes manualmente ni guardar cambios en las escenas. El Canvas oscurece la escena existente, usa el mismo tipo de letra y `MenuButtonHighlight` del menú principal, y adapta su escala con `CanvasScaler.Expand`.

`PauseMenuController` conserva y restaura `Time.timeScale`, `AudioListener.pause`, la selección de UI y la visibilidad del HUD. No desactiva componentes de gameplay ni sobrescribe sus bloqueos de control. Se rechaza la pausa durante diálogos/documentos, muerte, cinemáticas, movimiento deshabilitado, cargas, fundidos y el accidente del árbol que utiliza esperas de tiempo real. La interfaz y sus efectos usan tiempo no escalado.

`GameplayPause` bloquea las entradas del gameplay durante la pausa y hasta soltar E, H, R, F, Q, TAB, ESC, Enter y los botones del ratón usados al reanudar. El fotograma de cierre y el de liberación siguen bloqueados. WASD y SHIFT pueden seguir sostenidos al reanudar; no se exige soltarlos. La barrera de liberación se puede actualizar incluso cuando se desactiva el propietario de la pausa. Esto evita estados permanentemente bloqueados y clics de CONTINUAR que lancen botellas.

Las físicas y los temporizadores escalados quedan detenidos. Los eventos automáticos reciben guardas de pausa, los sonidos se suspenden mediante el listener y las transiciones de proximidad/volumen de audio no avanzan mientras están pausadas. Los avisos de HUD que antes dependían de tiempo no escalado usan un reloj de presentación que descuenta el tiempo pausado. Las preferencias de volumen siguen afectando a los AudioSources correspondientes.

OPCIONES reutiliza exclusivamente `MenuPreferences` y sus claves `FC.Audio.*` y `FC.Display.*`: música, efectos, pantalla completa y resoluciones compatibles. Se guardan al volver o reanudar. No se añade música de exploración ni un sistema de guardado. La salida carga MainMenu después de liberar la pausa; Nueva partida sigue utilizando el reinicio ya existente de misión, supervivencia, HUD y viajes.

## Botellas

El clic derecho ya existía en `BottleAim`. Se conservó su funcionamiento y se pulió: limpia `InRange`, libera inmediatamente el bloqueo de apuntado cuando `CanAct` lo permite y tiene prioridad sobre un clic izquierdo simultáneo. Q no inicia apuntado si se mantiene el botón derecho. La cancelación no crea proyectiles, no consume inventario y no emite ruido. Los bloqueos de muerte, narrativa y viaje siguen teniendo prioridad. La lectura de botones se omite durante la pausa y la liberación posterior; los proyectiles existentes congelan su actualización durante la pausa.

## Archivos de esta actualización

Nuevos, con sus `.meta`:

- `Assets/Scripts/UI/GameplayPause.cs`
- `Assets/Scripts/UI/PauseMenuController.cs`

Cambios pequeños sobre scripts existentes:

| Archivo | Cambio |
| --- | --- |
| `Assets/Scripts/Player/LeoMovement.cs` | Ignorar entradas mientras están bloqueadas por pausa; conservar velocidad, controles y física. |
| `Assets/Scripts/Player/SurvivalPlayer.cs` | Integrar pausa en `CanAct`, entradas, daño y reloj de avisos. |
| `Assets/Scripts/Player/BottleAim.cs` | Cancelación, prioridad de botones y barrera de entrada. |
| `Assets/Scripts/Player/BottleProjectile.cs` | Congelar el proyectil sin generar un impacto durante la pausa. |
| `Assets/Scripts/Interaction/PlayerInteraction.cs` | Bloquear E, Enter, ESC y TAB del gameplay; limpiar el foco. |
| `Assets/Scripts/Lighting/FlashlightController.cs` | Bloquear alternancia y apuntado mientras está pausado. |
| `Assets/Scripts/Lighting/GeneratorEntryLamp.cs` | No iniciar eventos automáticos durante la pausa. |
| `Assets/Scripts/World/Escarbador.cs` | Ignorar nuevos avisos de ruido mientras está pausado; conservar detección y estados fuera de la pausa. |
| `Assets/Scripts/World/TreeFallEvent.cs` | Exponer si está ejecutándose para impedir pausa durante el accidente; guarda de actualización y volumen actualizado. |
| `Assets/Scripts/World/TruckAccident.cs` | No iniciar un diálogo por proximidad durante la pausa. |
| `Assets/Scripts/World/RootsEncounter.cs` | Congelar presentación/eventos y ocultar su panel durante la pausa; conservar la recuperación previa del encuentro. |
| `Assets/Scripts/Audio/LeoFootstepAudio.cs` | No avanzar el fade de pisadas durante la pausa. |
| `Assets/Scripts/Audio/LocalizedRiverAudio.cs` | Congelar proximidad y evitar reinicios de la fuente pausada. |
| `Assets/Scripts/Narrative/RetenMission.cs` | Aviso del fusible con reloj que descuenta pausa. |
| `Assets/Scripts/UI/NarrativeUI.cs` | Congelar avisos/fades y ocultar la interfaz bajo la pausa. |
| `Assets/Scripts/UI/SurvivalHUD.cs` | Ocultar HUD/apuntado durante pausa y conservar la duración de los avisos. |

Pruebas reproducibles fuera de `Assets`: `Tools/Pausa/PauseValidationEditor.cs`, `PauseValidationRuntime.cs`, `ChapterRegressionRuntime.cs` y `README.md`. La regresión reutiliza la auditoría anterior y adapta únicamente su prueba de cancelación al clic derecho. Los helpers solo se copian a una copia temporal para ejecutarlos; no se incluyen en el videojuego.

No se modificaron escenas, sprites, animaciones, audio original, cámaras/límites serializados, Input Actions, el menú principal ni ProjectSettings. Se conservaron los cambios pendientes de trabajos anteriores.

## Validación

Las pruebas se ejecutan en Unity 6000.6.3f1, con teclado y ratón sintéticos y clicks reales sobre UI de Unity, en `Temp/AuditoriaCapitulo1Unity`. Esa copia usa una identidad distinta de PlayerPrefs para no tocar las preferencias originales. Las capturas son renders de Game View, no fotografías del escritorio.

**72 comprobaciones de pausa aprobadas, 0 fallos.** Resultados detallados: [pruebas de pausa](PausaYControles/PruebasPausa.txt). Incluyen ciclos repetidos de ESC, caminar/correr por una zona libre, física y batería congeladas, clic derecho con izquierdo/Q simultáneos, apuntado durante pausa, ausencia de consumo accidental al cerrar, sliders con ratón, preferencias tras reiniciar Unity, documentos/cinemáticas/fundidos, Escarbador, muerte/reintento, RetenTravel, confirmación de salida, MainMenu → Nueva partida → omitir intro → exterior, limpieza al desactivar el propietario y restauración de una escala de tiempo previa distinta de 1.

También se comprueba que `AudioSource.timeSamples` y `AudioSettings.dspTime` permanezcan fijos en pausa y que el audio vuelva a avanzar al reanudar. Las cuatro pantallas se capturan y sus elementos se verifican dentro de resoluciones reales 1280×720 (16:9) y 960×720 (4:3). Capturas: `Temp/PausaValidacion/*_1280x720.png` y `*_960x720.png`.

Se valida por SHA-256 que todas las escenas y ProjectSettings siguen idénticos al inicio de esta tarea y que los nueve clips originales permanecen intactos. El resultado está en [protección del proyecto](PausaYControles/Proteccion.json).

## Comprobación manual pendiente

- Jugar con el teclado y ratón habituales; pausar con WASD/SHIFT sostenidos y reanudar cerca del Escarbador.
- Apuntar, pausar y usar CONTINUAR; comprobar después un lanzamiento intencional y la cancelación con derecho.
- Escuchar el audio con auriculares: pausa/reanudación, volumen y ausencia de duplicados. La prueba de DSP no sustituye la evaluación auditiva.
- En un ejecutable Windows, comprobar pantalla completa, resoluciones del monitor y volumen tras cerrar y volver a abrir el juego. El editor no reproduce todos los cambios de ventana del ejecutable.
- Realizar una partida manual completa, incluido el cierre del capítulo I y CONTINUAR al capítulo II. Las pruebas automatizadas de regresión usan teletransportes e inventario de prueba; no equivalen a recorrer todo el capítulo jugando.

No quedan efectos sonoros nuevos por añadir como parte de esta tarea. Se conservan los sonidos pendientes ya registrados: Escarbador, pasos específicos de puente, motor más realista, linterna y puertas.
