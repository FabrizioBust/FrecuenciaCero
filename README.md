# Frecuencia Cero

Videojuego de **terror psicológico y supervivencia** en desarrollo, con exploración 2D de un bosque y un retén, interacciones narrativas y gestión de la batería de una linterna.

- **Motor:** Unity 6, versión **6000.6.3f1**.
- **Plataforma objetivo:** Windows.
- **Estado:** prototipo del primer nivel en desarrollo; no es una versión final.

## Avance actual

El proyecto contiene movimiento y animación de Leo, cámara de seguimiento, iluminación y niebla, linterna con consumo de batería y HUD. Incluye escenas del exterior del retén, su interior y el generador auxiliar, con desplazamiento entre ellas, diálogos de radio, lectura de documentos y objetivos. La misión del generador permite examinarlo, recoger un fusible e instalarlo para repararlo.

El progreso de misión y la carga de la linterna se conservan durante los cambios de escena de una partida. Iniciar una nueva sesión de Play reinicia el progreso; todavía no existe guardado entre ejecuciones. Este repositorio contiene el proyecto fuente, no una build de Windows. La revisión para preparar GitHub no incluye pruebas de ejecución en Unity.

## Abrir en Unity Hub

1. Clona o descarga el repositorio; si descargaste un ZIP, extráelo.
2. Instala **Unity 6000.6.3f1** desde Unity Hub. Para compilar para Windows, instala también el módulo de Windows Build Support correspondiente.
3. En la sección de proyectos de Unity Hub, utiliza la opción para agregar un proyecto desde disco y selecciona la carpeta que contiene `Assets`, `Packages` y `ProjectSettings`.
4. Abre el proyecto con esa versión y espera a que Unity restaure los paquetes e importe los recursos. La carpeta `Library` se genera localmente.
5. Abre `Assets/Scenes/Nivel01/N1_ExteriorReten.unity` y pulsa **Play** para iniciar el recorrido del prototipo. Las escenas de interior y generador ya figuran en la lista de escenas del proyecto.

## Controles del prototipo

- **WASD:** movimiento.
- **Mouse:** apuntar la linterna.
- **F:** encender o apagar la linterna.
- **E:** interactuar y avanzar diálogos; **Enter** también avanza diálogos.
- **Tab:** mostrar u ocultar el HUD.

## Contenido del repositorio

`Assets` conserva escenas, scripts, arte, recursos y archivos `.meta`; `Packages` registra las dependencias y `ProjectSettings` conserva la configuración de Unity. `Docs` contiene notas del desarrollo y material de validación existente.

Los cachés de Unity, builds, ajustes personales del editor y copias de trabajo locales están excluidos mediante `.gitignore`. No es necesario compartir `Library`, `Temp` ni `UserSettings` para abrir el proyecto.
