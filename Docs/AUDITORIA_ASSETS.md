# Auditoría de Industrial Robot Factory

Revisé el README y la licencia de CookieEfedu antes de integrar el arte. El paquete permite usar, modificar e incluir los gráficos en juegos compilados. Restringe la redistribución como assets independientes y su uso para entrenamiento de IA. El proyecto referencia las hojas originales y los sprites seleccionados. Las imágenes nuevas son composiciones para el escenario del juego.

## Inventario

Cada una de las cuatro paletas de color y la variante gris contiene las mismas seis hojas y 411 entradas. El recuento de los XML da 2.055 entradas entre las cinco variantes. El paquete incluye PNG individuales, coordenadas XML y JSON, y paletas PNG/GPL. Las hojas usan celdas de 32 × 32 px, salvo `small_props`, que usa 14 × 14 px. Los bordes transparentes y las coordenadas de cada hoja permiten alinear las secuencias.

| Hoja | Tamaño | Entradas por variante | Contenido revisado | Uso en Professional Edition |
|---|---:|---:|---|---|
| `terrain` | 512 × 512 | 160 | Placas, paneles 2 × 2, puertas, rejillas, pendientes y líquidos por profundidad | Paredes, puertas, respiraderos, paneles numerados, rejillas y el borde continuo de pista |
| `props` | 256 × 128 | 21 | Cuatro tipos de caja, máquinas, válvulas, vigas, plataformas y conjunto de trituradora | Vigas, consolas, válvulas, cajas de almacén y dos variantes del obstáculo triple |
| `small_props` | 256 × 128 | 64 | Conductos modulares A/B, lámparas, cables, LED, cadenas, imanes, interruptores y piezas | Conductos, lámparas, LED, cadenas, cables, indicadores y piezas de montaje |
| `props_animated` | 256 × 256 | 42 | 14 grupos de tres frames, incluyendo cintas, sierra, puerta, brazo, reactor, toggle y fluidos | Cinta, brazo en ida y vuelta, flujo animado, sierra en bucle y reactor estable |
| `enemies_animated` | 512 × 256 | 78 | 26 secuencias de tres frames para Bot Patrol, drones A/B y torretas A/B | Idle, carrera, daño y apagado del robot. Idle y vuelo de ambos drones |
| `vfx_animated` | 256 × 256 | 46 | Vapor de seis frames, chispas en dos direcciones, explosiones y proyectiles con sus estelas | Vapor ambiental, chispas al aterrizar y siete frames de plasma en impactos |

## Uso en la escena original

La escena usaba un muro Night Factory y una pista con tilemaps estáticos. `Assets/Tiles` contiene 13 tiles de placas o rejillas y el prefab de paleta `NightFactory`. El robot y su retrato compartían el primer frame de Bot Patrol Idle. Los obstáculos usaban la sierra Day Factory, el primer frame del dron A Night Factory y tres copias de Box 01. Los prefabs de interfaz ya tenían iconos de Play, Settings, Credits, Back, Home y Play Again, además del panel rojo con franjas de Game Over.

## Integración

- Night inicia cada partida. Day, Hot y Toxic se suceden cada 26 segundos de supervivencia y vuelven a Night al completar el ciclo. Cada sector combina cuatro salas diseñadas como acceso y maquinaria, almacén con cinta, montaje con brazo o módulo de reactor y fluidos.
- La cubierta visual se genera detrás del carril y su borde coincide con el suelo físico. Las máquinas, el vapor y los fluidos son decoración sin collider.
- Se reservaron las torretas, proyectiles, trituradoras, pendientes y líquidos con colisión. Sus mecánicas requerirían patrones diferentes de los tres que el juego enseña con los controles actuales.
- El importador ajusta el Point Filtering y los pivotes en las composiciones y las siete hojas de animaciones usadas. Los PPU de los tilemaps originales se conservan. Los gráficos de gameplay se referencian por sprite, con paletas legibles en todos los sectores.
