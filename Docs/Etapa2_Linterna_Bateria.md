# Frecuencia Cero — Etapa 2

Se inspeccionaron los scripts de movimiento, cámara y YSort, la escena, los materiales, Renderer2D y la entrada antes de modificar el proyecto. Se conserva Unity 6000.6.3f1, URP 17.6.0 e Input System. No se cambian Packages, ProjectSettings, el asset de acciones existente ni el listado de escenas de compilación.

## Archivos creados y modificados

Modificado:
- `Assets/Scenes/Nivel01_Bosque.unity`: materiales Sprite-Lit en 133 SpriteRenderer, luz global nocturna, objeto Linterna hijo de Leo e Interfaz_Bateria como objeto de escena. Se conservan los 550 objetos/componentes serializados anteriores.

Creados, con sus `.meta` correspondientes dentro de Assets:
- `Assets/Scripts/Lighting/FlashlightController.cs`: F, estado manual, posición del mouse y giro suave.
- `Assets/Scripts/Lighting/FlashlightBattery.cs`: descarga, intensidad y alcance según la carga, mínimos configurables y método AddCharge para una incorporación futura de recarga.
- `Assets/Scripts/Lighting/BatteryCharge.cs`: aritmética de descarga e interpolación, separada de Unity para poder verificarla sin ejecutar el Editor.
- `Assets/Scripts/UI/BatteryUI.cs`: porcentaje y barra provisional en pantalla mediante IMGUI (OnGUI); no necesita Canvas, EventSystem ni paquetes nuevos.
- `Assets/Editor/BosqueNightLightingSetup.cs`: menú opcional para configurar la etapa mediante las APIs del Editor, guardar una copia previa y evitar duplicar Linterna o la interfaz.
- `Assets/Art/Placeholders/ProvisionalLit.mat`: material Sprite-Lit-Default de URP 2D.
- Carpetas Lighting y UI con sus `.meta`.
- `Docs/Backups/Nivel01_Bosque.pre-etapa2.84e3b66746314565866e92430d4d2d3b.unity.txt`: copia creada antes de modificar la escena.
- Este documento.

Los archivos de preparación, compilación y comprobación quedan en `Temp/FrecuenciaCeroEtapa2`; no forman parte del juego. No se modificaron LeoMovement, SmoothCameraFollow, YSort, los sprites PNG ni sus ajustes de importación. El material ProvisionalUnlit anterior también se conserva.

## Comportamiento y configuración

La linterna empieza encendida al 100 %, consume únicamente mientras está encendida y puede alternarse con F incluso al 0 %. El alcance y la intensidad varían continuamente con la carga. No hay apagado automático al agotarse; se mantienen los valores mínimos.

Valores guardados en la escena:

| Carga | Intensidad | Alcance exterior en unidades |
| --- | --- | --- |
| 100 % | 1.60 | 5.50 |
| 50 % | 0.90 | 3.20 |
| 20 % | 0.48 | 1.82 |
| 0 % | 0.20 | 0.90 |

- `Leo > Linterna > Flashlight Battery`: Battery Percent 100; Full Battery Duration 120 segundos; Minimum/Maximum Intensity 0.2/1.6; Minimum/Maximum Range 0.9/5.5; Inner Radius Fraction 0.35.
- `Flashlight Controller`: Starts On activado; Aim Smooth Time 0.08 segundos; World Camera conectada a Main Camera.
- Light2D: tipo Point, ángulo interior 34°, exterior 64°, color cálido (1, 0.93, 0.78), blend style 0 y Default Sorting Layer. Sombras desactivadas.
- Global Light 2D: intensidad 0.45 y color azulado (0.64, 0.72, 0.86), para sugerir la madrugada sin dejar todo negro. No se añadió reloj ni ciclo horario.

El material iluminable es necesario porque Sprite-Unlit ignora las luces 2D. Se asignó ProvisionalLit a los sprites que compartían el material provisional anterior. No se alteraron escala, pivotes, ordenación ni colisiones.

El mouse se lee con Input System. Su posición se proyecta sobre el plano XY mediante la cámara existente, después de su seguimiento en LateUpdate. La rotación solo afecta a Linterna; no gira el Rigidbody2D ni el collider de Leo. La batería actualiza después de procesar F.

La interfaz se dibuja en coordenadas de pantalla y permanece fija al desplazarse la cámara. El porcentaje redondea hacia arriba para mostrar 0 % únicamente cuando la carga realmente llega a cero. La barra usa la carga continua sin redondear. Es una interfaz provisional, no una interfaz definitiva de pixel art.

## Pasos exactos para probar en Unity

