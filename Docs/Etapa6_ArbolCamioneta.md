# Etapa 6 — El árbol caído y la camioneta destruida

## Activación y recorrido

La camioneta existente es `Camioneta_Mantenimiento`, en `(-13, 5)`, en el exterior del retén. El tramo de regreso continúa hacia el oeste, aproximadamente a lo largo de `y = 4`. Se conserva la camioneta original, su collider y sus recursos; al ocurrir el accidente se oculta únicamente su SpriteRenderer.

Entrar en el interior no activa el accidente. El disparador es **terminar todas las líneas de la primera conversación por radio con el supervisor**. `NarrativeDirector` conserva ese diálogo original y registra su cierre en `MissionProgress.CompleteSupervisor()`. El componente nuevo `TreeFallEvent` espera a que la interfaz esté libre y reserva el evento mediante `TryBeginTreeFall()`.

Se reproduce un crujido creciente de 1,25 segundos y después un impacto. El accidente se registra en ese momento y el objetivo pasa a **Investigar el ruido del exterior**. Durante unos 0,3 segundos parpadean las dos luces locales del interior. Un desplazamiento pequeño y transitorio de la matriz de proyección produce el temblor de imagen; el Transform, el seguimiento y los límites de cámara no se modifican. La proyección normal se restablece al terminar el renderizado y al descargar el componente.

Al quedar libre la interfaz, el mismo sistema de diálogos muestra **LEO: «¿Qué fue eso...?»**. Los efectos no bloquean el movimiento; la reacción utiliza únicamente el bloqueo habitual del diálogo hasta que se cierre con E o Enter. Si se abre un documento durante el crujido, la reacción espera a su cierre. Si se sale durante el crujido, se conserva el accidente y no se reproduce otra vez al volver.

En el exterior, `TruckAccident` refleja el estado de la sesión: muestra el conjunto de árbol/camioneta destruida y habilita sus colliders. Acercarse al punto `(-10.1, 4)` a menos de 1,8 unidades, con una línea de visión libre y sin otro diálogo abierto, inicia automáticamente la conversación de descubrimiento. Se emplean los ocho textos solicitados, sin añadir otro HUD o sistema de radio.

**Completar las ocho líneas** registra `WreckDiscovered` y cambia el objetivo a **Localizar el generador auxiliar**. La misión existente del fusible y la reparación continúa a partir de ahí; se añadió ese requisito tanto al modelo de progreso como a la disponibilidad de las interacciones del generador y del fusible. Visitar esa escena antes no permite saltarse el descubrimiento.

## Persistencia

Los indicadores `TreeFallStarted`, `TreeFallen`, `TreeReactionCompleted` y `WreckDiscovered` pertenecen a `RetenMission.Session`, sin referencias a objetos de escenas. Se conservan al entrar/salir y al ir al generador. Al iniciar una nueva sesión de Play, el reinicio existente crea otro `MissionProgress` y la camioneta vuelve a estar intacta.

**No hay guardado permanente en disco.** Se conservan los mecanismos existentes de batería, encendido de linterna y visibilidad del HUD mediante Tab.

## Recursos provisionales y colisiones

- `Arbol_Camioneta_Provisional.png`: arte generado a partir del atlas local del exterior con la habilidad imagegen. Incluye techo y carrocería deformados, cristales rotos, raíces, tronco irregular, copa y restos. PNG con alfa real; filtro Point, sin mipmaps ni compresión, 110 PPU para conservar aproximadamente el tamaño de la camioneta original. Este arte debe revisarse y, si procede, sustituirse por arte definitivo.
- Crujido e impacto: clips sintéticos monofónicos generados por `TreeFallEvent` mediante `AudioClip.Create`. No se descargó audio ni se añadieron dependencias. Los campos `woodCreak` e `impact` permiten sustituirlos por grabaciones definitivas; si están vacíos se utilizan los sonidos provisionales. No se añadieron grabaciones de voz.
- `Raices_Carretera`, `Tronco_Irregular` y `Copa_y_Ramas`: tres PolygonCollider2D no trigger, activos solamente tras el accidente. Las raíces y el tronco cruzan el tramo de carretera de regreso. El collider original de la camioneta sigue activo.
- El arte compuesto utiliza el `YSort` existente, con la posición de referencia de la camioneta; no se altera el orden visual de Leo ni el resto del bosque. La copa entera comparte un punto de ordenación, por lo que la oclusión de sus ramas requiere revisión visual manual.

## Archivos de esta etapa

Modificados:

- `Assets/Scripts/Narrative/MissionProgress.cs`: indicadores del accidente, objetivos y requisito previo al generador.
- `Assets/Scripts/World/GeneratorInteraction.cs` y `FusePickup.cs`: disponibilidad después del descubrimiento.
- `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`: raíz `Etapa6_Accidente` y sus objetos nuevos.
- `Assets/Scenes/Nivel01/N1_InteriorReten.unity`: raíz `Etapa6_Estruendo`, audio y referencias.

Creados, junto con sus `.meta` cuando corresponde:

