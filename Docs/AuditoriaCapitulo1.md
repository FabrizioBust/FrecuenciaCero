# Auditoría del capítulo I — preparación de la exposición

Fecha: 13 de octubre de 2026. Unity 6000.6.3f1, URP 2D. Preparación para el 25 de octubre.

## Alcance y método

Se inspeccionaron MainMenu, IntroHistoria, las cuatro escenas del capítulo I, sus interacciones y componentes, el cierre y la entrada al capítulo II. Se abrieron las 14 escenas habilitadas del Build. La fase inicial mantuvo intactos los 276 archivos existentes fotografiados por SHA-256: scripts, escenas, configuración y originales de audio.

Las pruebas se ejecutaron con Unity real en `Temp/AuditoriaCapitulo1Unity`, una copia aislada con preferencias de prueba independientes. Se reutilizaron las versiones de paquetes ya almacenadas, sin instalar programas. El editor del proyecto principal no se cerró. Las entradas de teclado y ratón se sintetizaron mediante Input System y se limitaron a los dispositivos de prueba de esa instancia, sin alterar los controles del juego original.

**La prueba del recorrido es automatizada, no una partida manual.** Usa teclas reales del Input System para movimiento, E, tutoriales, Q, Escape, clic, H, Nueva partida y CONTINUAR. Reubica a Leo en puntos físicamente válidos para acceder a las interacciones; por tanto, no demuestra por sí sola que todos los tramos entre esos puntos se recorran cómodamente con WASD. También prepara inventario, salud y algunos eventos de ruido para comprobar casos concretos.

## Hallazgos clasificados

| Gravedad | Hallazgo demostrado | Evidencia y tratamiento |
| --- | --- | --- |
| CRÍTICO | Ninguno demostrado en el recorrido normal probado | El recorrido automatizado logró reparar el generador, evadir al Escarbador, mostrar la tarjeta final y entrar al capítulo II. La partida manual y la revisión del ejecutable siguen siendo necesarias. |
| IMPORTANTE — corregido | Interrumpir el primer encuentro mediante desactivación/reactivación de su GameObject dejaba la cámara sin seguimiento, al enemigo congelado y `playing` activo | Dos reproducciones con Unity, antes del cambio, fallaron las comprobaciones de limpieza y reactivación. `OnDisable` sólo quitaba `Cinematic`, mientras Unity detenía la coroutine del objeto. Se corrigió exclusivamente `RootsEncounter.cs`. |
| IMPORTANTE — pendiente de decisión | No existe un menú de pausa durante el gameplay | Búsqueda global de scripts y revisión de controles: Escape cierra documentos o cancela apuntado; no hay una interfaz de pausa ni una rutina de pausa del juego. No se añade porque introduciría una funcionalidad nueva. |
| MENOR — recomendación | Advertencias por APIs antiguas de Unity | La compilación avisa de `FindObjectsSortMode` y sobrecargas antiguas de `FindObjectsByType`. Se recomienda migrarlas posteriormente, sin una sustitución masiva durante el pulido. |
| MENOR — paquetes/build | Advertencias de shaders de Unity Inference/Sentis | El Build terminó con 510 advertencias. El log incluye variantes no admitidas de ConvGeneric y otros shaders Hidden/Sentis, además de CS0618. Los scripts del gameplay no utilizan ModelAsset, Tensor o Unity Inference. No se quitaron paquetes ni se alteraron shaders ajenos para reducir el número. |
| MENOR — editor | Excepciones del indexador SearchDatabase y avisos de servicios del editor | Se distinguieron de fallos de gameplay en los informes. No se cambiaron paquetes ni permisos para resolver servicios auxiliares del editor. |

Una comprobación inicial del botiquín asumía que el combate había causado exactamente un corazón de daño. Esa suposición no era válida: se corrigió la preparación de la prueba y se verificó H con salud e inventario controlados. **No se modificó el sistema de salud ni se presenta esa comprobación del arnés como un fallo del juego.** Las preparaciones fallidas del arnés de entrada, cierre de documentos y selección de objetos tampoco se registran como bugs del proyecto.

## Corrección aplicada

`Assets/Scripts/World/RootsEncounter.cs` conserva el seguimiento anterior de la cámara y la escala original del suelo durante el encuentro. Al interrumpirse:

- Detiene la coroutine y restaura cámara, suelo y controles.
- Libera el estado `playing` y los cuadros temporales del tutorial.
- Recupera el encuentro mediante el criterio de checkpoint ya usado por la recarga de escena y el método existente `Escarbador.ResetAtHideout`.

La ruta normal, los diálogos, tiempos, sonidos, detección de ruido y reglas de persecución permanecen iguales. La recuperación de una interrupción retoma desde el checkpoint completado, como la recuperación existente al recargar; no reproduce una segunda introducción del encuentro.

No se editaron escenas, colliders, límites de cámara, scripts de movimiento, inventario, HUD, linterna, IA o menú. Los 15 archivos `.unity` existentes conservan sus hashes, incluidos los 14 habilitados y SampleScene. Los nueve originales de audio están intactos. No se añadió música, contenido ni sonidos provisionales nuevos.

## Validación definitiva

**120 comprobaciones en Play Mode: 120 correctas, 0 fallidas.** Informe completo: [PruebasUnity.txt](AuditoriaCapitulo1/PruebasUnity.txt).

