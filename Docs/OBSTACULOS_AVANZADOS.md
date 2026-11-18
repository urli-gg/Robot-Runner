# Robot Runner Advanced Obstacles

## Amenazas

| Amenaza | Respuesta | Recursos |
| --- | --- | --- |
| Robot patrulla | Salto largo sobre su cuerpo más alto que la sierra | Bot Patrol Run, orientado hacia el corredor |
| Barrera eléctrica | Salto largo sobre los dos emisores y el arco | Toggle Activation y Spark A. Aviso sonoro sintetizado y conectado al mezclador SFX |
| Prensa hidráulica | Permanecer en el suelo. El brazo alcanza su máxima extensión a 1,20 unidades sobre la pista | Robotic Arm Grab. El trigger se acorta al retraerse y el planificador usa la extensión máxima |
| Rodillo industrial | Salto corto sobre una plataforma de perfil bajo y ancho | Conveyor Loop, con rodillos animados |

Los siete prefabs comparten el sistema de daño y se reciclan por tipo. Los cuatro nuevos toman los sprites del sector en que aparecen. Los recursos visuales provienen de las hojas de Robot Factory del proyecto.

## Balance en el Inspector

| Etapa | Tiempo | Intervalo base + variación | Recuperación mínima | Probabilidad de seleccionar un patrón |
| --- | --- | --- | --- | --- |
| Introducción | 0–18 s | 2,25–2,80 s | 0,42 s | 0 % |
| Presión | 18–36 s | 1,40–1,72 s | 0,28 s | 54 % |
| Dominio | 36–54 s | 0,98–1,20 s | 0,22 s | 68 % |
| Experto | Desde 54 s | 0,72–0,86 s | 0,16 s | 80 % |

El intervalo es un objetivo de generación. El spawner retrasa la siguiente salida si el par requiere más espacio, y añade una pausa breve al cerrar una secuencia. La velocidad sube continuamente de 5,60 a 9,40 unidades por segundo. Los pesos de aparición individual, el nivel de desbloqueo y la recomendación de salto de cada tipo se configuran en el prefab. Los intervalos, recuperación, patrones y velocidad se ajustan en el spawner de SampleScene.

- Presión incluye sierra → patrulla, cajas → dron y patrulla → barrera.
- Dominio añade dron → sierra → rodillo, y rodillo → barrera → prensa.
- Experto añade prensa → cajas → rodillo → dron, y sierra → patrulla → prensa → barrera.

## Comprobación de trayectorias

El modelo integra la misma fuerza, gravedad y paso de física que RunnerController. Usa la altura y anchura del collider del corredor y calcula el intervalo de altura útil para saltos cortos y largos. El volumen de cada obstáculo sale de sus triggers. Para un salto, la intersección horizontal de ambos colliders debe caber dentro del intervalo de altura útil con margen de dos pasos de física. Para un obstáculo aéreo, el corredor permanece en el suelo durante toda la intersección.

Para separar dos obstáculos se reserva la parte del vuelo anterior que queda tras cruzar su centro, la recuperación de la etapa y el tiempo de anticipación del siguiente salto. La velocidad usada para convertir esos tiempos a distancia acota la aceleración hasta llegar al segundo obstáculo. Los obstáculos aéreos reservan el ancho completo de intersección. Cada transición devuelve al corredor al suelo, así que la misma validación cubre la secuencia completa y la unión entre secuencias. Para la prensa se usa la envolvente de su fase extendida, que contiene el collider de cada fase. El margen de visión se comprueba a velocidad máxima en 4:3 y se exige un mínimo de 0,60 s antes de la acción prevista.

## Reproducción

- En Unity 6000.5.9f1, el menú **Robot Runner → Rebuild factory presentation** reconstruye e integra los prefabs y verifica el recorrido.
- Ejecutar `RobotRunnerEdition.BuildWindows` genera el player de desarrollo. Ejecutar el player con `-rr-smoke <directorio>` produce el informe y las capturas del gameplay.
- Ejecutar `RobotRunnerEdition.BuildAndroid` genera `RobotRunner_AdvancedObstacles_Test.apk`. La ruta por defecto es `Builds/AdvancedObstacles`; se puede usar la variable `ROBOT_RUNNER_BUILD_DIR`.

## Resultados en Unity 6000.5.9f1

- **Windows PASS.** La prueba final duró 420 segundos de juego con la semilla 173, cruzó los siete tipos y conservó las dos vidas. Registró 288 saltos, tres obstáculos activos simultáneos, 14 instancias asignadas, 412 reutilizaciones y 134 patrones completados. Llegó a 9,40 unidades por segundo.
- **Física y colisiones PASS.** Cada prefab quitó una vida en una colisión individual. Se verificaron la inmunidad temporal, la muerte, el récord, la pausa, el reinicio, la vuelta al menú, las preferencias de audio y el HUD.
- **Recorrido PASS.** El editor comprobó 61 velocidades, las 196 combinaciones ordenadas de par y etapa, y 32.768 selecciones deterministas en 128 semillas. El perfil conservador del tap inmediato tiene 0,54 s de vuelo y 1,01 unidades de altura. El máximo sostenido tiene 0,88 s de vuelo y 2,49 unidades de altura. A 9,40 unidades por segundo equivalen a 5,08 y 8,27 unidades horizontales. El toque breve automatizado, que mantuvo el toque unos fotogramas, alcanzó 1,29 unidades.
- **Capturas.** La prueba registró las 16 combinaciones de amenaza nueva y bioma con el HUD activo. El informe y las capturas se guardan localmente en `Builds/AdvancedObstacles/smoke-final`.
- **Android pendiente.** Unity Hub descargó Android Build Support, pero su instalador devolvió código 2. El editor seguía careciendo del módulo y devolvió `build target was unsupported` al intentar generar la APK. No se midieron FPS ni consumo en un dispositivo Android.
