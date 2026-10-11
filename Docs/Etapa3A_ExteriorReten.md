# Frecuencia Cero — Etapa 3A

Se creó `Assets/Scenes/Nivel01/N1_ExteriorReten.unity` como escenario independiente. `Nivel01_Bosque.unity`, todos los scripts anteriores, Input System, Renderer2D, Packages y ProjectSettings se conservaron sin modificaciones; se verificaron mediante hashes SHA256. La escena original también tiene una copia previa en `Docs/Backups/Nivel01_Bosque.pre-etapa3A.ffc77c0efa7543c3a5cc7f0b4ef9ae5b.unity.txt`.

## Organización del exterior

- **Sistemas_Leo**: copia de Leo, Rigidbody2D, collider, YSort, movimiento y Linterna con sus componentes anteriores. Se cambió la posición inicial a (-11, 3.6) y se añadió LeoSpriteAnimator solamente en esta nueva escena. La cámara, luz global e interfaz se copiaron con sus referencias intactas.
- **Exterior_Reten / Terreno_Tilemaps**: definición persistente de 1.426 celdas. Al abrir la escena, ExteriorTerrain construye cuatro Tilemaps: suelo/sendero, arroyo, detalles del suelo y vegetación baja. Los mapas guardados o pintados posteriormente tienen prioridad: no se repintan automáticamente.
- **Zona_A_Llegada**: camioneta a la izquierda, poste y cartel de mantenimiento. Espacio de salida libre debajo de la camioneta.
- **Zona_B_Bosque**: sendero curvo de anchura variable, 70 árboles de tres tipos distribuidos con separación irregular, rocas grandes y troncos caídos. Hojas, barro, piedras, charcos, raíces y arbustos aparecen en el terreno.
- **Zona_C_Arroyo**: agua estática de orillas variables, rocas y puente horizontal. Dos PolygonCollider2D cierran el agua al norte y sur del puente; las barandas tienen colisiones. El paso entre barandas queda abierto.
- **Zona_D_Reten**: caseta con techo, puerta, ventana y escalones representados en su sprite; cercas, cajas, barriles, señal y poste. El collider representa las paredes/base, no toda la cubierta.
- **Faroles_Calidos**: tres Light2D Point localizadas, sin sombras ni volumetría. Mantienen el material Sprite-Lit y contrastan con la luz global azulada y la linterna.
- **Limites_Exploracion**: cuatro colliders en los extremos del terreno de 42 × 28 unidades.

El recorrido sale de la izquierda, curva hacia el centro inferior, cruza el puente alrededor de (0.5, -3) y sube hacia la caseta en (11, 4). Los árboles se ordenan por Y en la base del tronco; sus copas no bloquean todo su ancho. Los objetos grandes y sus colliders son independientes. Las celdas repetitivas no son GameObjects individuales: solo se crean cuatro Tilemaps y su Grid.

## Archivos creados

- `Assets/Scenes/Nivel01/N1_ExteriorReten.unity` y `.meta`.
- `Assets/Scripts/World/ExteriorTerrain.cs`: construcción de los Tilemaps desde un layout persistente, sin sobrescribir mapas existentes.
- `Assets/Scripts/World/ExteriorTile.cs`: tile reutilizable con sprite y escala visual.
- `Assets/Scripts/World/FutureRetenEntrance.cs`: marcador informativo de la futura puerta; guarda N1_InteriorReten y E, sin ejecutar entrada ni transición.
- `Assets/Scripts/Player/LeoSpriteAnimator.cs`: reproduce las poses disponibles según la velocidad del Rigidbody2D; no modifica input ni movimiento.
- `Assets/Editor/ExteriorRetenTools.cs`: guarda los Tilemaps preparados en la nueva escena.
- `Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png`: atlas nuevo, 16 recortes individuales definidos en su `.meta`.
- `Assets/Art/Nivel01/Characters/Leo_Exterior.png`: copia exacta de Leo_Sprites.png con un importador independiente de 16 poses. La imagen original y su importador se conservaron.
- `Assets/Art/Nivel01/Terrain/*.png`: once texturas pequeñas de terreno generadas mediante patrones deterministas: Bosque_01, Bosque_02, Musgo, Tierra, Barro, Tierra_Piedras, Agua_01, Agua_02, Hojas, Charco y Orilla.
- `Assets/Art/Nivel01/Terrain/*.asset`: trece tiles; los once anteriores más Arbustos_Bajos y Raices_Bajas usando sprites del atlas.
- `Assets/Art/Nivel01/Terrain/ExteriorTerrainLayout.json`: posiciones y tipos de las celdas.
- `.meta` de scripts, assets y carpetas nuevas.
- Esta guía, `Docs/Etapa3A_Assets.md` y `Docs/Etapa3A_SpriteRectangles.csv`.
- Copia previa en Docs/Backups y archivos de preparación/comprobación en Temp/FrecuenciaCeroEtapa3A (estos últimos no forman parte del juego).

No se modificó ningún archivo que existiera antes de esta etapa. Los nuevos scripts extienden únicamente el nuevo exterior.

## Assets usados y pendientes

Se inspeccionó visualmente `ExteriorReten_Referencia.png` (1600 × 894, opaca). Se usó para orientar la composición y el arte; no aparece como fondo ni como recurso en la escena.

Se inspeccionó `Leo_Sprites.png` (1254 × 1254, transparencia real). Su importador original agrupaba varias poses en un solo sprite, además de objetos y fragmentos pequeños. Se preparó una copia con recortes medidos de las 16 poses de Leo: cuatro por dirección cardinal. Las diagonales reutilizan la dirección dominante; no se crearon poses nuevas que no estén en la hoja. Se excluyeron enemigos, documentos, muebles y otros objetos del interior.

