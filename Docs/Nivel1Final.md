# Nivel 1 — El Sendero del Retén

Implementación en Unity 6000.6.3f1, URP 2D. Abrir `Assets/Scenes/MainMenu.unity` y pulsar Nueva partida para recorrer el nivel completo. El menú sigue siendo la primera escena del Build.

## Recorrido y objetivos

Radio del retén → accidente del árbol y camioneta → investigar el exterior → reja del generador → examinar el generador → examinar y abrir la caja → recoger el fusible → reparar → cerrar el diálogo posterior → sendero sur → campamento → Claro de las Raíces → primera aparición y tutorial → evadir → salida segura → **cierre del capítulo I** → **CONTINUAR al capítulo II**.

El nuevo objetivo después del diálogo del generador es «Seguir el sendero sur hacia el campamento forestal». E en el sendero antes de esa etapa muestra un aviso y permite regresar. La reja de vuelta al retén continúa funcionando.

El campamento y el claro están en una sola escena: `Assets/Scenes/Nivel01/N1_BosqueAbandonado.unity`. La entrada está al norte, el campamento en torno a (-8, 8), el primer encuentro cerca de (-2, -1) y la salida segura en (11, -10). Se añadieron bosque, vegetación baja, raíces, terreno removido, refugio deteriorado, troncos apilados, mesa, carretilla, materiales y nota. La escena tiene colisiones físicas perimetrales, cámara limitada y luz ambiental independiente de la linterna.

No se añadieron la Quebrada del Silencio, un escenario de puente, cuevas, túneles ni otro encuentro obligatorio. El decorado preexistente del exterior se conserva.

## Controles

| Acción | Control |
| --- | --- |
| Caminar | WASD |
| Correr | Shift + WASD |
| Interactuar / continuar tutorial | E; Enter también continúa tutorial y diálogos |
| Encender o apagar linterna | F |
| Usar una batería de repuesto | R, cuando la carga no está completa |
| Usar un botiquín portátil | H, si Leo está herido y tiene existencias |
| Entrar en apuntado | Q, con una botella disponible |
| Mover la mira | Ratón |
| Lanzar | Clic izquierdo, dentro del alcance |
| Cancelar apuntado | Clic derecho o Escape |
| Mostrar / ocultar HUD existente | Tab |
| Reintentar tras morir | Enter |
| Continuar al capítulo II desde el cierre | Enter o clic en CONTINUAR |
| Omitir la introducción existente | Mantener Espacio |

Q detiene únicamente a Leo y muestra una mira; el mundo continúa. El alcance inicial es 8 unidades, configurable en `BottleAim`. La mira indica el impacto previsto mediante consultas de colisión 2D y se vuelve roja si el punto seleccionado excede el alcance. Cancelar o hacer clic fuera de alcance no consume. Un lanzamiento consume una botella y crea una trayectoria visible; al romperse emite ruido en el impacto real y no hace daño.

## Vida, inventario y recogidas

Se reutiliza `RetenMission.Session`: `MissionProgress.Survival` conserva tres corazones máximos, hasta cinco baterías, hasta diez botellas, hasta tres botiquines portátiles inicialmente y un conjunto de identificadores únicos recogidos. La capacidad de botiquines se configura en `Assets/Resources/FrecuenciaCero/InventorySettings.asset`. No hay otro administrador persistente ni un jugador adicional.

| Recurso | ID / ubicación |
| --- | --- |
| Primera batería | `generador_bateria_01`, (-8.1, 3.8), junto a la caja del fusible |
| Segunda batería | `generador_bateria_02`, (-6.7, 3.8), junto a la misma caja |
| Botella | `generador_botella_01`, (-5.5, 3.5), zona de herramientas |
| Botiquín rojo | `campamento_botiquin_01`, (-7, 7), frente al refugio |

Las recogidas usan E y contorno. Un inventario lleno no consume el objeto. El botiquín se guarda sin curar, incluso con salud completa. H consume uno y recupera un corazón cuando Leo está herido; con salud completa o sin existencias muestra un aviso y no consume. Los estados bloqueantes impiden usarlo. La carga porcentual de la linterna y los repuestos son datos separados. R consume un repuesto para restaurar la carga, sin cambiar el encendido manual ni la degradación original de intensidad y alcance. A cero carga continúa la iluminación mínima existente.

