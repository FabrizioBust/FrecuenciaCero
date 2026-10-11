# Frecuencia Cero — Ajuste fino de Leo, HUD y TAB

## Ajustes sobre la versión actual

Leo pasa de **200 a 181.81818 Pixels Per Unit** en `Leo_Exterior.png.meta`. La relación `200 / 181.81818 ≈ 1.10` aumenta su anchura y altura visual un **10 % adicional** respecto al estado inmediatamente anterior. No se reaplicó el aumento previo del 28 %. Los 16 frames usan el mismo PPU, con pivotes, recortes y referencias de animación intactos. Se mantienen Point, sin mipmaps y sin compresión.

Leo no tiene un hijo exclusivamente visual: SpriteRenderer y animación están en la raíz. Por eso se ajustó únicamente el tamaño importado del sprite. El Transform sigue en `(1,1,1)`, y Rigidbody2D, CapsuleCollider2D, movimiento, YSort, cámara y Pixel Perfect no se modificaron. La linterna mantiene su origen local `(0,0.2,0)` y su propia escala `(1,1,1)`. Su alineación visual con el sprite aumentado requiere comprobación en Game.

El HUD sigue usando IMGUI; no hay Canvas Scaler ni TextMeshPro en estas interfaces. `HudLayout.SizeMultiplier = 1.12` aumenta **un 12 %** los tamaños actuales del HUD, aplicándolo una sola vez a la escala calculada de dibujo. No se acumula por frame, al pulsar TAB ni al cambiar de escena. La referencia permanece en 1280×720.

Medidas configuradas a 1280×720:

| Elemento | Antes de este ajuste | Ahora |
| --- | --- | --- |
| Panel batería/linterna | 208×62 px | 232.96×69.44 px |
| Barra de batería | 184×5 px | 206.08×5.6 px |
| Texto batería/porcentaje | 13 px | 14.56 px efectivos |
| Texto linterna | 12 px | 13.44 px efectivos |
| Indicador F | 11 px | 12.32 px efectivos |
| Panel objetivo: ancho | 320 px | 358.4 px |
| Texto objetivo | 14 px | 15.68 px efectivos |
| Encabezado/aviso | 11 px | 12.32 px efectivos |
| Margen exterior | 16 px | 17.92 px |

Son medidas derivadas de la configuración, no mediciones de una captura del Editor. Los paneles conservan sus esquinas y su respuesta a ventanas estrechas. El objetivo continúa adaptando la altura al texto. Diálogos, documentos, mapa y mensajes contextuales mantienen exactamente sus estilos, tamaños y referencia de escala previos.

## TAB y persistencia

La acción local `<Keyboard>/tab` se añadió al `PlayerInteraction` que ya existe en cada escena. Usa el Input System configurado, igual que E. Cada pulsación nueva alterna el HUD; mantener TAB no genera alternancias repetidas. También se exige liberar la tecla al cambiar de escena, para que una tecla mantenida durante la carga no vuelva a alternar por error.

`HudVisibility` conserva un único booleano de sesión. La visibilidad inicial es `true` y se reinicia al comenzar una nueva partida/Play mediante `SubsystemRegistration`, incluso con Domain Reload desactivado. Las transiciones de escena no lo reinician. No hay GameObjects persistentes nuevos, `DontDestroyOnLoad`, escritura en disco ni duplicación de Canvas/controladores.

Al estar oculto, `BatteryUI` omite solamente su dibujo; `NarrativeUI` omite únicamente el panel del objetivo y su notificación. Sus componentes siguen habilitados. No se deshabilitan luces, descarga de batería, misión, movimiento ni interacción. Los documentos, diálogos y mensajes de E permanecen disponibles. TAB funciona también durante una interfaz narrativa y no la cierra ni avanza.

El objetivo se sigue actualizando mientras está oculto; al mostrar el HUD vuelve a dibujarse el objetivo actual. El aviso mantiene su duración original, así que un aviso cuyo tiempo terminó mientras estaba oculto no se reproduce de nuevo al mostrarlo.

## Archivos

Modificados:

- `Assets/Art/Nivel01/Characters/Leo_Exterior.png.meta`
- `Assets/Scripts/UI/HudLayout.cs`
- `Assets/Scripts/UI/BatteryUI.cs`
- `Assets/Scripts/UI/NarrativeUI.cs`
- `Assets/Scripts/Interaction/PlayerInteraction.cs`

Creados:

