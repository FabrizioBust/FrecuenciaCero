# Pulido del nivel 1 e inventario de botiquines

Segunda pasada sobre las escenas existentes en Unity 6000.6.3f1 / URP 2D. El menú y la introducción se conservan. Las pruebas se realizan en `Temp/GateVerificationUnity`, con preferencias aisladas del juego original; los arneses quedan fuera de Assets del proyecto principal.

## Escenarios y niebla

Se modifican `Assets/Scenes/Nivel01/N1_Generador.unity` y `N1_BosqueAbandonado.unity`. Se añaden arboledas con varios tamaños y variantes de los pinos y árboles existentes, vegetación baja agrupada, raíces, troncos caídos, rocas con musgo, hojarasca y ramas en los márgenes. La vegetación acompaña las cercas, el refugio, los materiales y el claro. Se mantiene libre el recorrido entre interacciones y se conserva el área central de evasión. Los nuevos troncos sólidos tienen colisiones en su base; las hojas y raíces bajas son decoración transitable. Los límites físicos originales se conservan.

Los terrenos `SueloGenerador.png` y `Survival/SueloBosque.png` reciben variaciones de humedad, musgo, pequeñas hojas y tierra irregular, manteniendo los trazados existentes. Se usan principalmente sprites del atlas exterior y el mismo material iluminado del proyecto. No se cambian luces, cámara, controles, navegación del Escarbador ni la reparación.

Se reutiliza `ForestFogController`, su textura de bancos y el pool existente. En ambas escenas hay 16 bancos activos de 24 disponibles, desplazados lentamente a 0.025 unidades por segundo, con envolvente de aparición y desaparición y tinte gris azulado. La capa ambiental tiene una textura de nubes con transparencia variable; no es un rectángulo uniforme. Las capas bajas quedan detrás de Leo y los props, y una capa superior tenue aporta profundidad. Los campos de densidad reducen la niebla sobre los caminos y las zonas de interacción. `AbandonedForestField` proporciona el campo del campamento y el claro; la rama original usada por el retén conserva su comportamiento.

`Assets/Editor/LevelOneVisualPolish.cs` contiene la receta explícita de autoría. No se ejecuta automáticamente y rechaza repetir una pasada ya existente. Los recursos añadidos están en `Assets/Art/Nivel01/VisualPolish`.

## Lectura y narrativa

Se mantiene `NarrativeDirector`, el controlador de E y los documentos originales. `NarrativePanels` comparte fondo oscuro semitransparente, borde fino, colores, tipografía y cálculos de tamaño entre el lector y `RootsEncounter`.

- Las notas cortas se centran y ajustan su altura al contenido. Muestran título pequeño y E / Escape para cerrar, sin barra de desplazamiento innecesaria.
- Los documentos largos tienen mayor altura y desplazamiento cuando su contenido supera el espacio disponible. El mapa conserva su ilustración; la bitácora conserva su texto y progreso de lectura.
- Los diálogos normales y el susurro de Leo usan un panel inferior ajustado al texto, nombre del personaje y fundido de 0.18 segundos. Los tutoriales usan el mismo estilo, centrados y con un acento ocre para distinguirlos.

No se cambian las frases, los tiempos de la cinemática, las teclas de avance ni los callbacks de misión. El HUD incorpora el contador de botiquines y deja espacio para los avisos de fusible y el objetivo en vistas estrechas.

## Botiquines portátiles

E recoge el botiquín rojo del campamento (`campamento_botiquin_01`, en torno a -7, 7), usando su contorno blanco y la interacción existente. Se añade al inventario y desaparece. Recogerlo nunca aumenta la vida y se permite con los tres corazones completos. Si el inventario está lleno, permanece disponible y su ID no se registra como recogido.

La capacidad inicial es **3**, compartida por todas las escenas y configurable en el Inspector del recurso `Assets/Resources/FrecuenciaCero/InventorySettings.asset`. El HUD muestra un icono rojo pixel art y `Botiquines: cantidad/máximo [H] usar`.

H consume exactamente uno si Leo está vivo, herido y puede actuar, y restaura un corazón. El corazón recuperado tiene un pulso breve, también durante el destello de invulnerabilidad del daño; el sprite recibe un tinte de curación cuando no está mostrando el daño. La curación no concede invulnerabilidad adicional. Con vida completa o sin botiquines se muestra un aviso breve y no se consume. Muerte, cinemática, lectura/diálogo, viaje y apuntado bloquean el uso.

Se amplía `MissionProgress.Survival` mediante `SurvivalProgress`; no se crea otro administrador ni otro jugador. Cantidad, vida actual e IDs persisten al cambiar o recargar escenas. El checkpoint conserva existencias, consumos e IDs, y mantiene su comportamiento previo de restaurar tres corazones al reintentar. Una recogida consumida no reaparece. Nueva partida reinicia la sesión sin borrar preferencias de audio o pantalla.

