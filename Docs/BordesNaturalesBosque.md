# Límites naturales del exterior del retén

## Inspección y alcance

Se trabajó sobre `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`, sin reconstruirla. Antes de editar se guardó una copia en `Docs/Backups/BordesBosque/N1_ExteriorReten.unity`.

El suelo original cubría x = [-21, 21], y = [-14, 14]. Los cuatro colliders perimetrales cierran ese mismo rectángulo. La cámara usaba SmoothCameraFollow sin confinamiento; a 1280×720, Pixel Perfect muestra aproximadamente 20×11,25 unidades. Al acercarse Leo a un borde, la cámara podía mostrar unos diez metros de terreno ausente a izquierda o derecha y unos cinco y medio arriba o abajo. La vegetación existente no cubría esa franja exterior. No se encontró Cinemachine Confiner 2D.

La conexión con `N1_Generador` ya estaba implementada. Se conservaron tanto la interacción como su señal, sendero y punto de regreso; no se creó otra transición.

## Cambios

- **Suelo continuo:** un nuevo sprite de 80×60 unidades, x = [-40, 40], y = [-30, 30], detrás de los Tilemaps existentes. Usa el mismo campo procedural y la misma cuadrícula mundial de píxeles. El color coincide exactamente dentro del terreno original; hacia el bosque distante se oscurece gradualmente. No se estiran ni repiten tiles grandes y sus bordes quedan fuera de la vista confinada.
- **Bosque alrededor del perímetro:** 216 árboles de tres siluetas, tamaños y orientaciones diferentes, distribuidos con separación irregular en tres franjas superpuestas. Incluye doce árboles adicionales en grupos del lado derecho. Se reutilizó el atlas actual; no se sustituyeron sus árboles ni otros props.
- **Vegetación baja:** 32 arbustos, 11 rocas y 11 grupos de raíces, entre las capas de árboles. Los senderos, el arroyo y los claros principales se excluyen de la colocación. La vegetación intermedia y lejana usa tintes discretamente más oscuros para dar profundidad junto con la niebla existente. Esta oscuridad es una variación de color; no se añadió un sistema nuevo de sombras dinámicas.
- **Colisiones:** 40 BoxCollider2D pequeños en troncos, matorrales y rocas del borde accesible. Se dejó espacio alrededor de obstáculos existentes. Los cuatro límites invisibles originales permanecen detrás de la vegetación; no se añadieron muros grandes ni se tocaron los polígonos del río o las barandas del puente.
- **Cámara:** `ForestCameraBounds` añadido solamente a Main Camera del exterior. Limita su centro considerando el tamaño de vista que calculará Pixel Perfect, incluso al cambiar la resolución. El área decorada de confinamiento es x = [-35, 35], y = [-23, 23], con margen de 0,125 unidades para el redondeo de píxeles. Corre después del seguimiento suave y antes del apuntado de la linterna. No cambia Orthographic Size, Pixel Perfect, zoom ni el script de seguimiento existente.

El grupo nuevo en Hierarchy es **Bosque_Limites_Naturales**, con tres subgrupos de vegetación y el suelo extendido. Los elementos del borde que interactúan visualmente con Leo conservan YSort. La vegetación lejana, que permanece inmóvil e inaccesible, usa el orden de renderizado precalculado desde la misma base vertical; no necesita ejecutar YSort cada frame. Si se mueve manualmente un árbol distante, también debe actualizarse su Sorting Order a aproximadamente `-posiciónY × 32`, o añadirse el YSort existente.

La niebla, iluminación, movimiento, personaje, HUD, TAB, linterna, batería, radio, misión e interior no se modificaron. El terreno y la vegetación añadidos están serializados: guardar la escena o reconstruir únicamente sus Tilemaps con el menú existente conserva estos objetos. No hay generación de bosque durante Play.

## Archivos

Modificado:

- `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`.

Creados, con sus `.meta`:

- `Assets/Scripts/Camera/ForestCameraBounds.cs`.
- `Assets/Scripts/Camera/ForestViewGeometry.cs`.
- `Assets/Scripts/World/ForestBorderField.cs`.
- `Assets/Editor/ForestBorderGroundBaker.cs`.
- `Assets/Art/Nivel01/Exterior/SueloBosqueExtendido.png`.

Documentación y comprobaciones:

- `Docs/BordesNaturalesBosque.md`.
- `Docs/Backups/BordesBosque/N1_ExteriorReten.unity`.
- `Docs/Validation/BordesBosque/Verificar.ps1` y `EstadoAnterior.json`.
- Archivos temporales de preparación/compilación en `Temp/FrecuenciaCeroBordes`.

## Prueba en Unity

