# Corrección del terreno del exterior

La corrección afecta exclusivamente a `Assets/Scenes/Nivel01/N1_ExteriorReten.unity` y a su generador. No modifica otras escenas, el sistema de entrada ni ProjectSettings.

## Qué cambió

- Se reemplazó la selección aleatoria de tonos por celda por una superficie calculada en coordenadas globales: tierra y vegetación oscuras, pequeñas hojas, piedras, raíces y manchas de barro. Los límites de los tiles no intervienen en el color.
- Se conservan los diez puntos del recorrido. Una curva Catmull-Rom suaviza sus tramos y una máscara irregular mezcla los bordes del sendero con el terreno.
- Se mantiene el recorrido del arroyo. Agua, barro y transición de las orillas están integrados en la misma superficie, evitando los cuadrados opacos de las antiguas capas de agua y orilla. Las rocas existentes permanecen en sus posiciones.
- `SueloOrganico.png` mide 1344 × 896 píxeles: 42 × 28 unidades a 32 PPU. Se divide en 77 sprites contiguos de hasta 128 × 128 píxeles, colocados mediante un Tilemap. Cada región utiliza su propio fragmento de la imagen; no se repite una textura de cuatro unidades. Importación Point, sin mipmaps ni compresión, rectángulos completos y pivote inferior izquierdo.
- Los dos PolygonCollider2D del agua siguen el mismo contorno que el generador visual, con un margen de 0.13 unidades sobre la orilla. El margen evita que los pies entren en el agua. Sus polígonos terminan en las barandas existentes: norte en Y = -2.25 y sur en Y = -3.75. Queda libre el corredor del puente en Y = -3; las barandas conservan sus colliders.
- Los tiles antiguos se conservan como archivos. Se sustituyen solamente las capas generadas de suelo, agua y detalles; se conserva la vegetación baja y todos los objetos detallados.

## Persistencia y reconstrucción

`ExteriorTerrain` migra una vez las capas antiguas, incluso si ya estaban guardadas. Guarda una revisión en la escena para que las aperturas siguientes no repinten el terreno. La reconstrucción explícita vuelve a generar el PNG desde `OrganicTerrainField`, rellena las capas generadas y guarda la escena, con las colisiones recalculadas. También conserva las celdas pintadas en `Vegetacion_baja`.

`StreamCollisionLayout` está conectado a los colliders existentes `Agua_Norte` y `Agua_Sur`; no genera un collider que atraviese el puente. La escena contiene también los polígonos actualizados como datos serializados.

El menú de reconstrucción reemplaza las ediciones manuales de suelo, agua y detalles de esas tres capas. Úsalo para regenerar la superficie; para conservar una edición posterior, guarda normalmente la escena.

## Pasos en Unity Editor

1. Detén Play. Espera a que termine la importación y compilación. Si Auto Refresh está desactivado, pulsa **Assets > Refresh**.
2. Abre desde Project **Assets > Scenes > Nivel01 > N1_ExteriorReten**. Si la escena estaba abierta durante los cambios externos, vuelve a abrirla y utiliza la versión actualizada del disco cuando Unity lo solicite.
3. Ejecuta **Frecuencia Cero > Exterior Reten > Reconstruir terreno organico y guardar**. Esto prepara y guarda los Tilemaps y los polígonos en la escena. No requiere configurar referencias manualmente. En adelante también puedes usar **Guardar Tilemaps preparados** para guardar la preparación existente.
4. En Hierarchy, abre **Exterior_Reten > Terreno_Tilemaps > Grid_Exterior > Suelo_y_sendero**. La capa contiene la superficie continua; `Arroyo_estatico` y `Detalles_suelo` quedan vacías porque sus colores están integrados en el suelo.
5. Pulsa **Play**, haz clic en Game y mueve a Leo con **WASD**. Recorre el sendero desde la camioneta hasta el retén. Comprueba que no aparecen cuadrados contrastantes ni líneas en las uniones, especialmente con la linterna encendida.
6. Acércate al arroyo por ambos lados, al norte y al sur del puente: Leo debe detenerse en la orilla. Cruza por el centro del puente, aproximadamente Y = -3, en ambos sentidos. Prueba también las esquinas de sus barandas para comprobar que no permiten entrar al agua.
7. Para inspeccionar geometría, activa **Gizmos** en Scene y selecciona **Exterior_Reten > Zona_C_Arroyo > Agua_Norte** y **Agua_Sur**. En **Polygon Collider 2D > Edit Collider**, los polígonos deben seguir las orillas y dejar el hueco del puente.
8. Detén Play, guarda, cierra y vuelve a abrir la escena. Comprueba que se conserva el terreno. Ejecuta otra vez la reconstrucción y verifica que el aspecto y el cruce permanecen iguales.