- Nueva partida desde el menú con Enter; carga de la introducción y omisión manteniendo Espacio; preferencias conservadas.
- WASD, diagonales sin velocidad extra, carrera a la velocidad existente y detención; pisadas individuales y silencio en reposo.
- Entrada/salida del retén, radio, documentos, accidente y descubrimiento de camioneta; objetivos y liberación de controles.
- Reja, examen del generador, caja, apertura, fusible, reparación, anomalía y acceso al bosque. Se comprobó la selección del objetivo correcto entre suministros cercanos.
- Salud, existencias, carga y encendido de linterna conservados al viajar; checkpoint libre de solapamiento sólido.
- Tutorial del Escarbador, búsqueda y retirada al permanecer quieto; detección de botella; persecución por ruido continuo.
- Q bloquea a Leo, Escape cancela, una botella realmente lanzada consume inventario y emite ruido en el impacto; H recupera un corazón y consume un botiquín.
- Muerte y reintento conservan la misión; cierre, tarjeta, selección de CONTINUAR y Enter cargan el capítulo II; llegada libera controles y elimina el overlay.
- Dos interrupciones del encuentro después de la corrección restauran cámara, criatura y control sin coroutine bloqueada.
- Una cámara y un AudioListener por escena; 14 escenas abiertas sin scripts ni referencias rotas; 7 rutas y destinos de aparición emparejados.

Las pruebas del audio comprueban datos, referencias, reproducción y estado de fuentes; **no equivalen a escucha con auriculares**. No se afirma haber evaluado auditivamente naturalidad, crujidos o balance.

**Build de desarrollo Windows x64 generado correctamente:** 0 errores, 510 advertencias, 345.711.152 bytes, 5 minutos y 13 segundos. Usa la identidad original del producto y las mismas escenas y ajustes del proyecto. Los helpers de auditoría son Editor-only y no se incluyen como sistemas de gameplay. Resultado: [BuildWindows.txt](AuditoriaCapitulo1/BuildWindows.txt).

**32 clips procesados comprobados mediante GetData en Unity:** datos presentes y margen de pico conservado bajo 0,5, sin modificar importadores ni originales. Resultado: [NivelesAudio.txt](AuditoriaCapitulo1/NivelesAudio.txt).

Se ejecutó el `.exe` con Direct3D 11 y Direct3D 12 en la GPU del equipo; ambos inicializaron motor, gráficos y entrada sin excepciones ni referencias faltantes registradas durante el arranque. Esto es una prueba de arranque, no un recorrido completo ni una revisión visual del ejecutable. Resultado resumido sin datos de red: [ArranqueWindows.json](AuditoriaCapitulo1/ArranqueWindows.json).

El ejecutable está en `Builds/AuditoriaCapitulo1Windows/FrecuenciaCero.exe`. Conservar juntos el EXE, `FrecuenciaCero_Data`, DLLs y demás archivos del Build. Es un Build de desarrollo para validar; no se presenta como una entrega final sin la partida manual.

## Pendientes y límites de persistencia

Sin sustituirlos por recursos genéricos, quedan pendientes los cinco sonidos indicados:

1. Escarbador.
2. Pasos específicos del puente.
3. Motor de camioneta más realista.
4. Encendido y apagado de linterna.
5. Apertura y cierre de puertas.

Se conservaron los efectos ya existentes, incluidos los provisionales anteriores de eventos narrativos. Los sonidos pendientes no se presentan como errores de compilación o requisitos para terminar el capítulo.

**La persistencia es de sesión.** Cambiar de escena y reintentar conservan progreso e inventario; cerrar el proceso no guarda una partida completa. El marcador de capítulo I completado y las preferencias sí usan PlayerPrefs. No se añadió un sistema de guardado ni un botón de reanudación al menú. Evitar cerrar el ejecutable a mitad del recorrido esperando recuperarlo después.

## Comprobación personal antes de la exposición

1. Jugar una partida completa en el ejecutable, desde Nueva partida hasta CONTINUAR, caminando todos los senderos sin las reubicaciones del arnés. Revisar obstáculos, puente, reja, puertas y salida del claro.
2. Salir y volver al retén/generador; comprobar objetivo, inventario, linterna, suministros recogidos y que el accidente no se repita.
3. Probar caminar/correr, detenerse para evadir, lanzar una botella, recibir daño, curarse con H y reintentar al morir.
4. Revisar el cierre y CONTINUAR tanto con Enter como con clic; comprobar que el capítulo II devuelve WASD y la linterna.
5. Escuchar con auriculares pasos, radio, llegada y río localizado; comprobar volumen, silencios y ausencia de música de exploración.
6. Revisar textos y HUD en 16:9 y 4:3, pantalla completa/ventana, cambio de foco, equipo/proyector de la exposición y duración de una partida completa. Escape no abre una pausa.

## Archivos de esta tarea

- Modificado: `Assets/Scripts/World/RootsEncounter.cs`.
- Nuevos: este informe y los resultados pequeños en `Docs/AuditoriaCapitulo1/`.
- Herramientas de prueba: `Tools/AuditoriaCapitulo1/ChapterAuditEditor.cs` y `ChapterAuditRuntime.cs`, fuera de Assets; no forman parte del gameplay ni de las escenas.
- Registros completos y copia aislada: `Temp/AuditoriaCapitulo1/` y `Temp/AuditoriaCapitulo1Unity/`; no se deben publicar los logs completos del editor.

No se hicieron commits, staging ni pushes.
