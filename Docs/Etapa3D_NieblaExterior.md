# Etapa 3D — Niebla ambiental del exterior

## Implementación

Se añadió `Exterior_Reten > Niebla_Exterior` únicamente a `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`. La escena interior y los scripts existentes conservan sus archivos. Antes de editar se respaldó el exterior en `Docs/Backups/Etapa3D/N1_ExteriorReten.unity`.

`ForestFogController` controla dos capas independientes en coordenadas del mundo. No sigue ni se coloca sobre la cámara, no utiliza partículas, niebla 3D ni efectos volumétricos. La aparición inicial de Leo junto a la camioneta, las transiciones con E, los controles, la batería y el HUD no se modifican.

### Capa A: niebla ambiental

Un SpriteRenderer cubre 96 × 72 unidades, con una textura de alfa procedural de 1536 × 1152 a 16 PPU. Esto deja margen alrededor del área explorable y de la vista de cámara. Las últimas ocho unidades de sus extremos se desvanecen; todos los píxeles del borde tienen alfa cero.

El patrón mezcla ruido suave a varias escalas. La densidad se reduce cerca del sendero curvo existente y en las zonas despejadas de la camioneta y el retén; aumenta fuera de esas zonas, entre los árboles, y alrededor del río. Los puntos del sendero y el centro del río proceden de `OrganicTerrainField`, sin modificar ese generador ni su geometría.

La capa oscila muy lentamente, con desplazamientos máximos de 0.28 unidades en X y 0.18 en Y. El desplazamiento reducido conserva la correspondencia aproximada de la máscara con los caminos. Su opacidad tiene una variación temporal leve.

Orden de renderizado: Sorting Layer **Default**, Order **-1200**. Se dibuja después del suelo y la vegetación baja, antes de los árboles, objetos y Leo. No oculta las copas con una única capa frontal uniforme.

### Capa B: bancos móviles

Hay un pool fijo de **24 SpriteRenderer**, de los que **12** están habilitados por defecto. Todos reutilizan `BancoNiebla.png`, de 256 × 128 a 32 PPU, con una silueta irregular, gradientes de alfa y borde completamente transparente.

Cada banco tiene ancho de 10–16 unidades, alto de 4.5–8, pequeñas rotaciones y reflejos del sprite para variar su apariencia. Sus velocidades y fases son distintas. Un tercio se origina cerca del río; los demás se distribuyen por el bosque y sus alrededores. La transparencia de cada grupo se modula también por la zona en la que está su centro: menos en caminos y claros, más en el río.

Los ciclos duran 130–240 segundos. Durante el primer 23% aparecen gradualmente, y durante el último 27% desaparecen. El sprite se recoloca solamente al alcanzar el extremo invisible del ciclo. No se crean ni destruyen GameObjects en cada aparición. Los ciclos siguientes usan otras posiciones y duraciones.

Sorting Layer **Default**, Order **700**: una pequeña parte de la bruma se superpone a los sprites existentes para aportar profundidad. Los valores iniciales son transparentes; se limita la opacidad aplicada a cada renderer a 0.18 como máximo. La textura y el desvanecimiento reducen todavía más la opacidad visible. Superponer muchos bancos o bajar mucho la transparencia puede aumentar la densidad conjunta.

## Material y nitidez

Las dos capas comparten `ForestFogLit.mat`, que usa el shader existente **Universal Render Pipeline/2D/Sprite-Lit-Default**. Permite que las luces 2D coloreen la bruma directamente. Es iluminación de sprites transparentes; **no hay dispersión volumétrica, sombras volumétricas ni simulación de vapor**.

No se alteran Global Light 2D, linterna ni faroles. El HUD continúa siendo dibujado por su sistema actual, fuera de estas capas de sprites.

Los PNG tienen RGB blanco y alfa variable; el controlador aplica sus colores azul grisáceo y verde grisáceo. Sus importadores usan Point, Full Rect, sin mipmaps ni compresión. Los gradientes están en el canal alfa, por lo que los bordes pueden ser suaves sin cambiar el filtrado ni la nitidez de los sprites del escenario.

