# Frecuencia Cero — Etapa 4

## Cambios entregados

Las dos escenas existentes incluyen un objeto raíz `Narrativa_Reten` con `PlayerInteraction`, `NarrativeDirector` y `NarrativeUI`. El exterior tiene únicamente la puerta y el HUD de objetivos. El interior añade tres puntos de interacción: radio, bitácora y mapa. Los cambios están guardados en los archivos `.unity`; no hace falta montar componentes manualmente ni ejecutar un generador al iniciar Play.

La radio y la bitácora ya están dibujadas en el sprite del escritorio. Se conservaron ese sprite, el mapa de pared y todos los muebles. Los puntos de radio y bitácora están delante de sus respectivas zonas del escritorio. El mapa se examina desde el lateral derecho del escritorio para que su collider no obligue a interactuar a través del mueble. Son puntos de lectura, no objetos físicos nuevos.

### Archivos modificados

- `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`
- `Assets/Scenes/Nivel01/N1_InteriorReten.unity`
- `Assets/Scripts/World/RetenDoor.cs`

### Archivos creados

- `Assets/Scripts/Interaction/WorldInteraction.cs`: contrato común y comprobación de distancia, lado de acceso y línea de visión mediante colisiones 2D.
- `Assets/Scripts/Interaction/InteractionButtonGate.cs`: exige soltar una tecla antes de una nueva activación, también al cambiar de escena.
- `Assets/Scripts/Interaction/PlayerInteraction.cs`: único lector de E, Enter y Escape; selecciona un solo objeto disponible, el más cercano, y muestra un solo mensaje contextual.
- `Assets/Scripts/Narrative/DialogueSequence.cs`: conversaciones editables como assets, con interlocutor, texto y un identificador opcional de sonido por línea.
- `Assets/Scripts/Narrative/DocumentDefinition.cs`: documentos editables con título, texto y sprite opcional.
- `Assets/Scripts/Narrative/NarrativeTarget.cs`: puntos de radio, bitácora y mapa configurables desde Inspector.
- `Assets/Scripts/Narrative/NarrativeDirector.cs`: reproducción de conversaciones/documentos, bloqueo temporal del movimiento y secuencia narrativa.
- `Assets/Scripts/Narrative/MissionProgress.cs`: requisitos de misión y marcas de contenido completado.
- `Assets/Scripts/Narrative/ObjectiveState.cs`: registro reutilizable de objetivo con clave, descripción y revisión; evita notificaciones por asignaciones repetidas.
- `Assets/Scripts/Narrative/RetenMission.cs`: estado de sesión, sin objetos persistentes duplicados ni guardado en disco.
- `Assets/Scripts/UI/NarrativeUI.cs`: paneles, lectura con desplazamiento y HUD de objetivos.
- `Assets/Narrative/Nivel01/Radio_Supervisor.asset`
- `Assets/Narrative/Nivel01/Radio_SinRespuesta.asset`
- `Assets/Narrative/Nivel01/Radio_Anomalia.asset`
- `Assets/Narrative/Nivel01/Bitacora_Reten.asset`
- `Assets/Narrative/Nivel01/Mapa_CimaDelAguila.asset`
- Los `.meta` de los scripts y assets nuevos, y de las carpetas `Interaction`, `Narrative` y `Narrative/Nivel01`.
- Esta guía.
- Copias previas de ambas escenas y `RetenDoor.cs` en `Docs/Backups/Etapa4/`.

Los archivos auxiliares en `Temp/FrecuenciaCeroEtapa4/` contienen la instantánea de hashes, los datos de preparación, scripts de preparación/verificación, respuesta de compilación, DLL de comprobación y resultados. Son auxiliares; Unity no depende de ellos para jugar.

## Funcionamiento

Las puertas conservan sus referencias de destino y su comprobación de acceso. `RetenDoor` ahora participa en el contrato `WorldInteraction` y deja la lectura de E y el mensaje contextual a `PlayerInteraction`. Se mantiene `RetenTravel` para el cambio de escena y la conservación de batería/estado de encendido. No se modificaron sus scripts ni las mecánicas de Leo.

El controlador evalúa solo los targets asignados en Inspector. Descarta los que estén fuera de rango, al otro lado del objeto o detrás de un collider sólido. Los triggers y los colliders de Leo no bloquean la línea de visión. Un empate de distancia se resuelve por el orden del array. Cada frame procesa una sola rama: interfaz abierta, evento automático o interacción del mundo. Abrir una interfaz consume esa pulsación y no avanza su primera línea. E y Enter simultáneos tampoco saltan dos líneas.

Las interfaces deshabilitan temporalmente el componente existente `LeoMovement`, detienen su Rigidbody2D y restauran su estado previo al cerrar. No cambian `Time.timeScale`, la cámara, las luces, la linterna ni la batería. La batería continúa descargándose si la linterna sigue encendida; F sigue disponible.

