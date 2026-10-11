# Frecuencia Cero — Pulido de escala visual y HUD

## Cambios

El HUD existente utiliza IMGUI (`BatteryUI` y `NarrativeUI`), no Canvas, Canvas Scaler ni TextMeshPro. Antes, sus coordenadas se escalaban desde 640×360: a 1280×720 un texto de tamaño 14 se dibujaba con tamaño efectivo 28, y los paneles ocupaban 470×144 y 696×144 píxeles. Se corrigió la referencia de escala del HUD, además de compactar los paneles.

`HudLayout` comparte una referencia de **1280×720** únicamente para batería, linterna, objetivos y avisos de misión. Los diálogos, documentos, mapa y mensajes de interacción conservan sus estilos y referencia previa de **640×360**. No se cambió ningún Canvas ni la configuración global del proyecto.

Medidas configuradas a 1280×720, con escala HUD 1:

| Elemento | Configuración nueva |
| --- | --- |
| Panel batería/linterna | 208×62 px, esquina superior izquierda |
| Margen exterior | 16 px |
| Batería y porcentaje | Fuente 13; porcentaje alineado a la derecha |
| Barra de carga | 184×5 px |
| Indicador F | Fuente 11, recuadro discreto de 18×16 px |
| Texto de linterna | Fuente 12: «apagar linterna» / «encender linterna» |
| Objetivo | Panel de 320 px de ancho, esquina superior derecha; fuente 14 |
| Encabezado/aviso de objetivo | Fuente 11; conserva su duración de 3 segundos |

El panel del objetivo calcula su altura según el texto y permite varias líneas, con un mínimo de 52 px. No se abre un panel adicional al actualizar la misión: cambia temporalmente el encabezado a «OBJETIVO ACTUALIZADO». Los colores siguen siendo oscuros y verde grisáceo. En ventanas estrechas el objetivo se coloca debajo del panel de batería para evitar que ambos se solapen. La escala mínima de 0.75 mantiene algo de legibilidad en previews pequeños; para valorar el tamaño final, usa Game a 1280×720.

Leo utiliza un mismo atlas de 16 sprites en las dos escenas. Su importación pasa de **256 a 200 Pixels Per Unit**. Al dividir los mismos píxeles por 200, el tamaño visual crece uniformemente **256/200 = 1.28**, es decir, **28 %** tanto en anchura como en altura, dentro del intervalo solicitado. El frame inicial pasa de aproximadamente 0.395×0.762 a 0.505×0.975 unidades. No se reescaló ni redibujó la textura.

El Transform de Leo conserva `(1,1,1)`. El aumento afecta solo al tamaño del sprite importado: Rigidbody2D, CapsuleCollider2D de `(0.42,0.25)` con offset `(0,0.1)`, velocidad, pivotes de pies, YSort y todas las referencias de animación permanecen iguales. La linterna conserva su origen local `(0,0.2,0)`, apuntado y alcance; no es hija de un Transform agrandado.

Las cámaras tampoco se ajustaron: el exterior mantiene tamaño ortográfico 5.625 y el interior 2.8125, con sus respectivas configuraciones Pixel Perfect originales. Point, ausencia de mipmaps y ausencia de compresión se conservan en el atlas. La nitidez aparente y el muestreo final deben revisarse en Game: el filtrado Point no implica que todos los píxeles de los distintos assets compartan una escala entera.

## Archivos

Modificados:

- `Assets/Scripts/UI/BatteryUI.cs`
- `Assets/Scripts/UI/NarrativeUI.cs`
- `Assets/Art/Nivel01/Characters/Leo_Exterior.png.meta`

Creados:

- `Assets/Scripts/UI/HudLayout.cs` y su `.meta`.
- Esta guía.
- Copias previas de los dos scripts, la importación del atlas y ambas escenas en `Docs/Backups/PulidoEscalaHUD/`.
- Archivos auxiliares de compilación, hashes y verificación en `Temp/FrecuenciaCeroPulidoHUD/`.

Los archivos de ambas escenas permanecen idénticos. Los cambios se aplican a ellas mediante los scripts y el atlas que ya comparten; no hace falta añadir componentes o reconstruir las escenas.

## Cómo probar en Unity

1. Vuelve a Unity, espera la importación del atlas y la compilación de scripts. Si hace falta, usa `Assets > Refresh`. No cambies manualmente el Transform ni el collider de Leo.
2. En Project, selecciona `Assets/Art/Nivel01/Characters/Leo_Exterior.png`. En Inspector debe figurar **Pixels Per Unit: 200**, **Filter Mode: Point (no filter)** y las 16 subdivisiones originales. La importación ya está guardada; no hace falta ajustar nada.
3. Abre `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`. En Game, abre el selector de resolución y elige **1280×720**. Si no existe, pulsa **+**, elige **Fixed Resolution**, introduce Width 1280 / Height 720 y guárdala. Evalúa el tamaño con Game suficientemente grande y su control Scale a 1× cuando la pantalla lo permita.
4. Abre `Window > General > Console`, comprueba que no haya errores y presiona Play. Observa batería/F arriba a la izquierda y objetivo arriba a la derecha. Comprueba la identificación de Leo junto a camioneta, árboles y puerta. El zoom del bosque debe conservarse.
5. Prueba WASD, diagonales, mouse/F y descarga de batería. Recorre el puente, intenta entrar al agua y verifica las colisiones. Observa la niebla y los faroles.
6. Entra con E al retén. Comprueba que Leo mantiene el aumento visual dentro, que puede rodear los muebles y que la puerta funciona sin cambiar la física.
7. Abre radio, bitácora y mapa: sus paneles y textos deben conservar el tamaño anterior. Comprueba que los mensajes contextuales no quedan tapados por el HUD. Completa una actualización de objetivo para revisar su aviso pequeño y el texto largo en varias líneas.
8. Sal y vuelve a entrar. La batería, el estado F y el progreso narrativo deben conservarse con las transiciones normales. No detengas Play entre esas transiciones, porque iniciar una nueva sesión sigue reiniciando la misión como antes.

No se requiere configuración manual adicional de escenas, cámara ni colliders.

## Verificación y límites

La compilación independiente de todos los scripts de `Assets/Scripts` y `Assets/Editor`, usando el compilador y las bibliotecas locales de Unity 6000.6.3f1, terminó **sin errores ni advertencias**.

La comparación de archivos confirma que solo se modificaron los dos scripts de UI y el PPU del atlas. Ambas escenas, textura PNG, pivotes, IDs, animaciones, físicas, luces, cámara, batería, niebla, interacciones y datos narrativos permanecen intactos. Los métodos que dibujan diálogos/documentos, sus estilos y la lógica de aviso de objetivo se compararon con sus copias previas. Los 16 sprites aumentan anchura y altura en la misma proporción de 1.28.

**No se ejecutó Unity Play ni se comprobó el aspecto en el Editor.** Quedan pendientes la importación efectiva, legibilidad en pantalla, proporciones junto a los objetos, origen visible del cono y pruebas jugables. Las medidas indicadas son la configuración del código y la importación; no son una medición de una captura de Game. La compilación independiente no sustituye la compilación/importación dentro del Editor.

Referencia de API consultada para la escala IMGUI: [GUI.matrix](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GUI-matrix.html). La relación entre píxeles y unidades del sprite corresponde a [pixelsPerUnit](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Sprite.Create.html).
