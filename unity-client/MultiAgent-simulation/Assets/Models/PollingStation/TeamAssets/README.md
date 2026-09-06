# Assets del equipo

Estos FBX fueron proporcionados por el equipo mediante la carpeta compartida de
Google Drive. Permanecen importados sin cambiar la simulacion ni las coordenadas
de las estaciones.

| Archivo | Uso en Unity |
|---|---|
| `mesa.fbx` | Disponible para sustituir mesas cuando se ajuste manualmente. |
| `Silla.fbx` | Disponible para sustituir sillas cuando se ajuste manualmente. |
| `Casilla.fbx` | Disponible para sustituir mamparas cuando se ajuste manualmente. |
| `Urna.fbx` | Disponible para sustituir urnas cuando se ajuste manualmente. |
| `modelo_mujer.fbx` | Modelo visual de los votantes. Incluye `Idle` y `Walk`. |

El mobiliario individual no se activa automaticamente porque el ajuste no
uniforme deformaba sus proporciones. La escena conserva el mobiliario original,
que se ve correctamente, y utiliza el modelo femenino animado para los votantes.
El modelo masculino no se incluye todavia porque el equipo indico que sus
animaciones siguen pendientes.

Para reconstruir la escena despues de actualizar un archivo:

1. Detener Play.
2. Seleccionar `Tools > Polling Station > Build Andares Scene`.
3. Abrir `Assets/Scenes/PollingStationAndares.unity`.
