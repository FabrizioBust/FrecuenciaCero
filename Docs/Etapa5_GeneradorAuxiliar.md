# Etapa 5 — Generador auxiliar

Se creó `Assets/Scenes/Nivel01/N1_Generador.unity` y se conectó con el exterior mediante el sistema existente de E y `RetenTravel`. No hace falta reconstruir escenas ni asignar referencias a mano.

## Misión

Al terminar todas las líneas de la radio con el supervisor, el objetivo pasa directamente a **Localizar el generador auxiliar**. En los archivos existía una condición antigua de bitácora/mapa: se eliminó esa dependencia. Sus archivos y conversaciones originales se conservaron; la anomalía anterior que revelaba «Soy Leo» queda deshabilitada. Esta etapa usa exclusivamente la nueva transmisión solicitada después de la reparación.

La primera inspección del generador presenta las dos líneas de Leo y cambia el objetivo a **Encontrar un fusible de repuesto**. Al regresar sin repuesto, indica que necesita uno compatible. En el almacén abierto cercano hay un fusible recogible, con desaparición y aviso breve en el HUD. El registro sencillo de objeto clave sustituye la necesidad de un inventario: recuerda la recogida y consume el fusible al instalarlo. Si el jugador explora el almacén primero, puede recogerlo, pero todavía debe examinar el generador.

La instalación presenta las dos líneas de Leo; la reparación se registra al terminar el diálogo. Durante 1,6 segundos alternan el sprite apagado/encendido y las luces, con una pequeña vibración exclusivamente del sprite. Después permanecen encendidos los dos postes y la luz del generador. Dos segundos después de la reparación se abre la transmisión: RADIO «Krrrr...», VOZ DESCONOCIDA «Ahora sabemos dónde estás», LEO «¿Quién está hablando?», RADIO «Krrrr...». Se registra como completada al cerrar la última línea y no se repite. El objetivo queda en **Regresar al camino de servicio**.

No se encontraron clips WAV, OGG ni MP3 en Assets. No se añadió audio de motor ni grabaciones de voz. Las líneas de radio conservan identificadores `radio_static` y `unknown_voice` compatibles con el evento de sonido existente, pero no reproducen sonido por sí solos.

## Escena y persistencia

La nueva escena contiene un camino continuo con bordes irregulares, bosque, caseta abierta, máquina antigua, almacén de repuestos, herramientas, barril, cajas, señales, postes y cables. Los obstáculos y límites usan BoxCollider2D. La máquina tiene una colisión fija separada de su sprite animado; los almacenes no tienen un collider que cierre su entrada.

Reutiliza los componentes actuales de movimiento, animación de Leo, cámara ortográfica/Pixel Perfect, linterna, batería, HUD e interacción. La nueva cámara conserva la configuración del exterior. El interior existente no se modificó. La niebla utiliza el material y bancos existentes, con una nueva textura ambiental para este terreno; `useExteriorDensity` está desactivado únicamente aquí para no aplicar las coordenadas del río del retén. Los valores del exterior no cambian.

`RetenMission.Session` conserva conversación, objetivo, inspección, fusible, reparación y anomalía durante la partida. `RetenTravel` sigue transportando batería y encendido de la linterna, y `HudVisibility` conserva TAB. Cada escena tiene su propia copia de los sistemas actuales y se carga con `LoadSceneMode.Single`; no se crearon gestores persistentes ni un segundo HUD simultáneo. Una nueva sesión de Play reinicia la misión; no se implementó guardado entre ejecuciones.

Los sprites nuevos usan Point, sin mipmaps ni compresión. Los dos estados de la máquina comparten dimensiones de recorte, PPU y pivote. Los colliders, escala de Leo, tamaño del HUD y controles actuales no se modificaron. El arte generado y el terreno se guardaron en Assets: abrir y guardar las escenas no depende de un generador en ejecución.

## Prueba en Unity

1. Vuelve a Unity y deja terminar la importación/compilación. Si pregunta por los cambios externos de escena, usa **Reload** para cargar los archivos actualizados. Si tienes modificaciones manuales sin guardar, respáldalas antes de recargar.
2. En Project, abre `Assets > Scenes > Nivel01 > N1_ExteriorReten` con doble clic. Abre **Window > General > Console** y comprueba que no aparezcan errores. La lista de escenas ya incluye `N1_Generador`; se puede revisar en **File > Build Profiles > Scene List**.
3. Pulsa **Play** y haz clic en Game. Entra al retén con E, acércate a la radio sobre el escritorio y completa toda la conversación con E o Enter. Si el texto aún se escribe, la primera pulsación lo completa y la siguiente avanza. Comprueba **Localizar el generador auxiliar** sin leer documentos.
4. Sal con E. Desde la puerta exterior, sigue el pequeño sendero hacia abajo y a la derecha. Junto a la señal, aproximadamente en `(14, -0.4)`, aparece **E: seguir el camino de servicio**. Pulsa E. El acceso de regreso queda detrás de Leo, al suroeste.
5. Sigue el camino hacia arriba y a la derecha hasta la máquina frente a la caseta. Acércate por el lado inferior y pulsa **E: examinar generador**. Completa ambas líneas y comprueba **Encontrar un fusible de repuesto**. Vuelve a examinar sin fusible para comprobar la respuesta breve.
6. Sigue el ramal hacia arriba y a la izquierda. En el almacén abierto de madera, entra por abajo y busca el fusible claro delante de la estantería, cerca de `(-7.4, 5.15)`. Pulsa **E: recoger fusible**. Debe desaparecer, mostrar el aviso y cambiar el objetivo a **Instalar el fusible en el generador**. No debe volver a recogerse.
7. Regresa a la máquina y pulsa **E: instalar fusible**. Completa las dos líneas. Comprueba la breve animación, el encendido de las luces y **Regresar al camino de servicio**. Espera dos segundos; completa la nueva transmisión extraña.
8. Pulsa TAB para ocultar el HUD. Mantén la linterna encendida y regresa por el mismo sendero a la señal del suroeste; pulsa **E: regresar al camino de servicio**. Entra y sal del retén. Comprueba que TAB sigue oculto, la batería conserva su carga en vez de volver a 100, y el objetivo no cambia. Pulsa TAB otra vez para ver el HUD. Los diálogos y prompts E siguen visibles incluso con el HUD oculto.
9. Vuelve al generador. El fusible debe seguir ausente, la máquina y las luces deben estar encendidas y la anomalía no debe repetirse. Revisa WASD, apuntado con mouse, F, colisiones, niebla y Console en las tres escenas.
10. Detén y vuelve a iniciar Play: comienza una sesión nueva con el HUD visible y la misión inicial. Abrir directamente `N1_Generador` en una sesión nueva permite inspeccionar el escenario, pero sus interacciones requieren completar primero la radio desde el retén.

