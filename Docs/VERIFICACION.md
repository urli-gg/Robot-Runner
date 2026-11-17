# Verificación

## Prueba de escritorio

La prueba de humo usa el player de desarrollo de Windows construido con Unity 6000.5.9f1. Inyecta toques con Input System para pulsar controles reales. La prueba de recorrido usa el controlador del juego con colliders activos y registra las instancias del pool.

| Comprobación | Cobertura |
|---|---|
| Arranque y navegación | SampleScene, PLAY, pausa, Settings, Credits, Resume, Game Over, Play Again y vuelta al menú |
| Entrada | Salto corto, salto mantenido, toque sobre Pause y toque fuera de los botones con la partida pausada |
| Vida y récord | Impacto con la torre de cajas, inmunidad temporal, muerte, guardado de la puntuación y reinicio con dos vidas |
| Progresión | Recorrido automatizado de 300 segundos de juego, cuatro biomas, todos los niveles de dificultad y cruce de cada tipo de obstáculo |
| Configuración | Toques en ambos toggles y en el slider, actualización del mezclador, persistencia tras recargar la escena y un solo AudioManager |
| Resoluciones | 1280 × 590, 1200 × 900, 1440 × 648 y 1280 × 720 como referencias de relaciones de aspecto |

El comando de compilación valida las referencias de scripts en la escena y los prefabs de obstáculos. El perfil de salto y los colliders se comprueban a 61 velocidades entre 5,6 y 7,85 unidades por segundo.

En la revisión visual se corrigió el orden de dibujo del HUD y el parentesco de las etiquetas SFX y Volume. El test comprueba esas relaciones y el retorno a Pause al cerrar Settings.

## APK de prueba

Unity completó la compilación en `Builds/ProfessionalEdition/RobotRunner_Professional_Test.apk`. El archivo pesa 39.005.854 bytes. `aapt2` confirmó el paquete `com.urli.robotrunner`, la orientación horizontal, el nivel mínimo 26 y el nivel objetivo 36. `apksigner` validó la firma v2 y se comprobó que el APK contiene `libil2cpp.so` para ARM64.

SHA-256 de la APK verificada

```text
2c66db7905c5b2f5b2c81a1c2b82aeb0ba64887643b7687ace87c1c6006db20c
```

El APK estable conserva los mismos bytes que en `main`. Ambos binarios se guardan localmente.

## Reproducir

Compila en Windows con el método `RobotRunnerEdition.BuildWindows` y ejecuta el player de desarrollo con `-rr-smoke <carpeta-de-evidencias>`. La prueba guarda capturas y un archivo `smoke-test.txt` y devuelve 0 si pasa. El código de prueba se excluye de la APK de distribución.

## Cobertura de dispositivo

Las resoluciones de escritorio comprueban proporciones de pantalla. Para los recortes se comprobó la geometría del HUD, los botones y el control de volumen con los perfiles de Device Simulator en ambas orientaciones horizontales.

| Perfil | Pantalla | Área segura en píxeles |
|---|---:|---|
| iPhone 13 Pro Max | 2778 × 1284 | 2496 × 1221, con márgenes laterales de 141 px |
| iPad Pro 12.9 (2018) | 2732 × 2048 | 2732 × 2008, con margen inferior de 40 px |
| Galaxy S10+ | 3040 × 1440 | 2898 × 1440, con un margen lateral de 142 px que cambia de lado al rotar |

La inspección del recorte en hardware, la mezcla por altavoz, el consumo de memoria y los FPS sostenidos de Android quedan para una prueba con dispositivo conectado.