- `Assets/Scripts/UI/HudVisibility.cs` y su `.meta`.
- Esta guía.
- Copias previas de los cinco archivos modificados y de las dos escenas en `Docs/Backups/AjusteFinoHUDTab/`.
- Instantánea de hashes, respuesta de compilación, DLL y script/resultados de verificación en `Temp/FrecuenciaCeroAjusteFinoHUDTab/`.

`N1_ExteriorReten.unity` y `N1_InteriorReten.unity` no se reconstruyeron ni modificaron. Los ajustes se aplican mediante sus scripts y sprites compartidos. No se alteraron historia, diálogos, documentos, objetivos ni configuraciones generales.

## Prueba en Unity

1. Vuelve a Unity y espera la importación del atlas y la compilación. Si es necesario, usa `Assets > Refresh`. No añadas componentes ni cambies escalas manualmente.
2. Abre `Assets/Scenes/Nivel01/N1_ExteriorReten.unity`. En Game selecciona **1280×720**. Revisa `Window > General > Console` y presiona Play; haz clic en Game para dar foco al teclado.
3. Comprueba que el HUD empieza visible y es ligeramente mayor que antes. Observa a Leo junto a la camioneta, árboles y puerta; revisa la nitidez y el origen visible del cono de luz al apuntar en varias direcciones. Las cámaras deben conservar el encuadre anterior.
4. Pulsa TAB: batería, porcentaje, indicación F, misión y aviso deben desaparecer. Mantén TAB unos segundos: el estado no debe cambiar otra vez. Suelta y vuelve a pulsar: debe reaparecer. Repite varias veces.
5. Con la linterna encendida, oculta el HUD y camina durante unos segundos. Muévela con el mouse y prueba F. Vuelve a mostrar el HUD: la batería debe reflejar la descarga real, sin volver a 100 %. Recuerda que la descarga sucede mientras la linterna esté encendida, como antes.
6. Oculta el HUD en el exterior y entra al retén con E. Debe seguir oculto en el interior, y el mensaje contextual de la puerta debe seguir disponible.
7. Con el HUD oculto, abre la radio o un documento. Deben aparecer sus paneles con el tamaño anterior, bloquear/restaurar movimiento como antes y permitir avanzar/cerrar con los controles existentes. TAB durante estos paneles debe cambiar solo los indicadores de las esquinas.
8. Muestra el HUD dentro del retén y sal con E. Debe permanecer visible fuera, con el objetivo y la batería conservados. La misión no debe repetir eventos ya completados en esa sesión.
9. Detén Play y vuelve a iniciarlo. El HUD debe volver a estar visible, aunque la sesión anterior terminara con él oculto.

No se necesita configuración adicional en Inspector ni en Build Profiles para estos cambios.

## Verificación realizada

- Compilación independiente de todos los scripts de `Assets/Scripts` y `Assets/Editor` con Roslyn y las bibliotecas locales de Unity 6000.6.3f1: **sin errores ni advertencias**.
- Pruebas aisladas ejecutadas sobre el código real de `HudVisibility` y el filtro de pulsaciones: sesión nueva visible, primera/segunda pulsación, 120 frames manteniendo la tecla, tecla mantenida al llegar, reemplazo del filtro de entrada sin perder visibilidad, diez alternancias posteriores y reinicio de sesión. Solo se sustituyó el atributo de Unity para poder compilar este modelo fuera del motor; no se simuló el renderizado ni una carga real de escena.
- Comparación de los 263 archivos preexistentes: 258 permanecen iguales y únicamente cambiaron los cinco archivos previstos. Ambas escenas son idénticas a sus copias previas.
- Los métodos de diálogos/documentos, la lógica de objetivo/notificación y el enrutamiento previo de E/Enter/Escape permanecen iguales. La batería, la linterna, el movimiento, los datos narrativos, la niebla, la física y la configuración de proyecto no se modificaron.
- Cambio del atlas limitado a PPU: los 16 sprites conservan sus píxeles, IDs y pivotes; el aumento es uniforme y relativo a 200 PPU.

**No se ejecutó Unity Play ni se revisó visualmente el resultado en el Editor.** Quedan pendientes la importación efectiva, las pulsaciones reales, la persistencia durante la transición asíncrona, el consumo con HUD oculto y la alineación visual de la linterna. La compilación y las pruebas aisladas no sustituyen esas comprobaciones.

Referencias de API consultadas: [InputAction](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.InputAction.html) y [RuntimeInitializeOnLoadMethod](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html).
