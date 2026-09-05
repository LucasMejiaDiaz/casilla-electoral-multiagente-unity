# Escena de casilla especial — integración Unity

La escena `Assets/Scenes/PollingStationAndares.unity` se genera mediante:

`Tools > Polling Station > Build Andares Scene`

Por defecto usa datos simulados para que la escena funcione aunque el backend
esté apagado. Para utilizar el puente HTTP, seleccione
`Systems/SimulationStateProvider` y desactive `Use Mock Data`.

La escena usa `Assets/Models/PollingStation/casilla_votacion.fbx`, exportado
del modelo de Blender de la casilla Andares. Sus nodos `SPAWN`, `EXIT`,
`SLOT_*` y `QUEUE_*` se enlazan automáticamente al `SceneLayout`; por ello, los
votantes se colocan en los puntos diseñados en Blender sin enviar coordenadas
desde Python. Si el FBX no existe, el generador conserva como respaldo la
escena estilizada hecha con primitivas.

## Contrato HTTP

El proveedor consulta `GET http://127.0.0.1:5000/get_agents` una vez por
segundo. La respuesta debe contener:

```json
{
  "step": 12,
  "simulation_time": 42.5,
  "running": true,
  "paused": false,
  "external_event": {
    "active": false,
    "kind": "",
    "remaining": 0.0
  },
  "agents": [
    {
      "id": 17,
      "state": "esperando_casilla",
      "station": "casilla",
      "queue_position": 2
    }
  ]
}
```

Unity calcula las posiciones a partir del estado; el backend no necesita
enviar `x`, `y` ni `wealth`.

## Sustitución de assets

Los placeholders están en `Assets/Prefabs/PollingStation` y se mantienen como
respaldo. El modelo detallado puede regenerarse con
`Tools/Blender/export_casilla_to_unity.py`. Los modelos definitivos deben
mantener pivote al nivel del piso, escala en metros y frente hacia `+Z`. Se
pueden sustituir sin cambiar el contrato del backend.
