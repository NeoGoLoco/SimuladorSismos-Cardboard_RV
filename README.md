*REQUERIMIENTOS TÉCNICOS Y GUÍA DE INSTALACIÓN - SISMO VR*
Proyecto: Simulador de Protección Civil en Realidad Virtual
Materia: Proyecto Integrador II (TSU en TIID - 501)
Fecha de última actualización: 15 de abril de 2026 (Ya para entregarlo a la Memoria)
Por: Edgar Adrián Vázquez Peñaloza.
-----------------------------------------------------------
1. ESPECIFICACIONES DEL MOTOR Y PLATAFORMA
------------------------------------------
- Versión de Unity: 6000.3.9f1 (Unity 6).
- Render Pipeline: Universal Render Pipeline (URP).
- Plataforma: Android (Móvil).
- API de Gráficos: OpenGL ES 3.0 (Borrar Vulkan si es que aparece).
- Nivel de API mínimo: Android 8.1 "Oreo" (API 27) -> Hasta el máximo que es Android 16.0 (API 36).
- Scripting Backend: IL2CPP con .NET Standard 2.1.

2. PAQUETES Y LIBRERÍAS (Package Manager)
------------------------------------------
Es muy necesario tener instalados los siguientes módulos:
- Google Cardboard XR Plugin: Para visión estereoscópica.
- Input System (1.x): Para el soporte de control Bluetooth.
- XR Plugin Management: Configurado para Android/Cardboard.
- TextMeshPro: Para interfaces e instrucciones flotantes.
- ProBuilder: Para colisiones y edición de geometría.

2.5. CONFIGURACIÓN DEL GOOGLE CARDBOARD (Configuración en Player Settings y Scripts)
------------------------------------------
Requisitos de software:

Unity 6000.0.23f1 o una versión posterior
- Asegúrate de incluir la compatibilidad con compilación para iOS y Android durante la instalación.
- Asegúrate de instalar la versión de parche 23f1 o posterior.
- Git debe estar instalado y el ejecutable git debe estar en la variable de entorno PATH.

Como descargarlo:
- Abre Unity y crea un proyecto 3D nuevo.
- En Unity, ve a Window > Package Manager.
- Haz clic en + y selecciona Add package from git URL.
- Pega https://github.com/googlevr/cardboard-xr-plugin.git en el campo de entrada de texto.
(El paquete se debe agregar a los paquetes instalados.)

Con el paquete ya descargado y exportado en el proyecto, vamos a Google Cardboard XR Plugin for Unity.
Los recursos de muestra se cargan en Assets/Samples/Google Cardboard/<version>/Hello Cardboard.

Para generar/activar los dos scripts que le da funcionamiento al giro y movimiento de cámara:
Es necesario irse a Player Settings, y en Publishing Settings hay que marcar dos casillas.
--> Custom Main Gradle Template
--> Custom Gradle Properties Template 
(Estos archivos se pueden encontrar en la carpeta Assets/Plugins/Android).

- En 'dependencias' del 'mainTemplate.gradle' debería quedar así: 
dependencies {
    implementation fileTree(dir: 'libs', include: ['*.jar'])
    implementation 'androidx.appcompat:appcompat:1.6.1'
    implementation 'com.google.android.gms:play-services-vision:20.1.3'
    implementation 'com.google.android.material:material:1.12.0'
    implementation 'com.google.protobuf:protobuf-javalite:3.19.4'
**DEPS**}

- Y el documento de gradleTemplate.properties debería de quedar así:
org.gradle.jvmargs=-Xmx**JVM_HEAP_SIZE**M
org.gradle.parallel=true
unityStreamingAssets=**STREAMING_ASSETS**
android.useAndroidX=true
android.enableJetifier=true
**ADDITIONAL_PROPERTIES**

3. CONFIGURACIÓN DE ESCENA (Tags y Layers)
------------------------------------------
Para que los scripts funcionen, los siguientes Tags deben existir:
- [Player]: Asignado al objeto "Jugador".
- [MainCamera]: Asignado a la cámara principal.
- [Agarrable]: Para objetos que se pueden recolectar.
- [Mochila]: Específico para el ítem de la mochila.
- [SillaRuedas]: Para la interacción con el NPC.

Layers Críticas:
- Static: El escenario debe ser estático para el NavMesh.
- Ignore Raycast: Para el Reticle Pointer de Cardboard.

4. INSTRUCCIONES DE REINSTALACIÓN
----------------------------------------
Paso 1 (Git): Debido al tamaño de los assets, usar el buffer extendido:
   git config http.postBuffer 524288000
   (Esto ocurre porque esa dirección contiene un modelo pesado, osea, es mayor a 100MB y Git no deja si no lo fuerzas).
   git clone [URL_DEL_REPOSITORIO]

Paso 2 (Unity): 
   - Abrir el proyecto con Unity 6000.3.9f1.
   - Ir a Project Settings > XR Plugin Management.
   - Activar la casilla "Cardboard" en la pestaña de Android.

Paso 3 (Físicas/IA):
   - Si la calle no tiene caminos azules, ir al objeto "Generador_NavMesh".
   - Presionar el botón "Bake" en el componente NavMeshSurface.

5. CONTROLES (Configuración Bluetooth) -> A través de los Scripts conectados al New Input Manager
--------------------------------------
El proyecto está optimizado para mandos de Xbox/PlayStation vía Bluetooth.
- Joystick Izquierdo: Movimiento.
- Botón Sur (A/X): Agarrar objetos / Interactuar.
- Botón Este (B/O): Cancelar / Salir.

6. SCRIPTS PRINCIPALES
--------------------------------------
- SistemaAgarre.cs: Lógica de recolección y Raycast.
- MetaNivel.cs: Evaluación final y control de puertas.
- ControlSillaRuedas.cs: Movimiento del NPC.
- FlechaControlTotal.cs: Guía visual de navegación.
