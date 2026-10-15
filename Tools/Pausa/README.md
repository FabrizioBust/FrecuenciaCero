# Validación de pausa

Estos helpers no pertenecen a `Assets` y no forman parte del juego. Nunca los copies al proyecto principal para una entrega.

1. Trabajar sobre una copia de `Assets`, `Packages` y `ProjectSettings` con Unity 6000.6.3f1. Cambiar `companyName`/`productName` solo en la copia para aislar PlayerPrefs. Conservar las mismas versiones de paquetes.
2. Copiar `PauseValidationEditor.cs` a `Assets/Editor` de la copia y `PauseValidationRuntime.cs` a `Assets` de la copia.
3. Ejecutar Unity sobre esa copia con `-executeMethod PauseValidationEditor.Play -logFile <log-local>`. Utilizar un editor gráfico oculto para que Input System procese teclado/ratón sintéticos correctamente; no usar `-batchmode` para esta prueba. El helper termina el proceso después de guardar el resultado.
4. Para la regresión del capítulo, copiar `Tools/AuditoriaCapitulo1/ChapterAuditEditor.cs` a `Assets/Editor` y `ChapterRegressionRuntime.cs` a `Assets/ChapterAuditRuntime.cs` de la copia. Ejecutar `-executeMethod ChapterAuditEditor.Play`. La única adaptación respecto de la auditoría anterior es probar clic derecho para cancelar apuntado, porque ESC ahora abre pausa.
5. Para compilar Windows x64, ejecutar `-batchmode -quit -executeMethod PauseValidationEditor.Build` sobre la copia. Se necesitan los módulos Windows ya instalados del editor; no instala dependencias.

La copia utilizada durante esta tarea es `Temp/AuditoriaCapitulo1Unity`. Los resultados específicos de pausa quedan en `Temp/PausaValidacion`; los resultados del helper de auditoría quedan en `Temp/AuditoriaCapitulo1`. Los informes finales se conservan en `Docs/PausaYControles`.

Las pruebas usan teletransportes, inventario controlado y dispositivos sintéticos de Input System solo dentro del proceso de prueba. No cambian los dispositivos de Windows ni el editor principal. Se aíslan errores reconocibles de servicios del editor y se registran como `EDITOR_INFRA`; las excepciones del juego cuentan como fallos. No publicar logs completos del editor: pueden contener datos de servicios y licencias.
