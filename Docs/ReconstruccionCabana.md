# Cierre del capítulo I y vivienda del guardabosques

Implementación en Unity 6000.6.3f1 / URP 2D. Se conserva MainMenu como escena inicial y se utiliza el mismo jugador, sesión de misión, inventario, lector narrativo, interacción E, contornos, cargador y linterna del proyecto.

## Pantalla de cierre

`N1_BosqueAbandonado.unity` utiliza `Assets/Art/ChapterClosing/CierreCapituloI_BosqueLimpio.png`: versión limpia de la imagen adjunta, conservando su composición, bosque, niebla, sendero y criatura distante. Se eliminaron las letras, botón, separadores e icono dibujados. El fondo del menú y su baliza superpuesta ya no se utilizan en esta pantalla.

Se eliminó el encabezado FRECUENCIA CERO del cierre. Permanecen el título EL SENDERO DEL RETÉN, Capítulo I completado, la frase solicitada y CONTINUAR, todos como Text/Button reales de Unity. `ChapterContinueHighlight` utiliza marfil y ocre apagados y una transición suave de selección. El controlador `ChapterOneEnding`, sus tiempos, fundidos, entrada por Enter/ratón y protección contra cargas duplicadas se conservan. El menú y su logo conservan sus archivos originales.

## Distribución

No hay pasillo ni mapa gigante. La sala sirve como centro; los accesos secundarios están dentro de las habitaciones correspondientes.

| Escena en `Assets/Scenes/Nivel02` | Contenido y conexiones |
| --- | --- |
| `N2_CabanaGuardabosques.unity` | Sala/recibidor: estufa, sillón, mesa auxiliar, radio, fotografías, cuadros, biblioteca, alfombra, perchero, botas y leña. Sendero, cocina, comedor y despacho. Sin cama. |
| `N2_CabanaCocina.unity` | Cocina de leña, ollas/sartenes, fregadero, alacena, frascos, platos, preparación, cuchillo decorativo, alimentos y sacos. Sala y baño. |
| `N2_CabanaComedor.unity` | Mesa, mantel, dos tazas utilizadas, platos, vela, cuatro sillas —dos en el propio sprite de mesa y dos independientes—, vajillero, fotografías y alfombra. Sala. |
| `N2_CabanaBano.unity` | Habitación estrecha: bañera/ducha, lavabo, espejo, inodoro, tuberías, toallas, estantería y botiquín de pared. Cocina. |
| `N2_CabanaDespacho.unity` | Escritorio con radio, lámpara, libros, cuadernos y cajones, mapas, fotografías, biblioteca, archivero y equipo de mantenimiento. Sala, dormitorio y almacén. |
| `N2_CabanaDormitorio.unity` | Cama doméstica distinta de la del retén, mesita, armario/ropa, botas, baúl, fotos, cortinas, alfombra y lámpara. Despacho. |
| `N2_CabanaAlmacen.unity` | Estantes, cajas, herramientas, cuerda, bidones, sacos, piezas y madera. Despacho; inicialmente bloqueado. |

## Arte e iluminación

La referencia obligatoria es el interior real del retén: se inspeccionaron sprites, materiales, escalas, colliders y luces, y se guardó una captura previa en `Temp/ReconstruccionCabana/Reten-referencia.png`.

Se reutilizan sus paredes de madera, alfombras, puertas, ventanas nocturnas, escritorio/radio, mapas, silla, estufa, archivero, suministros y herramientas. Los materiales son los mismos del proyecto. La cámara de las habitaciones utiliza el mismo ajuste de píxeles que el retén: assetsPPU 32, referencia 320×180; Leo mantiene su Transform y animaciones.

Los muebles domésticos que faltaban se crearon con el atlas del retén como referencia gráfica. `Assets/Art/Nivel02/Interiores` contiene dos atlas con transparencia, 32 recursos Sprite independientes y una textura de tablones domésticos con juntas alternadas y desgaste. Se utiliza Point filtering, sin mipmaps ni compresión. Los PNG originales del retén y de Leo no se modificaron. No se integraron imágenes de sitios externos con licencia desconocida.

Cada habitación tiene una luz global tenue y al menos una fuente cálida localizada. Sala, cocina y comedor tienen varias zonas de luz; baño y almacén permanecen más oscuros. La linterna utiliza sus controles y batería habituales. Los muebles grandes tienen colliders de huella; las decoraciones pequeñas y cuadros pueden ser sólo visuales.

## Puertas y continuidad

`CabinRoomDoor` se conecta al `PlayerInteraction` y `InteractionOutline` existentes. Muestra E y el destino; al interactuar bloquea brevemente a Leo, utiliza `GeneratorGateFade` y llama al cargador único `RetenTravel`. Las llegadas emparejadas `RetenArrival` devuelven a Leo junto a la puerta correspondiente. Un bloqueo local y el estado de carga previenen transiciones simultáneas.

Vida, baterías, botellas, botiquines y recogidas conservan la misma sesión. Las preferencias de audio y pantalla no se reinician. Cada habitación tiene un checkpoint libre de obstáculos. El objetivo pertenece a la cabaña; `CabinRoomContext` lo inicializa también cuando se abre directamente una habitación desde el editor.