El progreso de inventario continúa siendo de sesión, como en la arquitectura existente. El único registro previo en disco es el marcador de finalización del nivel; esta pasada no añade un sistema completo de partidas guardadas ni un botón Continuar.

## Botella recogible

`VisualPolish/BotellaRecogible.png` es un sprite independiente de vidrio viejo verde oliva con etiqueta gastada y tapón marrón. Se dibuja en una cuadrícula de 8×16 píxeles artísticos; cada píxel ocupa cuatro texeles de una textura de 32×64 a 128 PPU, equivalente a la cuadrícula de 32 PPU de la escena. Usa Point, sin compresión ni mipmaps, y escala de Transform 1. Su altura es aproximadamente media unidad del mundo y mantiene el contorno de silueta existente.

Sólo se sustituye el SpriteRenderer de la recogida. `Survival/Botella.png`, `BottleAim`, `BottleProjectile`, el apuntado con Q, la trayectoria, el consumo, el sonido de impacto y la reacción del enemigo se conservan.

## Archivos de esta pasada

- Escenas: `N1_Generador.unity`, `N1_BosqueAbandonado.unity`.
- Terrenos: `Art/Nivel01/Generator/SueloGenerador.png` y `Art/Nivel01/Survival/SueloBosque.png`, con sus ajustes de importación cuando corresponde.
- Scripts modificados: `Narrative/SurvivalProgress`, `Player/SurvivalPlayer`, `Interaction/SupplyPickup`, `UI/SurvivalHUD`, `UI/HudLayout`, `UI/NarrativeUI`, `World/RootsEncounter`, `World/ForestFogController`.
- Scripts añadidos: `Narrative/SurvivalInventorySettings`, `UI/NarrativePanels`, `World/AbandonedForestField` y `Editor/LevelOneVisualPolish`, con sus metas.
- Recursos añadidos: configuración compartida de inventario; carpeta VisualPolish con botella, hojas, ramas y dos campos de bruma, con sus metas.
- Documentación: este archivo y la actualización de controles/vida en `Nivel1Final.md`.

## Comprobación y revisión manual

Los resultados definitivos y las capturas están en `Temp/PulidoVisualPortable`. La prueba integral aprobó **109 comprobaciones** nativas de recogidas, botiquines, H y bloqueos, ruido, botellas, colisiones, navegación, cinemática, tutorial, checkpoint, persistencia, final, menú, música, Nueva partida e introducción. Los arneses usan teclado y ratón reales del Input System, Physics2D y URP 2D. Las capturas del mundo se toman en 1920×1080 y 1280×960; las interfaces se capturan en el Game View de 2560×1440. También se verifica que los cálculos del panel compacto caben en 4:3.

La regresión del generador aprobó **72 comprobaciones**, incluyendo farola/parpadeo, caja examinada/abierta/vacía, fusible, reparación y diálogo posterior, viajes y lectura de los documentos originales. La prueba de interfaz aprobó **28 comprobaciones**, incluyendo el lector compacto, la bitácora, el mapa, los dos tutoriales, el diálogo del encuentro, H después de daño real, el contorno de la botella y la música del menú. Total: **209 comprobaciones aprobadas**.

Los informes son `Pruebas.txt`, `Pruebas-RegresionGenerador.txt` y `UI-Visual.txt`. La auditoría nativa de las seis escenas habilitadas encontró cero scripts faltantes y contornos asignados a todos sus interactuables activos. `Comparacion.json` confirma que los scripts de ejecución y las escenas publicadas son idénticos a la copia probada; no se eliminó ningún archivo preexistente. Menú, introducción, escenas del retén, sprites originales, movimiento, animador, linterna, scripts de cámaras, IA, proyectil, configuración URP y lista del Build mantienen sus hashes previos.

No hubo errores nuevos de compilación, shaders o gameplay en las pruebas definitivas. La copia del editor presenta excepciones de arranque anteriores en `UnityEditor.Search.SearchDatabase` y `Unity.Tutorials.Editor.TutorialModalWindow`; los informes las distinguen de los errores de ejecución del juego. Las ejecuciones diagnósticas previas quedan separadas de los informes definitivos.

Para revisión humana: recorrer el nivel desde el menú, comprobar el balance y la atmósfera en el monitor habitual, recoger el botiquín con E teniendo vida completa, recibir un golpe y pulsar H, viajar al generador y regresar, leer nota/bitácora/mapa y completar la evasión. Falta probar un Build final de Windows y valorar el resultado artístico. El arte provisional del Escarbador conserva la limitación anterior de una sola pose.

No se hacen commits, pushes ni cambios de configuración Git.
