# Área de generadores — pulido visual y funcional

## Instalación de mantenimiento

Se conservan los senderos, cobertizo, almacén, postes, generador, fusible y objetivos existentes. Se añaden cercas envejecidas, materiales de mantenimiento, una mesa lateral, tablones bajo la máquina, piedras, maleza y dos franjas de bosque alrededor del recinto. Los espacios de trabajo y recorridos se mantienen libres; las nuevas franjas de árboles evitan los senderos existentes.

Los recursos principales reutilizan los atlas del exterior y del interior del retén. El cableado ahora cuelga entre los postes, con una pequeña caída entre apoyos. La luz ambiental del mapa aumenta de 0,45 a 0,55 para mantener la exploración nocturna legible sin linterna, incluso tras el fallo de la farola. No se cambian las cámaras ni la serialización de sus límites.

La señal de regreso se desactiva únicamente en la escena del generador. Una reja con los mismos recursos que el acceso del exterior conserva `PlayerInteraction`, E, `GeneratorAccessGate`, su apertura/fundido y `RetenTravel`. Sigue usando `exterior_servicio` y el punto actual de llegada del exterior; no requiere llaves ni añade condiciones de misión. El punto de aparición original del generador se conserva.

## Farola de entrada

`GeneratorEntryLamp` reutiliza la luz `Farol_Acceso_Existente` y el poste próximo a la entrada. Inicialmente emite luz cálida de alcance moderado. Al aproximarse Leo a 1,35 unidades del poste, registra `EntryLampFailureStarted` en la misma instancia de `MissionProgress` y ejecuta intervalos irregulares de parpadeo durante aproximadamente 2,8 segundos.

Después permanece apagada. Salir y regresar no repite el evento; abandonar la escena durante el parpadeo conserva el fallo. Cuando `GeneratorRepaired` pasa a verdadero, la farola se enciende de manera estable, también en visitas posteriores. Una pequeña malla oscurece los píxeles luminosos del cristal mientras falla, sin editar el sprite original. No se añaden sonidos, voces ni sucesos sobrenaturales.

## Caja, apertura y fusible

La caja metálica se integra en el área de repuestos, cerca de la estantería. Sus partes reutilizan recortes del atlas original; no se reemplazan ni alteran los sprites de origen. El fusible conserva su recurso original.

1. **Examinar:** E abre un diálogo de una línea mediante `NarrativeDirector`: «Una caja de herramientas. Parece que todavía queda algo dentro.». `ToolboxExamined` se registra al cerrar el mensaje, sin cambiar el objetivo.
2. **Abrir:** la siguiente E registra `ToolboxOpened`. `ToolboxVisual` separa y eleva la tapa durante 0,45 segundos, descubre el interior y muestra el fusible. La recogida permanece bloqueada mientras se abre.
3. **Recoger:** E llama al `RetenMission.CollectFuse` existente. El objeto desaparece, `HasFuse` usa los mismos datos del inventario/progreso y el objetivo pasa a instalarlo. La caja queda abierta y vacía. La recogida no se puede repetir.

Los tres indicadores nuevos pertenecen a `MissionProgress`, no a un inventario paralelo. Se conservan entre escenas y se reinician con el mismo reinicio de Nueva partida. Se mantienen el examen del generador, su diálogo de instalación, consumo del fusible, luces indicadoras, vibración inicial y objetivo de regreso existentes. El diálogo posterior original se conserva sin reescribirlo ni agregar otro evento.

## Contornos y documentos

No existía un contorno para objetos de mundo. `InteractionOutline` es compartido por la caja cerrada, sus partes abiertas, la bitácora y el mapa. Reutiliza `InteractionProbe.CanReach` y las condiciones del `WorldInteraction` correspondiente. Solo se dibuja al estar dentro del alcance y con la interacción disponible; se oculta al alejarse, durante los modales o al finalizar la recogida.

El shader dibuja un borde exterior fino mediante el canal alfa y muestras vecinas, limitado a los UV del objeto para evitar artefactos del atlas. No dibuja un marco genérico ni cambia los materiales originales. La bitácora pintada dentro del escritorio usa una silueta invisible alineada a sus páginas; no se duplica la ilustración. El mapa usa su sprite existente. Los contenidos, ilustraciones y controlador de lectura de ambos documentos permanecen intactos.