La entrada exterior y el diálogo del desconocido conservan `CabinDoor`, sus tres líneas y su bisagra. La sala mantiene `cabana_umbral` y el regreso al sendero mantiene `sendero_cabana_puerta`.

## Interacciones y almacén

Hay lecturas neutrales para radio/fotografías de la sala, preparación de cocina, las dos tazas, botiquín de pared, escritorio/cajones, mapa y baúl. Se usan `ForestDocument` / `DocumentDefinition` y los cuadros existentes. Los recursos están en `Assets/Narrative/Nivel02/Cabana` y pueden reemplazarse por futuros documentos. No se revela la identidad del desconocido ni los secretos centrales.

La batería `cabana_cocina_bateria_01` y el botiquín portátil `cabana_bano_botiquin_01` utilizan `SupplyPickup` y los IDs persistentes existentes: no reaparecen después de ser recogidos. El botiquín de pared decorativo y el botiquín portátil son objetos diferentes.

La puerta del almacén se puede examinar con E y responde que está cerrada. Su futura habilitación se prepara con `RetenMission.Session.Chapters.UnlockStorage()`. `StorageUnlocked` pertenece al progreso de sesión y se conserva entre escenas y reintentos. No hay llave ficticia, rompecabezas añadido ni desbloqueo automático por visitar habitaciones. Nueva partida crea un progreso nuevo. El almacén ya tiene contenido, colisiones y regreso; su acceso normal espera el evento narrativo posterior.

## Archivos de esta tarea

- Modificados: `N1_BosqueAbandonado.unity`, `N2_CabanaGuardabosques.unity`, `ChapterProgress.cs`, lista del Build y documentación del prólogo.
- Creadas: las seis escenas adicionales, `CabinRoomDoor.cs`, `CabinRoomContext.cs`, `ChapterContinueHighlight.cs` y `Editor/CabinInteriorBuilder.cs`, con metas.
- Recursos: `Art/ChapterClosing`, `Art/Nivel02/Interiores`, `Narrative/Nivel02/Cabana`.
- `CabinInteriorBuilder` es una receta explícita, sin ejecución automática; protege las habitaciones adicionales existentes. No es necesario ejecutarla para jugar.

## Verificación

Las pruebas se ejecutan en `Temp/GateVerificationUnity`, con preferencias aisladas y Unity 6000.6.3f1. Los arneses y capturas permanecen en Temp, fuera de los Assets del proyecto principal. Los informes definitivos y resultados se registran en `Temp/ReconstruccionCabana`.

Resultado: **335 comprobaciones superadas**: 276 de navegación/interiores en `PruebasHabitaciones.txt` y 59 de cierre/prólogo en `PruebasCapitulos.txt`. Se abrieron las 14 escenas habilitadas del Build sin scripts ausentes. No se detectaron errores nuevos de compilación o gameplay; los arneses distinguen las excepciones anteriores de arranque de SearchDatabase / TutorialModalWindow.

El recorrido de habitaciones utiliza WASD y E reales del Input System: verifica puertas, puntos emparejados, contornos, controles recuperados, fundidos eliminados, inventario, carga/encendido de linterna, lectura, recogidas únicas, H y checkpoint. Comprueba que el almacén permanece cerrado, prueba su futura API de progreso y confirma que el estado desbloqueado persiste. Se corrigió el acceso al escritorio antes de la prueba definitiva. No se desbloquea el almacén en una partida normal por estas pruebas, realizadas en una sesión aislada.

La regresión del prólogo vuelve a comprobar la secuencia de unos 18 segundos, el fondo nuevo, ausencia de encabezado/baliza del menú, Enter y clic reales, carga única, radio, apagado de la ventana, tres líneas del desconocido, bisagra, entrada a la sala y regreso al bosque. Mantiene vida, inventario y linterna.

Se revisaron las catorce capturas de habitaciones en 16:9 y 4:3, además del cierre y las interfaces reales. Los registros definitivos son `PruebasHabitaciones-Final.log` y `PruebasCierre-Prologo.log`. `Comparacion.json` verifica mediante SHA-256 que **304 archivos** de scripts/metas, escenas, recursos nuevos, receta y lista del Build coinciden con la copia probada. Contra la auditoría inicial no hay archivos eliminados; menú, logo, introducción, retén, generador, sendero exterior del capítulo II, Escarbador, cámaras anteriores y controles conservan sus archivos previos.

Prueba manual recomendada: iniciar desde MainMenu, escapar del claro, pulsar CONTINUAR, avanzar por el sendero, escuchar/leer al desconocido y entrar. Explorar sala → cocina → baño; sala → comedor; sala → despacho → dormitorio. Examinar la puerta cerrada del almacén. Volver por cada acceso, revisar el HUD y probar el Build de Windows en las resoluciones habituales.

Pendiente de revisión humana: dirección artística y ritmo en el equipo habitual, audio de voces definitivo ya previsto en el prólogo y Build final de Windows. El evento narrativo que habilitará el almacén y los documentos definitivos pertenecen al desarrollo posterior; las habitaciones ya son escenarios funcionales.

Los prompts y el origen de los recursos generados se registran en [RecursosCabanaPrompts.md](RecursosCabanaPrompts.md). Se utilizó la herramienta integrada imagegen. No se realizaron commits ni pushes.
