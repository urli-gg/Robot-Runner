# Robot Runner — Professional Edition

Endless runner 2D en pixel art. El robot recorre Night, Day, Hot y Toxic Factory mientras esquiva sierras, torres de cajas y drones. La puntuación sube mientras sigues con vida y el juego guarda el mejor resultado en el dispositivo.

## Jugar

- Toca y suelta para un salto corto. Mantén pulsado para saltar más alto. En escritorio también puedes usar Espacio o el botón izquierdo del ratón.
- Salta la sierra con un toque. Mantén pulsado para pasar sobre las cajas. Quédate en el suelo al cruzarte con un dron.
- Empiezas con dos vidas. Tras un impacto tienes un periodo breve de invulnerabilidad. Usa **II** para pausar o abrir Settings.
- Los interruptores Music y SFX y el control Volume se guardan localmente.

## Abrir el proyecto

Instala **Unity 6000.5.9f1** y abre la raíz del repositorio desde Unity Hub. La escena inicial es [SampleScene](Assets/Scenes/SampleScene.unity). Pulsa Play en Unity y elige **PLAY** en el menú. El paquete de Input System y el renderer 2D se resuelven desde el manifiesto del proyecto.

Para Android, instala **Android Build Support**, **Android SDK & NDK Tools** y **OpenJDK** desde Unity Hub. Usa Android como plataforma en Build Profiles y compila con `SampleScene` habilitada. El proyecto usa orientación horizontal, IL2CPP, ARM64 y el paquete `com.urli.robotrunner`. Para una APK local, usa la firma de depuración configurada por Unity.

También puedes ejecutar desde PowerShell, con la terminal situada en la raíz del proyecto:

```powershell
& "C:/Program Files/Unity/Hub/Editor/6000.5.9f1/Editor/Unity.exe" `
  -batchmode -nographics -quit -buildTarget Android `
  -projectPath "$PWD" -executeMethod RobotRunnerEdition.BuildAndroid `
  -logFile "$env:TEMP/robotrunner-android.log"
```

El destino predeterminado es `Builds/ProfessionalEdition/RobotRunner_Professional_Test.apk`. La variable `ROBOT_RUNNER_BUILD_DIR` permite elegir otra carpeta de salida. La APK estable se conserva fuera del índice de Git.

## Organización

| Carpeta | Contenido |
|---|---|
| `Assets/Scenes` | Escena de arranque y escena de respaldo |
| `Assets/Prefab` | Menús y obstáculos con sus colliders |
| `Assets/scripts/Gameplay` | Salto, secuencias de obstáculos y reutilización de instancias |
| `Assets/scripts/Presentation` | Parallax, animaciones, avisos y área segura |
| `Assets/ProfessionalEdition` | Composiciones, tema de interfaz y efectos sonoros del juego |
| `Assets/Editor` | Importación, configuración y comandos de compilación |
| `Assets/assets/robot_factory` | Arte original de Industrial Robot Factory |

Las referencias a las composiciones, animaciones y prefabs ya están guardadas en el proyecto. **Robot Runner → Rebuild factory presentation** vuelve a generar las composiciones y reaplica su configuración para cambios de arte.

## Créditos

- Juego y diseño original de **Gael Saucedo**.
- Arte 2D de **CookieEfedu**. Consulta la [licencia incluida](Assets/assets/robot_factory/LICENSE.txt) y el [sitio del autor](https://cookieefedu.itch.io).
- Música retro existente identificada como **WolfSynth** en `Assets/Audio`. Professional Edition incluye efectos originales de salto, aterrizaje, impacto, apagado, cambio de sector y récord.

Consulta el [registro de cambios](CHANGELOG.md), la [auditoría de assets](Docs/AUDITORIA_ASSETS.md) y la [verificación](Docs/VERIFICACION.md).
