# Cambios

## Professional Edition

- Night, Day, Hot y Toxic Factory tienen muros distantes, salas modulares, maquinaria animada, escape de vapor y una cubierta que rueda a la velocidad de los obstáculos. Los sectores cambian por supervivencia, con una transición de paleta y decoración.
- Los fotogramas de Bot Patrol muestran carrera, subida, caída, aterrizaje, impacto y apagado. El destello de contacto y las chispas de aterrizaje se reutilizan durante la partida.
- Las sierras animadas, los drones A/B y las dos apariencias de las cajas usan colliders ajustados. La generación combina patrones de salto corto, largo y paso por debajo. Todos los obstáculos comparten la velocidad de la pista y se reciclan al salir de cámara.
- Los paneles de menú, pausa, ajustes, créditos y Game Over tienen transiciones, botones consistentes y área segura. Game Over conserva el fondo rojo y las franjas de advertencia. El HUD compacto muestra vidas, puntuación, pausa, sector y récord.
- El mezclador conserva sus controles y atenúa la música durante pausa y Game Over. El juego usa el clic original y seis efectos sintetizados para acciones y avisos.
- Un toque sobre **II** no se envía como salto. Se admiten toques justo antes del aterrizaje y soltarlos corta la extensión del salto. Las suscripciones se liberan al recargar la escena.
- Android solicita 60 FPS, reutiliza decoraciones y obstáculos y empaqueta el arte seleccionado. Los archivos generados de Android quedan fuera del índice de Git.