No había hojas de árboles, caminos, vehículos, cercas ni edificios en SourceSheets. Se generó un atlas provisional nuevo con imagegen para cubrir esos elementos; su procedencia y prompt están en Etapa3A_Assets.md. No se presenta como arte preexistente o definitivo. Los sprites tienen alfa real, sin cuadrícula dibujada. Los límites se midieron individualmente, porque la hoja generada no respeta celdas uniformes.

Todos los importadores nuevos usan Point, sin mipmaps ni compresión. El terreno tiene 32 PPU. El atlas detallado, más grande, usa 64 PPU, y la copia de Leo usa 256 PPU para convertir sus poses de aproximadamente 190 píxeles de alto en un personaje de aproximadamente 0.75 unidades. Se conservó Pixel Perfect Camera con Assets PPU 32 y referencia 640 × 360; los sprites de alta resolución se muestrean sobre esa cuadrícula. Las escalas uniformes de los props ajustan tamaños físicos coherentes: cajas y barriles pequeños, árboles grandes y caseta de unas cinco unidades de ancho. Los colliders compensan esa escala y mantienen medidas en unidades de mundo.

Queda pendiente arte definitivo del entorno, terreno con bordes pintados/RuleTiles más elaborado, pulido de las poses y del puente, sombras/oclusión y evaluación visual de brillo y superposición en Unity. La caseta y la camioneta son sprites provisionales completos y reemplazables. El terreno tiene una cuadrícula de una unidad; el sendero varía su contorno, pero sus bordes todavía conservan el escalonamiento de las celdas.

## Abrir y probar en Unity

1. Detén Play si está activo. Vuelve a Unity 6000.6.3f1 y espera que importe y compile los archivos. Si no aparecen, usa **Assets > Refresh**.
2. En **Project > Assets > Scenes > Nivel01**, haz doble clic en **N1_ExteriorReten**. No hace falta modificar ni reemplazar Nivel01_Bosque.
3. El componente ExteriorTerrain prepara los Tilemaps al cargar la escena. En Hierarchy, despliega **Exterior_Reten > Terreno_Tilemaps > Grid_Exterior**: deben aparecer los cuatro mapas. La definición y sus assets ya están guardados en disco; sus GameObjects se materializan al abrir la escena.
4. Para dejarlos también guardados como Tilemaps editables dentro del `.unity`, antes de Play pulsa **Frecuencia Cero > Exterior Reten > Guardar Tilemaps preparados**. Este comando es el paso manual para persistir las celdas mediante el serializador de Unity; no reconstruye los objetos ni sistemas. Tras guardarlos, puedes pintarlos con Tile Palette y el componente respetará esos mapas.
5. Abre **Window > General > Console** y verifica que no haya errores rojos ni Missing Script. Si no aparecen los mapas, comprueba que ExteriorTerrain tenga Layout, Lit Material y trece tiles asignados; las referencias ya están escritas en la escena y no requieren asignación normal manual.
6. En **Game**, elige 640×360, 1280×720 o 1920×1080. Mantén Pixel Perfect activo.
7. Pulsa **Play** y haz clic en Game. Leo comienza junto a la camioneta. Comprueba WASD, diagonales, seguimiento de cámara y animación.
8. Mueve el mouse y pulsa F. Comprueba linterna y batería; la UI debe permanecer fija. Para una prueba corta, cambia Full Battery Duration a 10 en **Sistemas_Leo > Leo > Linterna > Flashlight Battery** antes de Play, y después restáurala a 120.
9. Sigue el camino hacia la derecha y abajo hasta el puente. Cruza hacia la otra orilla y sigue el sendero hacia arriba/derecha hasta la caseta.
10. Intenta entrar al agua fuera del puente y chocar contra camioneta, rocas, cercas, paredes y troncos. Las copas deben permitir pasar detrás visualmente mientras la base sigue siendo sólida.
11. Acércate a **Zona_D_Reten > Puerta_Futura_E**. La puerta aún no responde a E: solo está preparada para una futura interacción. No existe transición de escena.
12. Detén Play. Los cambios realizados durante Play no se conservan. Para sustituir un prop, cambia Sprite Renderer > Sprite y ajusta su collider si cambia la dimensión física.

No se requiere cambiar Input System, Graphics Settings, Renderer2D ni paquetes. No se añadieron escenas futuras ni se cambió Build Profiles: para probar basta abrir el exterior y pulsar Play.

## Verificación y límites

- Compilación de todos los scripts runtime y Editor con Roslyn de Unity y las bibliotecas/paquetes locales: sin errores ni advertencias.
- Referencias locales de la nueva escena verificadas, IDs y GUIDs sin duplicados, assets nuevos presentes.
- Hashes de archivos previos comprobados: prototipo, scripts existentes, hojas originales y configuración permanecen idénticos.
- Inspección de las imágenes, dimensiones, alfa y rectángulos de recorte; registro en Etapa3A_SpriteRectangles.csv.
- Revisión matemática de la línea central del recorrido con la huella de Leo y los colliders previstos. Se retiró un cartel que invadía el camino. Es una comprobación geométrica, no una simulación de Physics2D.

No se pudo controlar el Editor abierto desde estas herramientas. No se ejecutó Unity Play ni se verificó el render final, la carga de los Tilemaps, la importación de los sprites múltiples, la animación ni los contactos reales. Tampoco se ejecutó desde aquí el comando de guardado de Tilemaps. Deben comprobarse con los pasos anteriores; la compilación y la geometría no demuestran que el escenario funcione durante Play.

El atlas provisional mejora el detalle, pero no equivale al acabado completo de la referencia. Sin sombras complejas, las luces no quedan ocluidas por árboles y paredes. No se incorporaron enemigos, inventario, puzles, interior, generador, sendero de escape ni carga de otras escenas.