- `Assets/Scripts/World/TreeFallEvent.cs`.
- `Assets/Scripts/World/TruckAccident.cs`.
- `Assets/Narrative/Nivel01/Accidente_Reaccion.asset`.
- `Assets/Narrative/Nivel01/Accidente_Descubrimiento.asset`.
- `Assets/Art/Nivel01/Accident/Arbol_Camioneta_Provisional.png` y la carpeta `Accident`.
- Este documento.

Los controladores de movimiento, animación, cámara, batería, HUD, niebla, radio y transiciones no se reescribieron. La escena del generador no se editó. Los bloques anteriores de las dos escenas se conservan; solo se incorporan objetos nuevos y sus referencias a SceneRoots. `decoratedBounds` y `roomBounds` mantienen su serialización corregida y sus dimensiones.

## Cómo comprobarlo en Unity

1. Deja terminar la importación y abre `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`. Si el editor mantiene una versión anterior de la escena, recárgala después de conservar cualquier edición manual pendiente.
2. Inicia Play. Comprueba la camioneta intacta, el objetivo **Investigar el retén**, WASD, cámara, mouse/F y batería.
3. Cruza el puente, entra al retén con E y verifica que no haya estruendo al entrar. Acércate a la radio y termina la conversación completa con E/Enter.
4. Escucha el crujido y el impacto; revisa el parpadeo sutil y el temblor breve. Cierra «¿Qué fue eso...?» y comprueba **Investigar el ruido del exterior**.
5. Sal con E y vuelve hacia la camioneta, al oeste del puente. Revisa su tamaño, el techo aplastado, el tronco/copa, los bordes del alfa, las ramas y el orden visual al caminar por delante/detrás. Intenta cruzar el tronco y las raíces sobre la carretera: deben impedir el paso.
6. Acércate por el lado del sendero al punto de descubrimiento. Termina las ocho líneas; el objetivo debe pasar a **Localizar el generador auxiliar**. Alejarte y volver no debe repetirlas.
7. Vuelve a entrar y salir; después sigue el sendero al generador y regresa. El accidente debe conservarse, sin cerrar el puente, la puerta ni el acceso del servicio. Comprueba que la batería no se reinicie y que Tab conserve su estado.
8. Completa la misión existente del fusible/generador y verifica que el accidente no haga retroceder el objetivo. Detén e inicia Play: la camioneta debe estar intacta otra vez.

La validación automatizada se ejecuta sobre una copia aislada en `Temp/CameraVerificationUnity`. Los resultados de la etapa están en `stage6-results.txt`; `Temp` queda excluido de Git. Las pruebas en modo batch no equivalen a una revisión visual ni a escuchar los efectos en los altavoces. No se realizaron commits ni pushes.

## Verificación realizada

La compilación de la copia en Unity 6000.6.3f1 terminó sin errores de C#. Las pruebas de Play completaron la radio y ambos diálogos mediante `NarrativeDirector.HandleModalInput`, ejecutaron la secuencia real y viajaron mediante `RetenTravel` entre exterior, interior, exterior, interior, exterior, generador y exterior.

Resultados comprobados:

- Inicio de Play con indicadores reiniciados, camioneta intacta y colliders del accidente inactivos.
- Entrada al interior sin activar el accidente; impacto únicamente después de cerrar la radio.
- Clips provisionales creados, dos luces interiores referenciadas, restauración de sus intensidades y restauración de la proyección sin desplazar el Transform de la cámara.
- Reacción correcta, restauración del movimiento y aparición del árbol al salir.
- Collider original de la camioneta conservado y raycast de Physics2D bloqueado por las raíces/tronco sobre la carretera.
- Búsqueda de rutas con consultas reales de Physics2D y una envolvente conservadora del collider de Leo: acceso al puente, al retén, al sendero del generador y al punto de descubrimiento.
- Descubrimiento por proximidad, ocho líneas, objetivo actualizado al terminar y ausencia de repetición tanto al permanecer cerca como tras volver del generador.
- Persistencia del accidente, objetivos, carga de batería y encendido de linterna entre escenas; batería que vuelve a consumir al encenderla; visibilidad de Tab conservada.
- Seguimiento de cámara después del accidente y después del regreso del generador.
- Modelo de misión: guardas de repetición, imposibilidad de saltar al generador sin descubrir el accidente, recogida anticipada del fusible después del descubrimiento, reparación y anomalía existentes sin regresión de objetivos.

Comparación por documentos YAML: los 1631 bloques anteriores del exterior y los 186 del interior permanecen idénticos, excepto la incorporación necesaria de una raíz a SceneRoots. No hay referencias locales incompletas ni IDs duplicados. Los scripts y campos de cámara no cambiaron.

Pendiente: revisión visual en Game del tamaño y oclusión del arte compuesto, intensidad perceptual de la sacudida/parpadeo, escucha y volumen de los sonidos provisionales y controles físicos WASD/E/F/Tab. Los controles no fueron simulados con un teclado: las pruebas invocaron el sistema de diálogo y posicionaron al personaje para las comprobaciones. La búsqueda de rutas no equivale a recorrerlas manualmente. Los efectos y el arte son provisionales; no existe guardado permanente.

Los archivos de configuración de renderizado/Player/Build Settings que ya diferían del commit antes de las ediciones de esta etapa se conservaron sin revertirlos. La implementación no cambia esas configuraciones.