La superficie ambiental requiere aproximadamente 6.75 MiB de textura RGBA32 y el sprite de bancos 0.125 MiB. Hay 13 renderers de niebla habilitados por defecto, hasta 25 con todos los bancos activos. Son estimaciones de recursos, no mediciones de FPS, memoria total o draw calls en Unity.

## Inspector y ajustes

Selecciona **Exterior_Reten > Niebla_Exterior**, componente **Forest Fog Controller**.

| Parámetro | Inicial | Efecto |
| --- | ---: | --- |
| Densidad general | 0.85 | Multiplica ambas capas. 0 las oculta. |
| Velocidad de movimiento | 0.028 | Velocidad base en unidades de mundo por segundo. 0 detiene el desplazamiento; los ciclos de aparición siguen funcionando. |
| Color de niebla | Azul grisáceo | Color principal de la bruma. |
| Transparencia | 0.88 | 1 es invisible. Reducirla aumenta la opacidad. |
| Intensidad de variacion | 0.55 | Modula la pulsación ambiental y diferencias de opacidad entre bancos. |
| Intensidad capa A | 0.7 | Permite ajustar u ocultar solo la niebla ambiental. |
| Intensidad capa B | 0.55 | Permite ajustar u ocultar solo los bancos. |
| Cantidad de bancos | 12 | De 0 a 24, reutilizando el pool existente. |
| Tinte de bancos | Verde grisáceo | Se mezcla con el color principal. |
| Area de bancos | X=-32, Y=-24, W=64, H=48 | Región inicial de los bancos generales; los cercanos al río usan su distribución específica. |

Para **menos niebla**, prueba Densidad general **0.5** o Transparencia **0.93**. Para **más niebla**, empieza con Densidad general **1.1** conservando Transparencia **0.88**. Puedes reducir Cantidad de bancos a **6–8** para despejar la capa frontal.

Cambia un parámetro cada vez y compara encendiendo/apagando el componente. Guarda los ajustes fuera de Play con **Ctrl+S**; los cambios hechos durante Play normalmente no se conservan al detenerlo.

## Cómo probar en Unity 6000.6.3f1

1. Detén Play y deja terminar la importación. Si Auto Refresh está desactivado, pulsa **Assets > Refresh**. Revisa Console.
2. Abre **Assets > Scenes > Nivel01 > N1_ExteriorReten**. Si estaba abierta durante los cambios externos, vuelve a abrirla utilizando la versión actualizada del disco.
3. Selecciona **Exterior_Reten > Niebla_Exterior**. Las referencias de la capa A y del pool ya están conectadas; no hay que colocar sprites ni configurar Sorting Layers a mano.
4. Pulsa **Play**, haz clic en Game y camina con WASD desde la camioneta hacia el bosque. La niebla debe mantenerse en el mundo cuando se mueva la cámara; no debe avanzar rígidamente con Leo.
5. Observa los bancos durante 30–60 segundos: su desplazamiento es deliberadamente lento. Para comprobar un ciclo completo, deja Play funcionando unos 4 minutos. Las apariciones y recolocaciones deben ser graduales, sin saltos visibles.
6. Recorre el río y el sendero. Compara la densidad mayor del río con el camino más despejado. Comprueba que siguen legibles las piedras, árboles, puente y cabaña.
7. Apunta la linterna con el mouse, pulsa F y pasa por los faroles. Comprueba que el cono, los colores cálidos y el HUD permanecen visibles. La respuesta real del material a las luces debe revisarse aquí.
8. Entra al retén con E. En el interior no debe existir `Niebla_Exterior` ni renderers de niebla. Sal con E: el controlador se prepara de nuevo al cargar el exterior, manteniendo la transición y la batería del sistema existente.
9. Durante la comparación puedes poner temporalmente Densidad general en 0. Vuelve al valor elegido, detén Play, aplica los ajustes fuera de Play y guarda.
10. Para revisar rendimiento real, abre **Window > Analysis > Profiler** durante Play y compara la niebla activada/desactivada. No se efectuó esa medición desde estas herramientas.

