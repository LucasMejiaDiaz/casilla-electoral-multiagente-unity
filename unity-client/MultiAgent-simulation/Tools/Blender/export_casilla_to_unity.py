"""Export the Andares Blender source as a Unity-ready FBX.

Usage:
    python export_casilla_to_unity.py SOURCE.blend OUTPUT.fbx

The script requires Blender's ``bpy`` Python module. Cameras and lights are
excluded because Unity owns those systems. Meshes, labels converted to meshes,
and named EMPTY anchors are preserved so Unity can reuse SLOT/QUEUE/PATH nodes.
"""

from __future__ import annotations

import sys
from pathlib import Path

import bpy


# Unity mueve a los votantes directamente entre puntos de la simulación. Estas
# dos mallas formaban un laberinto visual que los agentes parecían atravesar.
# Se omiten del FBX, pero permanecen intactas en el archivo .blend original.
EXCLUDED_VISUAL_OBJECTS = {"Cintas_Fila", "Postes_Fila"}


def export(source: Path, destination: Path) -> None:
    bpy.ops.wm.open_mainfile(filepath=str(source))

    for item in bpy.context.selected_objects:
        item.select_set(False)

    # Text objects are converted so labels survive without Blender fonts.
    for item in list(bpy.data.objects):
        if item.type != "FONT":
            continue
        bpy.context.view_layer.objects.active = item
        item.select_set(True)
        bpy.ops.object.convert(target="MESH")
        item.select_set(False)

    exported = []
    for item in bpy.data.objects:
        if item.type not in {"MESH", "EMPTY"}:
            continue
        if item.name in EXCLUDED_VISUAL_OBJECTS:
            item.select_set(False)
            continue
        item.hide_set(False)
        item.hide_viewport = False
        item.hide_render = False
        item.select_set(True)
        exported.append(item)

    destination.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=str(destination),
        use_selection=True,
        object_types={"MESH", "EMPTY"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z",
        axis_up="Y",
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
    )
    print(f"Exported {len(exported)} objects to {destination}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise SystemExit("Expected SOURCE.blend and OUTPUT.fbx")
    export(Path(sys.argv[1]).resolve(), Path(sys.argv[2]).resolve())
