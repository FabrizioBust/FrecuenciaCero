# Etapa 3C — Entrada al retén e interior

## Alcance y resultado

Se creó `Assets/Scenes/Nivel01/N1_InteriorReten.unity`. No existía otra escena interior ni `InteriorReten_Referencia.png`. La solicitud recibida termina en «Iluminación secundaria de la estufa,»; se implementaron los requisitos disponibles, incluida la salida de regreso al exterior.

En el exterior, el objeto existente `Exterior_Reten > Zona_D_Reten > Puerta_Futura_E` conserva su marcador `FutureRetenEntrance` y su trigger. Ahora tiene también `RetenDoor`, conectado a Leo. La interacción está en (10.2, 3.7), delante de los escalones y fuera del collider de la cabaña. No se cambió la geometría ni el terreno exterior.

La puerta muestra **E: entrar al retén** dentro de 1.05 unidades del centro del collider de Leo, con una franja de aproximación de 0.8 unidades por lado. Comprueba el lado frontal y una línea de visión filtrada para descartar obstáculos sólidos; ignora el propio Leo y los triggers. Solo escucha `<Keyboard>/e` y un flanco de pulsación. Requiere soltar E después de cargar una escena, evitando entradas/salidas repetidas al mantenerla pulsada.

El interior muestra **E: salir del retén** junto a la puerta inferior. La aparición interior está en (0, -2.7), despejada. Al volver al exterior, Leo aparece en (10.2, 3.05) delante de la puerta, sin regresar a la camioneta. La carga sustituye una escena por otra, por lo que no se acumulan cámaras, jugadores ni HUD.

`RetenTravel` conserva la carga de batería y el estado de encendido durante estos viajes. `RetenArrival` restaura esos valores y coloca a Leo en el punto de llegada correspondiente. Abrir directamente una escena en Play sigue usando su posición inicial y batería inicial. Esto no añade un sistema de guardado entre sesiones.

## Interior y arte

El interior ocupa aproximadamente 11 × 8.6 unidades. Está organizado bajo `Interior_Reten`, con grupos `Arquitectura`, `Muebles`, `Detalles_Ambientales`, `Iluminacion_Interior` y `Puerta_y_Aparicion`.

- Arquitectura: tablones envejecidos, paredes y vigas de madera con humedad, ventana nocturna y puerta inferior. Suelo y tramos de pared usan SpriteRenderer Tiled; los muebles se escalan uniformemente.
- Superior izquierda: armario verde oxidado, estantería con suministros y caja de herramientas provisional reutilizada.
- Superior central: escritorio con radio, bitácora, papeles, taza y lámpara; mapa de montaña y calendario en la pared. El mapa es ilustrativo, no un plano geográfico definitivo ni un puzle.
- Superior derecha: cama individual con manta verde y almohada; mochila con herramientas.
- Laterales: silla, otra estantería, cajas, estufa y barril.
- Centro: alfombra rojiza desgastada, mesa con papeles y taza, circulación alrededor.
- Ambiente: documentos esparcidos, desgaste, polvo, manchas y óxido contenidos en los sprites.

Se generó `Assets/Art/Nivel01/Interior/InteriorReten_Atlas.png` con la herramienta integrada image_gen y se comprobó la existencia de alfa cero en los espacios y alfa prácticamente opaco en los objetos. El atlas es de 1536 × 1024 y se importa como 16 sprites individuales. Se ajustaron los cortes a las siluetas reales: no se usa una imagen conceptual completa como fondo.

Importación: **64 PPU, Point, sin mipmaps ni compresión, Full Rect**. Los muebles tienen pivotes de base para YSort; alfombra, papeles y suelo usan pivotes centrales. Las cajas y el barril reutilizan el atlas exterior. La caja superior representa temporalmente una caja de herramientas: no se generó una caja metálica aislada adicional. Radio, bitácora, documentos y lámpara forman parte del sprite del escritorio y no son interactivos.

El prompt exacto y la procedencia del recurso están en `Docs/Etapa3C_PromptAtlas.txt`.