1. Detén Play. Vuelve al Editor y espera la importación y compilación. Si Unity avisa de cambios externos de escena, elige **Reload**; respalda primero cualquier edición manual sin guardar. Si los archivos nuevos no aparecen, ejecuta **Assets > Refresh**.
2. En Project, abre `Assets > Scenes > Nivel01 > N1_ExteriorReten` con doble clic. No necesitas reconstruir la escena ni asignar referencias. En Hierarchy deben aparecer **Bosque_Limites_Naturales** y el componente **Forest Camera Bounds** en Main Camera.
3. Abre **Window > General > Console**. Selecciona 1280×720 en Game, pulsa Play y haz clic dentro de Game para darle foco.
4. Desde la camioneta, recorre el camino original hasta el puente y el retén. Comprueba WASD, el paso del puente, las colisiones del río, la puerta y el acceso existente al generador. Entra y sal del retén para comprobar los puntos de regreso.
5. Recorre el flanco derecho, hacia x ≈ 18–20, tanto por debajo como por encima del retén. Debe continuar la vegetación detrás de los árboles cercanos; empuja contra troncos y matorrales y comprueba que no se atraviesan ni se sale del perímetro. El acceso al generador, situado más adentro, debe seguir libre.
6. Repite alrededor del oeste, norte y sur: cerca de x ≈ -20, y ≈ 13 y y ≈ -13. Muévete en ambas direcciones siguiendo el borde y observa las esquinas. Busca cortes del suelo, claros excesivamente vacíos, superposiciones incómodas y atascos. Leo no debe superar x = ±21 o y = ±14.
7. Prueba otra resolución, por ejemplo 1920×1080 y, si está disponible, 2560×1080. Comprueba que el seguimiento sigue siendo suave y que el marco de cámara permanece sobre suelo decorado. F, el apuntado, la niebla y TAB deben seguir funcionando.
8. Para medir el rendimiento real, abre **Window > Analysis > Profiler** durante Play y revisa CPU, GPU y Rendering cerca del borde derecho. Compara el mismo punto alternando temporalmente el grupo **Bosque_Limites_Naturales** y vuelve a activarlo al terminar. Esta comparación requiere Unity: los recuentos de objetos no garantizan un tiempo de frame concreto.

El menú nuevo **Frecuencia Cero > Exterior Reten > Regenerar suelo de bordes** sirve solo para volver a hornear la textura si posteriormente se cambia su campo procedural. No es necesario ejecutarlo para esta entrega; no reconstruye vegetación ni escenas.

## Validación y límites

- Compilación de todos los C# de Scripts y Editor con Roslyn del SDK de Unity 6000.6.3f1 y las referencias locales de Unity/URP/Input System: **salida 0, sin errores ni advertencias**.
- Referencias y jerarquía de la escena comprobadas. Todos sus documentos originales permanecen iguales excepto la lista de componentes de Main Camera, a la que se añadió el confinamiento, y SceneRoots, al que se añadió el grupo del bosque.
- Comparación SHA256: de los assets, paquetes y ajustes preexistentes, solo cambió la escena exterior. No se eliminaron archivos ni se modificó ninguna otra escena o mecánica.
- Comprobado el modelo de vista a 640×360, 1280×720, 1920×1080, 1040×585, 2560×1080, 3440×1440, 3840×1080 y 720×1280. Se mantienen las 20×11,25 unidades de vista a 1280×720; los confines contemplan el redondeo del render target de Pixel Perfect sin modificarlo. Ventanas extremas cuya vista sea mayor que toda el área decorada no pueden confinarse sin recortar o cambiar el zoom; esos formatos no se validaron.
- Comprobadas rutas estáticas, contando los colliders serializados y ambos polígonos del río: 755 celdas caminables del sendero original conservan su conexión. Se mantienen accesibles camioneta, retén, puente, entrada/regreso del generador, y no aparece una salida por el perímetro.
- La textura nueva es Point, 32 PPU, sin mipmaps ni compresión, y su tamaño máximo de importación es 4096 para conservar los 2560×1920 píxeles. Se comprobó que los píxeles del suelo original y su prolongación coinciden.
- Presupuesto añadido: 271 SpriteRenderer y 40 colliders pequeños; solo 40 elementos de la vegetación añadida usan YSort en tiempo de ejecución. Se reutilizan materiales, sprites y luces existentes, sin nuevas partículas, luces, texturas generadas por frame ni gestores persistentes. La nueva textura RGBA ocupa aproximadamente 18,75 MiB sin compresión en GPU; el resto del arte ya existía.
- **No se ejecutó Unity Play ni se comprobó la composición final dentro del Editor o el rendimiento GPU.** Se inspeccionó el PNG del suelo, pero las pruebas externas de geometría no sustituyen las físicas y la revisión visual real. Confirma el resultado siguiendo los pasos anteriores.

Para repetir las comprobaciones externas desde la raíz del proyecto: `powershell -NoProfile -ExecutionPolicy Bypass -File Docs/Validation/BordesBosque/Verificar.ps1`. Requiere PowerShell/.NET y corresponde al estado de esta entrega; cambios posteriores pueden alterar las comparaciones.
