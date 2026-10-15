# Repetir la auditoría

Estas clases son herramientas de pruebas, fuera de Assets del proyecto principal. No añadirlas a las escenas jugables.

La copia usada está en `Temp/AuditoriaCapitulo1Unity`. Para repetir con esa copia:

1. Copiar `ChapterAuditRuntime.cs` a `Assets/` de la copia, y `ChapterAuditEditor.cs` a su `Assets/Editor/`.
2. Mantener versiones idénticas de Assets, Packages y ProjectSettings; durante Play Mode usar companyName/productName exclusivos de pruebas para aislar PlayerPrefs. Los paquetes file: de la copia apuntan a las mismas versiones de Library/PackageCache del original.
3. Abrir la copia con Unity 6000.6.3f1 y `-executeMethod ChapterAuditEditor.Play`. El arnés cierra sólo esa instancia al finalizar y escribe `Temp/AuditoriaCapitulo1/PruebasPlayMode.txt`.
4. `ChapterAuditEditor.Inspect` abre todas las escenas habilitadas sin guardarlas y registra referencias, cámaras y llegadas. Puede ejecutarse con `-batchmode -quit`.
5. `ChapterAuditEditor.Build` verifica niveles de clips y crea el Build de desarrollo de Windows en `Builds/AuditoriaCapitulo1Windows`. Restaurar la identidad original del producto en la copia antes de compilar. No modifica el proyecto principal.

El arnés usa Input System con dispositivos sintéticos y preferencias aisladas, y reubica a Leo para las interacciones. No es una partida manual ni una prueba de comodidad de todos los caminos. Prepara datos para casos independientes y distingue excepciones conocidas del editor de errores de gameplay. No ejecutarlo en el proyecto principal para preservar las preferencias y la sesión del usuario.

Las carpetas Temp y Builds están excluidas de Git. Conservar los resultados pequeños del informe; no publicar logs completos del editor, que pueden incluir datos de servicios/licencia.