## Sistemas conservados y configuraciones

El interior reutiliza los mismos componentes de movimiento, Rigidbody2D, cápsula de pies, animación de Leo, YSort, seguimiento de cámara, linterna y HUD que el exterior. WASD, F y el apuntado con mouse mantienen sus controles. No se modificaron esos scripts; `FlashlightBattery` recibe únicamente un método para restaurar la carga al viajar.

Los muebles grandes y las cuatro paredes tienen BoxCollider2D no trigger. Los colliders de los muebles cubren sus bases físicas; la parte alta de los sprites puede superponerse a Leo según YSort. La pared inferior bloquea el límite de la habitación; la salida se activa con E desde el interior, sin necesidad de atravesar el collider de la pared.

Solo en la cámara interior se usa referencia Pixel Perfect de **320 × 180**, conservando 32 Assets PPU. Esto aproxima la vista a una habitación pequeña. Crop Frame Windowbox conserva la relación de aspecto; puede añadir márgenes negros según la ventana Game. `InteriorCameraBounds` limita el seguimiento después de `SmoothCameraFollow` y antes del apuntado de la linterna. La cámara exterior no cambia.

Iluminación interior: Global Light 2D de intensidad 0.48, lámpara cálida del escritorio de intensidad 1.25/radio 4 y luz de la estufa de intensidad 0.8/radio 2.7. Se reutiliza el material Sprite-Lit existente de URP. La iluminación y las sombras pintadas de los sprites requieren revisión visual en el Editor.

Se añadieron exterior e interior a `ProjectSettings/EditorBuildSettings.asset` porque `SceneManager.LoadSceneAsync` necesita escenas disponibles en la lista de compilación. Se conserva SampleScene y el asset de entrada existente. No se alteran los bindings, paquetes, URP ni otros ProjectSettings.

## Ejecutar y probar en Unity 6000.6.3f1

1. Detén Play y deja terminar la importación. Si es necesario, pulsa **Assets > Refresh**. Comprueba que Console no muestre errores.
2. Abre **Assets > Scenes > Nivel01 > N1_ExteriorReten**. Si estaba abierta durante la edición externa, vuelve a abrirla y carga la versión actualizada del disco.
3. Comprueba **File > Build Profiles > Scene List**: `N1_ExteriorReten` y `N1_InteriorReten` deben estar habilitadas. Ya se añadieron a la lista compartida. Si tu perfil usa **Override Global Scene List**, agrega estas dos escenas también a esa lista particular; no se modificó ningún perfil personalizado.
4. Pulsa **Play** y haz clic en Game. Camina con WASD hasta la cabaña. En sus escalones debe aparecer **E: entrar al retén**. Al alejarte, debe desaparecer.
5. Prueba E estando lejos y desde un lateral fuera de la franja frontal: no debe cargar el interior. Acércate a la puerta y pulsa E una vez. Leo debe aparecer en la zona inferior del interior.
6. Mantén E pulsada durante la carga: no debe salir automáticamente. Suelta E antes de volver a interactuar.
7. Explora alrededor de la mesa, entre los muebles y por los laterales. Prueba las paredes, escritorio, armario, cama, estufa y estanterías: Leo debe detenerse en sus bases.
8. Comprueba la cámara en Game: vista ortográfica, seguimiento limitado a la habitación, sprites nítidos. En Scene puedes activar Gizmos y seleccionar colliders para revisar sus contornos.
9. Prueba F y el mouse. Anota el porcentaje de batería y apaga la linterna antes de salir: la carga debe conservarse y la linterna seguir apagada. Con la linterna encendida, la batería continuará consumiéndose durante el tiempo de juego.
10. Acércate a la puerta inferior. Pulsa E cuando aparezca **E: salir del retén**. Debes volver delante de los escalones exteriores. Repite el viaje y confirma que sigue existiendo un único Leo, una cámara principal y un HUD.
11. Para inspeccionar el interior directamente, detén Play y abre **Assets > Scenes > Nivel01 > N1_InteriorReten**. La escena está serializada completa; no requiere un menú de construcción ni colocar referencias a mano. Guarda cualquier ajuste artístico posterior normalmente con **Ctrl+S**.

