# Acceso cercado al generador — Frecuencia Cero

El acceso del exterior ahora utiliza una puerta de malla metálica oxidada y un sendero curvo hacia la derecha. La composición adapta la imagen conceptual al retén existente; no incorpora la imagen como fondo.

## Implementación

- `N1_ExteriorReten.unity`: desactiva exclusivamente `Senal_Camino_Servicio` y su sendero anterior. Conserva el resto de los letreros, la caseta y las luces. Añade dos tramos fijos y una hoja móvil, sus colliders de suelo, vegetación de borde y el camino de servicio.
- La reja reutiliza `Cerca_Metal` del atlas original. Su geometría proyecta la cerca en diagonal manteniendo los postes verticales y conservando las coordenadas de textura. El nuevo shader usa la iluminación de URP 2D y no modifica materiales compartidos.
- El camino es una textura local a 32 PPU, con filtro Point, bordes transparentes e irregulares y una curva Catmull–Rom. Mantiene el terreno existente debajo y continúa hacia el bosque a la derecha. Se recolocan únicamente árboles próximos cuyo tronco o copa obstruían ese corredor; no se eliminan árboles ni recursos.
- `GeneratorAccessGate.cs` es un `WorldInteraction` de la lista existente de `PlayerInteraction`. Muestra exactamente **[E] Abrir reja** cuando `InteractionProbe.CanReach` confirma distancia, lado de aproximación y ausencia de obstáculos. También valida el alcance al ejecutar la interacción.
- La activación reserva inmediatamente la puerta y detiene temporalmente el movimiento. La hoja se abre mediante una proyección sencilla alrededor de la bisagra durante 0,32 segundos, seguida de un fundido de 0,22 segundos. La carga utiliza exclusivamente `RetenTravel.Begin`; no se añaden llaves, fusibles ni condiciones de misión.
- `GeneratorGateFade.cs` mantiene únicamente la cubierta visual durante la carga; después revela la escena durante 0,25 segundos y se destruye. Si falla la carga, se restaura la puerta y el movimiento.
- `generador_entrada` y el punto de llegada del mapa del generador permanecen intactos. El regreso utiliza el `RetenDoor` existente y `exterior_servicio`, cuyo punto actual en el exterior es `(17.7, 1.85)`, fuera de los colliders de la reja.

## Archivos

Modificado: `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`.

Añadidos, con sus archivos `.meta`:

- `Assets/Scripts/World/GeneratorAccessGate.cs`.
- `Assets/Scripts/World/GeneratorGateFade.cs`.
- `Assets/Editor/GeneratorAccessBuilder.cs`, constructor específico que rechaza reconstruir un acceso ya existente.
- `Assets/Art/Nivel01/GeneratorAccess/Reja.asset`, `Reja.mat` y `RejaIluminada.shader`.
- `Assets/Art/Nivel01/GeneratorAccess/CaminoServicio.png`.
- Este documento.

El mapa del generador, interior, menú, introducción, scripts de movimiento, HUD, batería, linterna, objetivos, accidente y sistema de transiciones se conservan. No se cambian los límites de cámara ni su serialización.

## Validación

La prueba se ejecuta en Play Mode de una copia temporal de Unity 6000.6.3f1, con preferencias aisladas. Usa Physics2D para encontrar el recorrido y estados de teclado del Input System aplicados antes del controlador existente; Leo recorre físicamente el trayecto desde la puerta del retén hasta la reja.

Comprueba alcance, indicador contextual, apertura con E, prevención de cargas duplicadas, fundido y limpieza, llegada al generador sin colisiones y regreso mediante E. Repite el viaje con partida inicial y con accidente y descubrimiento completados; verifica la misma instancia de partida, los objetivos, carga de batería, linterna apagada y encendida, HUD y ausencia de duplicados. La segunda vuelta conserva la camioneta destruida.

Informe: `Temp/AccesoGenerador/Pruebas.txt`. Capturas del mundo renderizadas con URP a 1920 × 1080 y 1280 × 960: `acceso-16x9.png` y `acceso-4x3.png`. Las capturas de RenderRequest no incluyen la interfaz IMGUI; el texto contextual se valida en `NarrativeDirector.ContextPrompt` y usa el HUD existente.

Las 53 comprobaciones finales pasaron. No hay errores nuevos de compilación, shader ni ejecución. La comparación SHA-256 de los archivos preexistentes en `Assets` y `ProjectSettings` confirma que solo cambió `N1_ExteriorReten.unity`; los archivos nuevos y la escena aplicados al proyecto coinciden con la copia validada.

La excepción previa de indexación del editor (`UnityEditor.Search.SearchDatabase`, `ArgumentOutOfRangeException`) se registra aparte. No se alteran paquetes ni cachés del proyecto para ocultarla.

## Revisión manual

1. Abrir el exterior y caminar desde la puerta del retén hacia la reja por el sendero.
2. Confirmar la legibilidad de **[E] Abrir reja** en Game View, acercándose y alejándose.
3. Pulsar E repetidamente: observar la apertura, el fundido y una única llegada al generador.
4. Regresar por el acceso original y comprobar posición, cámara, batería, linterna y objetivo. Repetir después del accidente.
5. Revisar visualmente la animación y el encuadre en un ejecutable Windows.

No se realizan commits ni pushes.

## Pulido de posición y orientación de Leo

La estructura de la reja se desplazó de `(14.5, 0.65)` a `(19.3, 1.75)`: 4,8 unidades hacia la derecha y 1,1 hacia arriba, siguiendo el sendero existente. Se conservan el terreno curvo y la vegetación en sus posiciones; se desplazan juntos los tres colliders de la cerca, la hoja móvil y la bisagra. El punto de interacción pasa a `(18.8, 1.22)`. No fue necesario ampliar el rectángulo de cámara ni cambiar el borde físico del escenario.

`GeneratorAccessBuilder.RepositionExisting` reproduce ese ajuste sin crear otra reja ni regenerar recursos. El constructor inicial también usa las coordenadas nuevas. La corrección de animación y las pruebas de este pulido se documentan en `Docs/PulidoRejaLeo.md`.