## Archivos

Modificados:

- `Assets/Scenes/Nivel01/N1_Generador.unity`.
- `Assets/Scenes/Nivel01/N1_InteriorReten.unity`: únicamente incorporación de los contornos de documentos.
- `Assets/Scripts/Narrative/MissionProgress.cs`: indicadores de presentación idempotentes, sin nuevas misiones.
- `Assets/Scripts/World/FusePickup.cs`: secuencia opcional de caja que sigue usando la recogida existente.

Añadidos, junto con sus `.meta`:

- `Assets/Scripts/World/ToolboxVisual.cs`.
- `Assets/Scripts/Lighting/GeneratorEntryLamp.cs`.
- `Assets/Scripts/Interaction/InteractionOutline.cs`.
- `Assets/Editor/GeneratorPolishBuilder.cs`: autoría específica que rechaza sobrescribir una instalación ya preparada.
- `Assets/Narrative/Nivel01/CajaHerramientas_Examinar.asset`.
- `Assets/Art/Nivel01/GeneratorPolish/`: partes de caja, cristal oscuro de farola, material/shader de contorno y silueta de la bitácora.
- Este documento.

La comparación SHA-256 de todos los archivos preexistentes de `Assets` y `ProjectSettings` detecta cambios únicamente en los cuatro archivos indicados. El exterior del retén, sus rejas/cámara, menú, introducción, movimiento/animaciones de Leo, linterna, batería, HUD, diálogos originales, accidente, atlas originales y configuración del proyecto permanecen sin modificaciones. Los archivos aplicados coinciden con la copia validada. No se realizan commits ni pushes.

## Pruebas ejecutadas

**72 comprobaciones superadas en Play Mode de Unity 6000.6.3f1**, en una copia con preferencias aisladas:

- Entrada desde la reja del exterior mediante el controlador real de E y llegada original libre de colisiones.
- Caminatas con teclado del Input System y Physics2D entre entrada, generador, caja y regreso, respetando los colliders.
- Farola encendida al entrar, fallo por proximidad, intervalos de distinta intensidad, apagado estable, cristal sin brillo y ausencia de repetición.
- Exploración y capturas sin linterna; batería de 63 % conservada durante toda la secuencia y los viajes.
- Contorno al acercarse a la caja, desaparición al alejarse o abrir un modal, examen, apertura, bloqueo durante la animación y fusible visible.
- Recogida única, objetivo de instalación y caja vacía persistente al regresar.
- Reparación con el diálogo original, consumo del fusible, luces indicadoras y encendido estable de la farola.
- Regreso al exterior e interior, documentos originales abiertos y cerrados, progreso de lectura y contornos coherentes, sin retroceder objetivos ni accidente.
- Nueva visita al generador reparado con farola estable, caja abierta vacía y diálogo posterior original sin repetición.
- Sin errores nuevos de compilación, shader ni ejecución. La excepción de indexación de `UnityEditor.Search.SearchDatabase` que ya existía se distingue de los errores del juego.

Informe: `Temp/GeneradorPulido/Pruebas.txt`. Comparación: `Comparacion.json`. Capturas URP revisadas en 16:9 y 4:3: `instalacion-completa.png`, `instalacion-4x3.png`, `entrada-encendida.png`, `entrada-apagada.png`, `entrada-reparada.png`, `caja-cerrada-contorno.png`, `caja-abierta-fusible.png`, `caja-vacia.png`, `bitacora-contorno.png` y `mapa-contorno.png`.

## Revisión manual pendiente

Las capturas de RenderRequest muestran el mundo y los contornos, pero no la interfaz IMGUI de diálogos/documentos. La prueba valida su apertura, contenido, cierre y progreso mediante el controlador existente. Queda revisar visualmente el texto, el desplazamiento de documentos y la sensación de las animaciones con teclado/ratón físicos en Game View y en un ejecutable Windows. No se ha generado ni probado un ejecutable en esta tarea.