E/Enter completan el texto progresivo si todavía se está escribiendo; la siguiente pulsación avanza una línea. Con `Characters Per Second = 0`, las líneas aparecen completas y cada pulsación avanza. Escape cierra documentos, pero no cancela conversaciones. La bitácora y el mapa se marcan al cerrar su panel; la radio al terminar todas sus líneas.

La conversación con el supervisor termina una sola vez por sesión. Volver a usar la radio muestra una respuesta breve. Leer los documentos antes de la conversación conserva sus marcas. Cuando las tres condiciones están completas, la anomalía tiene prioridad sobre otras interacciones y comienza al cerrar la última interfaz. Al terminar sus siete líneas, el objetivo cambia a «Localizar el generador auxiliar». La voz mantiene siempre el rótulo «VOZ DESCONOCIDA».

El estado de misión vive en memoria estática y se reinicia al comenzar una nueva sesión de Play, incluso con Domain Reload desactivado. Entrar/salir con las puertas existentes no lo reinicia. No hay `DontDestroyOnLoad`, `PlayerPrefs` ni escritura de partidas. Cada escena posee su propia interfaz y la carga existente usa `LoadSceneMode.Single`.

La bitácora y las conversaciones contienen los textos solicitados. El mapa reutiliza el sprite ilustrado existente y añade las cinco ubicaciones y una ruta de referencia textual: retén → camino de servicio → generador → torre. No asigna coordenadas ni crea destinos nuevos. La cabaña del guardabosques figura como ubicación pendiente de precisar.

La interfaz usa IMGUI como el HUD de batería existente, con paneles gris verde oscuros, texto legible y bordes discretos. Los objetivos están arriba a la derecha; la batería permanece arriba a la izquierda. No se modificaron Sorting Layers, resolución Pixel Perfect ni configuración URP. Los efectos de sonido todavía no existen: `Sound Cue` en las líneas y `On Sound Cue` en el director permiten conectar audio posteriormente.

## Inspector

Selecciona `Narrativa_Reten` en Hierarchy:

- `Player Interaction > Leo`: Leo de esa escena. `Targets`: puerta en el exterior; puerta, radio, bitácora y mapa en el interior. Para añadir futuros objetos, deriva de `WorldInteraction` y agrega el componente a este array.
- `Narrative Director > Movement`: movimiento de ese Leo. `Allow Interior Events`: activado solo en el interior. Referencias a las tres conversaciones y los dos documentos. `Characters Per Second`: 32 inicialmente, 0 para texto instantáneo. `On Sound Cue`: evento opcional sin listeners por defecto.
- `Narrative UI > Objective Notification Seconds`: duración del aviso, inicialmente 3 segundos.

En el interior, despliega `Narrativa_Reten` y selecciona sus hijos:

- `Radio_Escritorio`: punto de acceso en (-0.95, 2.02), rango 0.85.
- `Bitacora_Escritorio`: punto de acceso en (0.4, 2.02), rango 0.85.
- `Mapa_Pared_LecturaLateral`: punto de acceso en (1.25, 3.35), rango 1; se aproxima desde la derecha.

`Kind`, `Interaction Range`, `Approach Half Width`, `Approach Direction` y el Transform pueden ajustarse. Los gizmos muestran el alcance al seleccionar un punto. Esas posiciones son puntos de interacción en la cabaña existente; no son coordenadas de las ubicaciones narrativas del mapa.

Para editar textos, abre `Assets/Narrative/Nivel01` en Project y selecciona el asset correspondiente. Los diálogos ofrecen el array `Lines` con `Speaker`, `Text` y `Sound Cue`. Los documentos ofrecen `Title`, `Body` e `Illustration`.

## Prueba exacta en Unity

