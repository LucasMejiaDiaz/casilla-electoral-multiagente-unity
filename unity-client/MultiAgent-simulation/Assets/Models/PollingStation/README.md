# Modelo de la casilla Andares

`casilla_votacion.fbx` se genera desde el archivo fuente de Blender con:

```powershell
python Tools/Blender/export_casilla_to_unity.py SOURCE.blend Assets/Models/PollingStation/casilla_votacion.fbx
```

La exportación conserva los nodos `SPAWN`, `EXIT`, `SLOT_*`, `QUEUE_*` y
`PATH_*`. Unity usa el modelo como capa visual; el backend de Python continúa
siendo la autoridad sobre el estado de cada votante.