El contacto con el Escarbador quita un corazón y activa un segundo de invulnerabilidad y un destello rojo. Al morir, se bloquea el movimiento y aparece la pantalla de reintento. Enter restaura tres corazones y reaparece en (-8, 8), lejos del escondite; el enemigo vuelve al escondite. Se conservan misión, consumos, recogidas y tutorial, evitando duplicaciones o un reintento que dependa de botellas.

## Ruido y Escarbador

`NoiseEvents` transporta posición, intensidad, radio y tipo. `MovementNoise` comprueba desplazamiento físico: caminar emite intensidad 0.45 con radio 6; correr, intensidad 1 con radio 13. Quieto no emite pasos. Los impactos de botella tienen radio 16. Los intervalos, radios y parámetros de IA son configurables.

El Escarbador no guarda un Transform de Leo ni consulta su posición para detectarlo. Sólo actualiza su objetivo mediante sonidos recibidos dentro del radio y por encima de su sensibilidad. Las consultas físicas sirven para navegar alrededor de obstáculos; no detectan al jugador por visión.

Sus estados son Oculto, Alerta, Investigar, Perseguir, Buscar y Retirarse. Los pasos continuos actualizan la persecución; al dejar de escucharlos investiga la posición recordada, busca durante cinco segundos y vuelve físicamente a su escondite. Los pasos se investigan con un pequeño margen y una búsqueda alrededor del sonido; una botella se investiga en su impacto. Puede volver a escuchar pasos después de una distracción. Su collider está alineado con el punto usado por la navegación y el trazado deja margen para las esquinas.

La apariencia es provisional: se creó un Sprite independiente usando la criatura encorvada que ya existe en el atlas del proyecto. El PNG y los sprites originales de Leo se conservaron. `crawlFrames` permite sustituir o ampliar la animación más adelante.

## Cinemática y cierre

La primera aparición bloquea temporalmente los controles, mueve suavemente la cámara, anima la tierra y revela gradualmente la criatura orientada hacia pasos registrados. Muestra «¿Qué... fue eso?», devuelve la cámara y presenta los dos textos exactos del tutorial. E o Enter continúa. El HUD de ruido se desbloquea en el tutorial.

La sesión registra la aparición y su finalización. Al regresar o recargar no se repite; un reintento es seguro. La retirada completa habilita el objetivo de salida. En la salida segura, con la criatura retirada, comienza ahora una cinemática de unos 18 segundos, seguida de la presentación **Capítulo I completado** y un botón **CONTINUAR** hacia el prólogo del capítulo II. La integración y sus pruebas se detallan en [CapitulosI_II.md](CapitulosI_II.md).

El sonido de tierra se sintetiza localmente y respeta el volumen de efectos. La finalización se registra en la sesión y en la clave `FrecuenciaCero.Progress.Level1Completed` de PlayerPrefs. `RootsEncounter.levelTwoReady` se conserva como evento de cierre; CONTINUAR usa el cargador existente para entrar en `N2_SenderoCabana`. Vida, inventario y linterna conservan su continuidad. El resto del progreso persiste durante la sesión; no se agregó un sistema completo de partidas guardadas en disco.

## Contornos y HUD

Se amplió `InteractionOutline` para sprites y la malla de las rejas. Usa la transparencia del recurso, sin recolorear sus píxeles, y se muestra únicamente para el objeto seleccionado por el controlador de E. Las partes de la caja y el fusible comparten su interacción; mapa y bitácora conservan sus siluetas. Se alineó una máscara de silueta sobre la radio integrada en el escritorio. Las recogidas y las interfaces bloqueantes ocultan los contornos correspondientes.

Los corazones ocupan la parte superior izquierda; debajo quedan la carga original y los contadores de repuestos. El objetivo mantiene la derecha y el ruido la esquina inferior derecha. Se ajustaron las posiciones de los avisos y del objetivo en vistas estrechas. La mira utiliza las coordenadas de pantalla del impacto en el mundo.

## Archivos