1. Vuelve a Unity y espera a que importe/compile. Si solicita recargar una escena modificada externamente, usa `Reload` para leer su versión actualizada. No sobrescribas el archivo nuevo con una copia vieja que siguiera abierta en el Editor.
2. En Project, abre `Assets > Scenes > Nivel01` y haz doble clic en `N1_ExteriorReten`. Deja solo esa escena cargada para esta prueba. Comprueba en Hierarchy que existe `Narrativa_Reten`.
3. Abre `Window > General > Console`, limpia la consola y verifica que no hay errores de importación/compilación. En Game elige una vista de 640×360 o 1280×720, o usa una relación 16:9 con espacio suficiente.
4. En `File > Build Profiles > Scene List`, comprueba que `N1_ExteriorReten` y `N1_InteriorReten` están habilitadas. Ambas ya están registradas; no se modificó esta configuración en la etapa 4. Si tu perfil usa una lista de escenas propia, incluye allí ambas.
5. Presiona Play y haz clic en Game para darle foco. Verifica WASD, mouse/F, niebla exterior y objetivo «Investigar el retén». Recorre el sendero hasta el retén y acércate a la puerta por debajo. Pulsa E una vez. Mantener E no debe devolver a Leo inmediatamente al exterior.
6. Dentro, rodea la mesa central y ve al escritorio de la pared superior. Frente a su parte izquierda aparece «E: usar radio». Pulsa E: debe abrirse la primera línea «RADIO: Krrrr... krrrr...», sin saltarla. Mantén E/WASD: Leo debe permanecer detenido y no deben aparecer mensajes de puertas. Usa pulsaciones separadas de E o Enter para completar/avanzar cada línea.
7. Tras terminar la conversación debe restaurarse WASD y mostrarse «Investigar el retén antes de buscar el generador», con aviso breve. Usa otra vez la radio: debe mostrar solamente el mensaje breve de falta de respuesta.
8. Frente a la parte derecha del escritorio, usa «E: leer bitácora». Comprueba el título y todos los párrafos; usa la rueda o la barra de desplazamiento para leer el final. Cierra con E o Escape. La E que abre no debe cerrarla. WASD debe bloquearse mientras lees y recuperarse al cerrar.
9. Rodea el escritorio por la derecha, sube junto al mapa de la pared superior y busca «E: examinar mapa». Lee las cinco ubicaciones y la ruta de referencia. Cierra con E o Escape. Al cerrar este último requisito debe comenzar automáticamente la anomalía. Leo seguirá bloqueado hasta terminarla; no debe saltarse su primera línea. Comprueba las dos frases rotuladas «VOZ DESCONOCIDA» y termina las siete líneas.
10. Debe aparecer «Localizar el generador auxiliar». Sal por la puerta inferior con E, observa la batería y el objetivo, vuelve a entrar y revisa la radio/documentos. La conversación inicial y la anomalía no deben repetirse. La batería no debe volver a 100% y F debe conservar su estado entre las puertas.
11. Detén Play y vuelve a iniciarlo. Prueba leer bitácora y mapa antes de la radio: el objetivo inicial debe continuar hasta terminar la conversación, y luego debe desencadenarse la anomalía sin exigir releer documentos.
12. Prueba cerca del borde entre radio y bitácora: solo debe aparecer un mensaje y abrirse una interfaz por pulsación. Al alejarte desaparece. Intenta interactuar desde detrás del escritorio/las paredes: un collider sólido entre Leo y el punto debe impedirlo. No aumentes el rango hasta que la prueba base esté confirmada.

La nueva etapa termina en el objetivo de localizar el generador; no se ha creado ni implementado el generador.

## Verificación realizada y límites

- Compilación independiente de todos los scripts de `Assets/Scripts` y `Assets/Editor` mediante Roslyn del Unity 6000.6.3f1 instalado y las bibliotecas locales del proyecto: **sin errores ni advertencias**. No equivale a confirmar la importación/compilación dentro del Editor ni a una build de Player.
- Pruebas ejecutadas del modelo de progreso: las seis permutaciones de radio/bitácora/mapa, requisitos incompletos, anomalía única, tres objetivos y repetición de interacciones sin reinicio.
- Pruebas ejecutadas del filtro de pulsaciones: E mantenida al llegar, liberación, nueva pulsación y prevención de activaciones repetidas.
- Verificación de referencias y GUIDs: un controlador por escena, tres puntos interiores, evento desactivado en el exterior y referencias internas resueltas.
- Comparación con copias previas: los 710 bloques existentes del exterior y los 172 del interior se conservaron; únicamente se amplió la lista de raíces. De los 225 archivos preexistentes examinados, 222 permanecen idénticos y solo cambiaron las dos escenas y `RetenDoor.cs`.
- Análisis independiente de los BoxCollider2D del interior, usando una envolvente conservadora de los pies: existen recorridos desde la aparición hasta cada zona, puntos sin solapamiento físico, líneas de visión muestreadas libres y zonas donde cada interacción es la más cercana. Esto no sustituye ejecutar Physics2D durante Play.
- **No se ejecutó Unity Play ni se comprobó visualmente la interfaz en el Editor.** Unity ya estaba abierto y no había una herramienta conectada para controlarlo. Quedan por confirmar importación de assets, aspecto/legibilidad en Game, pulsaciones reales, física y transición asíncrona, bloqueo/restauración de movimiento y comportamiento completo con la linterna. No se afirma que esas pruebas hayan pasado.
- La progresión se conserva con los cambios de escena normales mediante E. Detener Play reinicia la sesión. Descargar una escena manualmente durante una conversación sin terminarla no cuenta ese contenido como completado.
- Se conserva el sprite y el filtrado del mapa; el aspecto final del panel requiere revisión en Game. No se añadió audio ni una tipografía externa.

Las acciones usan el Input System ya instalado. Referencias consultadas: [InputAction 1.20](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.InputAction.html) y [RuntimeInitializeOnLoadMethod](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html).