No cambies Input System, URP, Pixel Perfect Camera ni los controles existentes para esta corrección.

## Archivos

Modificados:

- `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`
- `Assets/Art/Nivel01/Terrain/ExteriorTerrainLayout.json`
- `Assets/Scripts/World/ExteriorTerrain.cs`
- `Assets/Editor/ExteriorRetenTools.cs`

Creados, con sus respectivos `.meta`:

- `Assets/Scripts/World/OrganicTerrainField.cs`
- `Assets/Scripts/World/ContinuousGroundTile.cs`
- `Assets/Scripts/World/StreamCollisionLayout.cs`
- `Assets/Editor/OrganicTerrainBaker.cs`
- `Assets/Art/Nivel01/Terrain/SueloOrganico.png`
- `Assets/Art/Nivel01/Terrain/SueloOrganico.asset`

Documentación y respaldo:

- `Docs/CorreccionTerrenoExterior.md`
- `Docs/Backups/CorreccionTerreno/N1_ExteriorReten.unity`
- `Docs/Backups/CorreccionTerreno/ExteriorTerrain.cs`
- `Docs/Backups/CorreccionTerreno/ExteriorRetenTools.cs`
- `Docs/Backups/CorreccionTerreno/ExteriorTerrainLayout.json`

Herramientas y resultados temporales de la comprobación están en `Temp/CorreccionTerreno`: `prepare.ps1`, `verify.ps1`, `preserved.json`, `compile.rsp`, `Prototype.dll` y `verification.txt`. Unity puede limpiar Temp. `prepare.ps1` es un script de aplicación inicial, no debe ejecutarse nuevamente sobre la escena modificada; utiliza el menú de Unity para reconstruir.

## Comprobaciones y límites

- Compilación de todos los scripts de Scripts y Editor con Roslyn y las bibliotecas locales de Unity 6000.6.3f1: sin errores.
- Se inspeccionó visualmente el PNG generado, fuera del Editor. No contiene el mosaico anterior.
- 10112 muestras del PNG coinciden exactamente con el generador reproducible. El salto RGB medio entre píxeles en las uniones de los fragmentos es 3.859, frente a 3.949 en el resto de la imagen; no se introducen saltos de color asociados al corte de los tiles.
- 17149 muestras del agua visible fuera del puente quedan dentro de los polígonos serializados. 1336 muestras del rectángulo que contiene los pies de Leo a lo largo del cruce quedan fuera de los colliders del agua.
- Se conservan idénticos 621 bloques serializados de la escena. Cambian solamente el componente de terreno, el objeto padre del arroyo para añadir el componente de contorno y los dos polígonos del agua. Se añade un componente; no se elimina ninguno.
- Los hashes de los scripts de jugador, iluminación, cámara, HUD, ProjectSettings y los atlas existentes coinciden con los anteriores. Las celdas de vegetación baja se conservan.

**No se ejecutó la escena en Unity Editor ni Play.** La compilación independiente no comprueba la importación real de sprites, el renderizado con URP, la iluminación nocturna ni la respuesta de la física durante Play. Los pasos anteriores son necesarios para verificar estos puntos; no se afirma que el resultado visual esté aprobado en el Editor.

No se implementaron mecánicas nuevas ni otras escenas.
