# PremierHub VR

Aplicación de realidad virtual desarrollada en **Unity** para **Meta Quest 3s**. Permite a los usuarios iniciar sesión con su cuenta de PremierHub y jugar un minijuego de portero (goalkeeper) en VR, donde los puntos obtenidos se sincronizan con el backend.

## Requisitos

### Hardware
- Meta Quest 3s

### Software
- [Unity 6](https://unity.com/releases/editor/whats-new/6000.0) (versión usada en el proyecto)
- [Android Build Support](https://docs.unity3d.com/Manual/android-sdksetup.html) instalado desde Unity Hub
- [Meta XR SDK](https://developers.meta.com/horizon/downloads/package/oculus-utilities-for-unity-5/) (incluido en el proyecto vía Packages)
- [Node.js](https://nodejs.org/) (para correr el backend localmente)
- [ADB](https://developer.android.com/tools/adb) (para desplegar en el headset)

---

## Estructura del proyecto

```
Juego PremierHub/
├── Assets/
│   ├── Codigos/          # Scripts de C# (login, juego, API client)
│   ├── Scenes/           # Escenas de Unity
│   │   ├── LoginScene    # Pantalla de inicio de sesión
│   │   ├── Menu          # Menú principal con perfil del jugador
│   │   └── SampleScene   # Minijuego de portero
│   ├── Prefabs/          # Prefabs reutilizables
│   └── StreamingAssets/  # Archivos de configuración en runtime (.env)
├── Backend/              # Servidor Node.js (API REST)
└── ProjectSettings/      # Configuración de Unity
```

---

## Instalación y ejecución local

### 1. Clonar el repositorio

```bash
git clone https://github.com/4ll3ssandro/Juego-PremierHub.git
cd "Juego PremierHub"
```

### 2. Configurar el backend

```bash
cd Backend
npm install
npm start
```

El servidor levanta en `http://localhost:4000`.

### 3. Configurar la URL del backend en Unity

Crea el archivo `Assets/StreamingAssets/.env` con el siguiente contenido:

```
PREMIERHUB_API_URL=http://localhost:4000
```

> Para builds standalone en el headset usa la URL pública del backend en lugar de `localhost`.

### 4. Abrir el proyecto en Unity

1. Abre **Unity Hub**.
2. Selecciona **Open > Add project from disk** y apunta a la carpeta `Juego PremierHub`.
3. Usa la versión de Unity indicada en el proyecto (Unity 6).
4. Una vez abierto, ve a **File > Build Settings** y confirma que la plataforma está en **Android**.

### 5. Ejecutar en el Editor (simulación de escritorio)

Abre la escena `Assets/Scenes/LoginScene.unity` y presiona **Play** en el Editor de Unity. Puedes navegar por las escenas usando el simulador XR del Editor.

---

## Desplegar en Meta Quest 3s

### Prerrequisitos

- Activa el **Modo Desarrollador** en tu Meta Quest 3s desde la app Meta en tu teléfono.
- Conecta el headset al PC por USB y acepta el permiso de depuración ADB en el visor.
- Verifica la conexión:

```bash
adb devices
```

### Build e instalación

1. En Unity, ve a **File > Build Settings**.
2. Asegúrate de que la plataforma es **Android** y que el dispositivo Quest aparece en **Run Device**.
3. Haz clic en **Build and Run** (o exporta el `.apk` con **Build** e instálalo manualmente con ADB):

```bash
adb install -r Meta.apk
```

### Actualizar la URL del backend en el build

Para que el headset pueda conectarse al backend, el archivo `.env` dentro de `StreamingAssets` debe contener la URL pública antes de hacer el build:

```
PREMIERHUB_API_URL=https://tu-backend.ejemplo.com
```

---

## Despliegue en Meta Horizon (developers.meta.com)

La aplicación fue publicada en [Meta Horizon Store para desarrolladores](https://developers.meta.com/horizon/).

### Pasos para subir una nueva versión

1. Genera el `.apk` firmado desde Unity (**File > Build Settings > Build**).
2. Inicia sesión en [Meta Developer Dashboard](https://developers.meta.com/horizon/).
3. Selecciona tu aplicación **PremierHub VR**.
4. Ve a **Distribución > Cargar binario** y sube el `.apk`.
5. Completa los metadatos requeridos (notas de versión, capturas de pantalla, etc.).
6. Envía la versión al canal de **Release Channel** deseado (Alpha, Beta o Public).

---

## Escenas y flujo de la aplicación

| Escena | Descripción |
|---|---|
| `LoginScene` | El usuario ingresa su correo y contraseña. Se valida contra el backend y se guarda la sesión. |
| `Menu` | Muestra el perfil del jugador (nombre, puntos). Botón para iniciar el minijuego. |
| `SampleScene` | Minijuego de portero en VR. Al terminar, los atajadas se envían al backend y se suman puntos. |

---

## Variables de entorno

| Variable | Descripción | Default |
|---|---|---|
| `PREMIERHUB_API_URL` | URL base del backend | `http://localhost:4000` |

El archivo `.env` debe ubicarse en `Assets/StreamingAssets/.env`.

---

## Scripts principales

| Script | Función |
|---|---|
| `LoginController.cs` | Maneja el formulario de login y llama a la API |
| `PremierHubApiClient.cs` | Cliente HTTP para comunicarse con el backend |
| `PremierHubSession.cs` | Almacena la sesión activa del usuario |
| `MenuGameSelection.cs` | Muestra el menú principal y carga el juego |
| `BallSpawner.cs` | Genera pelotas en el minijuego |
| `GloveBallDetector.cs` | Detecta las atajadas del portero |
| `RestartGame.cs` | Reinicia la partida |