No hace falta ejecutar un generador para construir la escena: los sprites, el material, el controlador y el pool ya están serializados. El menú **Frecuencia Cero > Niebla exterior > Regenerar texturas** sirve únicamente para volver a producir los PNG desde `ForestFogField`; mantiene sus GUID y las referencias. No reconstruye ni reemplaza el exterior.

## Archivos

Modificado:

- `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`.

Creados, incluyendo sus respectivos `.meta`:

- `Assets/Scripts/World/ForestFogController.cs`.
- `Assets/Scripts/World/ForestFogField.cs`.
- `Assets/Editor/ForestFogBaker.cs`.
- `Assets/Art/Nivel01/Fog/NieblaAmbiental.png`.
- `Assets/Art/Nivel01/Fog/BancoNiebla.png`.
- `Assets/Art/Nivel01/Fog/ForestFogLit.mat`.

Otros archivos creados:

- `Assets/Art/Nivel01/Fog.meta`.
- `Docs/Etapa3D_NieblaExterior.md`.
- `Docs/Backups/Etapa3D/N1_ExteriorReten.unity`.

Temporales de creación y verificación, sin dependencias del juego, en `Temp/FrecuenciaCeroEtapa3D`: `prepare.ps1`, `verify.ps1`, `preserved.json`, `scene-spec.json`, `compile.rsp`, `Prototype.dll`, `verification.txt`, `ambient-alpha-preview.png` y `bank-alpha-preview.png`. Los previews representan el alfa como gris para inspeccionar sus bordes; no se usan en la escena. No vuelvas a aplicar el script inicial sobre una escena ya preparada; utiliza el menú de regeneración para los PNG.

## Comprobaciones realizadas y límites

- Compilación independiente de todos los scripts de Assets/Scripts y Assets/Editor con Roslyn y las bibliotecas locales de Unity 6000.6.3f1: sin errores.
- 36300 muestras del alfa ambiental y 703 de la textura de bancos coinciden exactamente con el generador reproducible. Todos los bordes de ambos PNG tienen alfa 0.
- Se inspeccionaron los mapas de alfa fuera de Unity: hay variación continua, un sendero despejado y contornos suaves sin mosaicos de color.
- Densidad regional de muestra: sendero 0.299, bosque 1.007, río 1.550 y bosque alejado del río 0.977. Son factores del campo procedural, no medidas de visibilidad renderizada.
- La función de desvanecimiento permanece entre 0 y 1 y alcanza 0 en ambos extremos del ciclo. Su variación máxima en pasos de 0.001 del ciclo es 0.0065.
- Los márgenes de la textura se compararon geométricamente con el área explorable y el tamaño ortográfico de la cámara. La comprobación no sustituye recorrer los límites y probar relaciones de aspecto reales en Game.
- 629 bloques originales de la escena permanecen idénticos. Solo se amplía la lista de hijos de `Exterior_Reten` para alojar el objeto de niebla. No se elimina ningún bloque anterior.
- Se mantienen los 114 BoxCollider2D, dos PolygonCollider2D, la cápsula de Leo y su Rigidbody2D. Ningún objeto nuevo tiene collider ni Rigidbody.
- Todas las referencias locales de la escena resuelven. Se conservan los hashes de todos los archivos anteriores fuera de la escena exterior, incluidos interior, scripts de juego, materiales anteriores, sprites y ProjectSettings.

**No se ejecutaron Unity Editor ni Play.** No se verificaron el importador real, la apariencia final con URP, la iluminación sobre la bruma, el movimiento durante Play, el viaje con E ni el rendimiento medido. El ajuste inicial es sutil por construcción; su aprobación visual requiere las pruebas en el Editor.

No se implementaron enemigos, eventos de terror ni sistemas narrativos.