## Archivos creados

- `Assets/Scenes/Nivel01/N1_Generador.unity` y su `.meta`.
- `Assets/Scripts/World/GeneratorInteraction.cs`, `FusePickup.cs`, `GeneratorVisual.cs`, `GeneratorTerrainField.cs`, con sus `.meta`.
- `Assets/Narrative/Nivel01/Generador_Examinar.asset`, `Generador_SinFusible.asset`, `Generador_Instalar.asset`, `Generador_Funcionando.asset`, `Generador_Anomalia.asset`, con sus `.meta`.
- Carpeta `Assets/Art/Nivel01/Generator` y su `.meta`: `Generador_Atlas.png`, `SueloGenerador.png`, `NieblaGenerador.png`, `Cables.png`, `AccesoServicio.png`, con sus `.meta`.
- `Docs/Etapa5_GeneradorAuxiliar.md`, `Docs/Etapa5_PromptAtlas.txt`.
- `Docs/Validation/Etapa5/Verificar.ps1` y `EstadoAnterior.json`, para repetir las pruebas del modelo y las comprobaciones estáticas de referencias, geometría y alcance de cambios.
- `Docs/Backups/Etapa5`: versiones anteriores de los siete archivos modificados y copia de `N1_InteriorReten.unity`, que permaneció intacta.
- Archivos temporales de preparación y compilación en `Temp/FrecuenciaCeroEtapa5`; no forman parte del juego.

## Archivos modificados

- `Assets/Scripts/Narrative/MissionProgress.cs`: progreso y objetivos de la nueva misión, sin dependencia de documentos.
- `Assets/Scripts/Narrative/RetenMission.cs`: momento de reparación y aviso de recogida.
- `Assets/Scripts/Narrative/NarrativeDirector.cs`: transmisión después de reparar, reutilizando el diálogo y bloqueo modal existentes.
- `Assets/Scripts/UI/NarrativeUI.cs`: aviso breve de fusible, respetando TAB y las escalas actuales.
- `Assets/Scripts/World/ForestFogController.cs`: selección de densidad del exterior o del nuevo terreno; comportamiento anterior por defecto.
- `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`: solo acceso al generador, punto de regreso, señal y prolongación del sendero. Se conservaron todos sus componentes originales salvo la lista de interacciones, a la que se añadió el nuevo acceso.
- `ProjectSettings/EditorBuildSettings.asset`: nueva escena habilitada, conservando escenas y configuración de entrada existentes.

## Comprobaciones realizadas y límites

- C# de Scripts y Editor compilado mediante Roslyn del SDK de Unity 6000.6.3f1, con ensamblados locales de Unity, Input System y URP: **código de salida 0, sin errores ni advertencias**.
- Pruebas ejecutadas del modelo real: conversación sin documentos, objetivos sucesivos, recogida única, reparación con requisitos, consumo del objeto clave, anomalía única, repetición de radio/documentos sin regresión, y recogida anticipada del fusible.
- Comprobadas las referencias locales, jerarquía y dependencias de las tres escenas, referencias de sprites y registro de carga. Todos los colliders serializados de la zona nueva se comprobaron con una búsqueda de rutas conservadora del tamaño de los pies de Leo y líneas de visión estáticas hacia las interacciones.
- Comparación SHA256 de los 265 archivos preexistentes registrados: únicamente los siete cambios previstos; sin archivos eliminados. Se comprobaron importadores Point, transparencia y dimensiones de los recortes. Se inspeccionaron los PNG del atlas y del suelo, pero no su composición bajo iluminación de Unity.
- **No se ejecutó Unity Play ni se verificó visualmente la nueva escena dentro del Editor.** Las pruebas externas no sustituyen la física, la importación efectiva, el renderizado, los controles y las transiciones en tiempo real. La prueba manual anterior es necesaria antes de confirmar la etapa.

Para repetir la validación externa desde la raíz del proyecto: `powershell -NoProfile -ExecutionPolicy Bypass -File Docs/Validation/Etapa5/Verificar.ps1`. Utiliza archivos actuales, PowerShell/.NET y `rg`; no abre ni modifica escenas en Unity. La comparación de alcance corresponde a esta entrega y detectará cambios posteriores como diferencias.
