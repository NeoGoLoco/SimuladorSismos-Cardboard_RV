graph TD
    %% --- ESTILOS VISUALES (CSS) ---
    classDef scene fill:#2d3436,stroke:#b2bec3,stroke-width:2px,color:#fff,rx:5px,ry:5px;
    classDef script fill:#0984e3,stroke:#74b9ff,stroke-width:2px,color:#fff,rx:5px,ry:5px;
    classDef tag fill:#e17055,stroke:#fab1a0,stroke-width:2px,color:#fff,rx:20px,ry:20px;
    classDef layer fill:#6c5ce7,stroke:#a29bfe,stroke-width:2px,stroke-dasharray: 5 5,color:#fff,rx:5px,ry:5px;
    classDef disabled fill:#636e72,stroke:#2d3436,stroke-width:1px,color:#b2bec3,rx:5px,ry:5px;

    %% --- NODO RAÍZ ---
    Root[SampleScene]:::scene

    %% --- ESCENARIO ---
    Root --> Escenario[1. Escenario]:::scene
    Escenario -.-> TagEscenario(((Static Layer))):::layer

    %% --- FLUJO DEL JUEGO ---
    Root --> Flujo[2. Flujo del Juego]:::scene

    %% Sistemas principales
    Flujo --> DirDialogos[Director_Dialogos]:::scene
    DirDialogos --- ScrDialogos((Asistencia NPC)):::script

    Flujo --> Meta[Meta]:::scene
    Meta --- ScrMeta((Gestor Simulación)):::script

    Flujo --> Silla[SilladeRuedas]:::scene
    Silla -.-> TagSilla([Tag: SillaRuedas]):::tag
    Silla --- ScrSilla((Control Silla Ruedas)):::script

    Flujo --> Aleatorizador[AleatorizadorObjetos]:::scene
    Aleatorizador --- ScrAleatorio((Aleatorizador Objetos)):::script

    %% --- EL JUGADOR ---
    Flujo --> Jugador[Jugador]:::scene
    Jugador -.-> TagJugador([Tag: Player]):::tag
    Jugador --- ScrJugador((Control Terremoto / Sistema Agarre)):::script

    %% Desglose de Cámara
    Jugador --> Camara[Main Camera]:::scene
    Camara -.-> TagCamara([Tag: MainCamera]):::tag
    
    Camara --> Reticle[CardboardReticlePointer]:::scene
    Reticle -.-> LayerReticle(((Layer: Ignore Raycast))):::layer
    Reticle --- ScrReticle((Cardboard Reticle)):::script

    %% Cámaras Deshabilitadas
    Flujo --> CamPC[MainCamera - PC]:::disabled
    Flujo --> CamQuest[CameraOffset - Quest 3]:::disabled

    %% --- OBJETOS AGARRABLES ---
    Aleatorizador --> Items[Objetos agarrables]:::scene
    Items --> Vaso[Vaso / Mouse / Laptop...]:::scene
    Vaso -.-> TagItem([Tag: Agarrable / Mochila]):::tag
    Vaso --- ScrItem((Objeto Sismico / Valor Objeto)):::script