Escena creada: `N1_BosqueAbandonado.unity`, con su `.meta`. Escenas modificadas: `N1_Generador.unity` (sendero, recogidas, sistemas y contornos), `N1_ExteriorReten.unity` y `N1_InteriorReten.unity` (supervivencia y contornos). El menú y la introducción no fueron modificados.

Scripts nuevos:

- `Narrative/SurvivalProgress.cs`.
- `Player/NoiseEvents.cs`, `MovementNoise.cs`, `SurvivalPlayer.cs`, `BottleAim.cs`, `BottleProjectile.cs`.
- `Interaction/SupplyPickup.cs`, `ForestDocument.cs`.
- `World/ForestPassage.cs`, `ForestNavigation.cs`, `Escarbador.cs`, `RootsEncounter.cs`.
- `UI/SurvivalHUD.cs`.
- `Editor/LevelOneFinalBuilder.cs`, receta explícita de creación que rechaza sobrescribir el bosque existente.

Scripts existentes modificados: `MissionProgress`, `RetenMission`, `LeoMovement`, `NarrativeDirector`, `PlayerInteraction`, `InteractionOutline`, `BatteryUI`, `NarrativeUI` y `GeneratorTerrainField`.

Recursos nuevos en `Assets/Art/Nivel01/Survival`: terreno, batería, botella, botiquín, carretilla, siluetas, Sprite del Escarbador, nota y documentos. Se actualizó `SueloGenerador.png` para integrar el descenso y se agregó el bosque al Build conservando la lista anterior.

## Verificación de la implementación original

Estas comprobaciones corresponden a la primera implementación. La pasada posterior de niebla, interfaces y botiquines portátiles y sus pruebas actualizadas están documentadas en [PulidoVisualPortable.md](PulidoVisualPortable.md).

Pruebas nativas en Unity 6000.6.3f1, en una copia de verificación, con teclado y ratón del Input System, Physics2D y URP 2D:

- **84 comprobaciones integrales aprobadas**: recogidas con E, persistencia, ruido y correr, apuntado, cancelaciones, consumo, impactos, nota, cinematografía y tutorial, ceguera y última posición escuchada, navegación alrededor de una pared, daño físico, invulnerabilidad, muerte, checkpoint, viajes, salida recorrida con WASD sin botellas, espera en silencio, final guardado, menú, Nueva partida y omisión de introducción.
- **72 comprobaciones de regresión del generador aprobadas**: caminar por la instalación, rejas, farola y cristal, caja cerrada/examinada/abierta/vacía, fusible, reparación, diálogo posterior, batería de linterna, viajes y documentos originales. Se actualizó únicamente la expectativa del objetivo posterior para reflejar el nuevo sendero sur.
- **68 comprobaciones de orientación aprobadas**: cuatro direcciones, diagonales, cuatro fotogramas laterales, reposo, velocidad física original y linterna apuntada de forma independiente.
- Auditoría de las seis escenas habilitadas: cero scripts faltantes y todos los interactuables activos con contorno asignado.
- Capturas revisadas del campamento y claro en 1920×1080 y 1280×960; captura real del Game View con HUD en 2560×1440.
- Comparación SHA-256 contra el inicio de la tarea: ningún archivo preexistente eliminado. Menú, introducción, atlas original de Leo, animador, scripts de cámara y configuración URP conservados.

Los informes están en `Temp/Nivel1Final/Pruebas.txt`, `Pruebas-RegresionGenerador.txt`, `Pruebas-RegresionLeo.txt` y `Comparacion.json`; capturas en la misma carpeta. El arnés y la copia de Unity quedan en Temp, fuera de Assets del proyecto original.

No hubo nuevos errores de compilación, shaders o gameplay en las pruebas aprobadas. La copia muestra un error previo del índice de búsqueda del editor (`UnityEditor.Search.SearchDatabase`), separado de las comprobaciones de gameplay.

Pendiente de revisión humana: comodidad del recorrido y del balance, percepción del audio en los altavoces del equipo y ejecución de un Build final de Windows. El arte del enemigo es provisional y aún no tiene una animación definitiva de varias poses. No se hicieron commits ni pushes.