## Archivos creados o modificados

Modificados:

- `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`: componente de interacción y punto de regreso.
- `Assets/Scripts/Lighting/FlashlightBattery.cs`: método `RestoreCharge`.
- `ProjectSettings/EditorBuildSettings.asset`: dos entradas nuevas para la carga de escenas.

Creados:

- `Assets/Scenes/Nivel01/N1_InteriorReten.unity` y `.meta`.
- `Assets/Scripts/World/RetenDoor.cs` y `.meta`.
- `Assets/Scripts/World/RetenTravel.cs` y `.meta`.
- `Assets/Scripts/World/RetenArrival.cs` y `.meta`.
- `Assets/Scripts/Camera/InteriorCameraBounds.cs` y `.meta`.
- `Assets/Art/Nivel01/Interior/InteriorReten_Atlas.png` y `.meta`.
- `Assets/Art/Nivel01/Interior.meta`.
- `Docs/Etapa3C_InteriorReten.md`.
- `Docs/Etapa3C_PromptAtlas.txt`.

Respaldos previos a la modificación, fuera de Assets:

- `Docs/Backups/Etapa3C/N1_ExteriorReten.unity`.
- `Docs/Backups/Etapa3C/FlashlightBattery.cs`.
- `Docs/Backups/Etapa3C/EditorBuildSettings.asset`.

Herramientas temporales de creación y comprobación en `Temp/FrecuenciaCeroEtapa3C`: `prepare-assets.ps1`, `prepare-scenes.ps1`, `group-interior.ps1`, `atlas-spec.json`, `interior-objects.json`, `compile.rsp`, `Prototype.dll`, `verify.ps1` y `verification.txt`. No son dependencias del juego. No ejecutes otra vez los scripts de aplicación inicial sobre las escenas terminadas; la escena interior ya contiene sus objetos y referencias.

## Verificación y límites

- Compilación de todos los scripts de Assets/Scripts y Assets/Editor mediante el Roslyn y las bibliotecas locales de Unity 6000.6.3f1: sin errores.
- Todas las referencias locales fileID de ambas escenas resuelven a componentes/objetos presentes.
- Se conservan idénticos 624 bloques existentes del exterior. Solo se amplían la lista de componentes de la puerta y la lista de raíces para el punto de regreso. No se elimina ningún componente existente.
- Componentes de Rigidbody2D, cápsula, movimiento, animación, YSort, seguimiento, linterna y HUD del interior coinciden con los del exterior. El método nuevo de batería no cambia el cálculo de consumo.
- Comprobación geométrica conservadora con un rectángulo de 0.42 × 0.25 alrededor de los pies: aparición interior y aproximación a la salida despejadas; 12 destinos de los pasillos conectados al punto de aparición en una cuadrícula de 0.1 unidades (5507 celdas accesibles).
- El punto de regreso exterior no intersecta ninguno de sus BoxCollider2D sólidos.
- Se inspeccionó el atlas y se comprobaron transparencia, 16 cortes y ajustes de importación.

**No se ejecutaron Unity Editor ni Play desde estas herramientas.** El proyecto tiene un archivo de bloqueo de Unity y no hay una interfaz disponible para controlar su instancia. Las comprobaciones independientes no verifican el importador real, el renderizado con URP, las luces, E/F durante Play, la secuencia de carga ni la respuesta dinámica de las colisiones. No se afirma que el aspecto visual esté aprobado en el Editor; realiza las pruebas anteriores.

No se añadieron enemigos, eventos de terror, puzles ni interacciones con los muebles.

La firma de la comprobación de obstáculos se contrastó con la documentación oficial de [Physics2D.Linecast](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/physics2d/linecast). La carga utiliza la API existente de [SceneManager.LoadSceneAsync](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/SceneManager/SceneManager.cs/).