1. Detén Play si está activo. Vuelve a Unity y espera la importación. Usa **Assets > Refresh** si los archivos nuevos no aparecen.
2. Si Nivel01_Bosque estaba abierto antes de esta intervención, abre SampleScene y después vuelve a abrir Nivel01_Bosque para cargar la versión actual del disco. Evita guardar una versión antigua cargada en memoria encima del archivo actualizado. Si tienes cambios propios sin guardar, guárdalos en otra escena antes de recargar.
3. En **Project > Assets > Scenes**, haz doble clic en **Nivel01_Bosque**.
4. En **Hierarchy**, despliega **Prototipo_ElSenderoDelReten > Leo > Linterna**. Confirma que tiene Light2D, Flashlight Controller y Flashlight Battery. Confirma también que existe **Interfaz_Bateria**.
5. En **Window > General > Console**, verifica que no haya errores rojos ni Missing Script. La comprobación independiente realizada no sustituye la importación del Editor.
6. Para la prueba rápida, selecciona **Linterna** y, en **Inspector > Flashlight Battery**, establece **Full Battery Duration = 10**. Mantén **Battery Percent = 100**. Ajusta la duración en este componente; las propiedades de intensidad/radio del Light2D se actualizan desde la batería durante Play.
7. En **Game**, selecciona 640×360, 1280×720 o 1920×1080. Pulsa **Play** y haz clic dentro de Game.
8. Mueve el mouse alrededor de Leo: el cono debe apuntar hacia él con un pequeño retraso suave. Prueba especialmente arriba, abajo, izquierda y derecha para comprobar que la orientación coincida.
9. Usa WASD y diagonales. La cámara debe seguir a Leo y la luz debe permanecer junto a él. Prueba también el contacto con rocas y troncos para comprobar que la etapa anterior sigue funcionando.
10. Observa el porcentaje y la barra mientras la luz está encendida. Con duración de 10 segundos, debería pasar por 50 % aproximadamente a los 5 segundos, 20 % a los 8 y 0 % a los 10. Compara el cono en esos puntos.
11. Antes de llegar a cero, pulsa **F**. La luz debe desaparecer y el porcentaje/barra deben permanecer estables durante varios segundos. Pulsa F otra vez: la descarga debe continuar desde esa carga.
12. Al llegar al 0 %, comprueba que siga existiendo una luz corta y tenue. En el Inspector durante Play, **Light2D** debe seguir habilitado, con Intensity 0.2 y Outer Radius 0.9. Pulsa F dos veces: debe apagarse y volver a encenderse con esa luz residual, manteniendo 0 %.
13. Para comprobar valores concretos rápidamente, puedes cambiar **Battery Percent** a 100, 50, 20 o 0 durante Play. Son ajustes temporales de prueba en el Inspector, no una mecánica de recarga del juego.
14. Detén Play. Restablece **Full Battery Duration = 120** en modo edición si cambiaste la duración antes de Play y no quieres conservar la prueba rápida. Guarda la escena con **Ctrl+S** después de tus ajustes definitivos.

No es necesaria configuración manual adicional de entrada, materiales, referencias, Canvas o luces. El menú **Frecuencia Cero > Configurar iluminacion de Etapa 2** es opcional: la escena ya está configurada en disco. Si lo usas, exige guardar tus cambios antes y crea otra copia de seguridad; no hace falta ejecutarlo para la prueba normal.

Sin sombras, la luz puede iluminar sprites al otro lado de obstáculos. No hay oclusión por árboles en esta etapa. El seguimiento suave de Transform se combina con la cuantización visual propia de Pixel Perfect.

## Verificación realizada y límites

- Compilación de todos los scripts runtime y Editor mediante Roslyn de la instalación Unity 6000.6.3f1 y sus bibliotecas/paquetes: terminó sin errores ni advertencias.
- Trece comprobaciones ejecutadas sobre BatteryCharge: consumo, ausencia de consumo apagada, llegada a cero, carga nunca negativa, intensidad y alcance residuales, valores intermedios, independencia del reparto de tiempo entre fotogramas y degradación monótona.
- Revisión de escena: IDs únicos y referencias locales presentes. Se compararon los 550 bloques originales con la copia de seguridad: fuera de la luz global, hijos de Leo y lista de raíces, solo cambiaron las referencias al material de 133 SpriteRenderer. No se eliminaron ni reconstruyeron los componentes anteriores.

No se ejecutó Unity Play desde estas herramientas. No se verificó visualmente el brillo final, el cono, su orientación efectiva, F, la UI o la importación de la escena en el Editor. Tampoco se repitió la prueba física de movimiento/colisiones en Unity tras este cambio. Las comprobaciones de aritmética y compilación no prueban esas funcionalidades de ejecución. Los pasos anteriores permiten comprobarlas en tu Editor.

Esta etapa no incorpora enemigos, detección por luz, inventario, puzles, otros niveles, baterías coleccionables, sombras complejas, volumetría ni postprocesamiento. La próxima etapa queda pendiente de tu confirmación.
