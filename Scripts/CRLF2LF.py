# <copyright file="CRLF2LF.py" company="River-Mochi">
# Copyright (c) 2026 River-Mochi. All rights reserved.
# Licensed under the MIT License. You may not use this file except in compliance with this License.
# See LICENSE file in the project root for full license information.
# This notice and the MIT License notice must be kept with
# all copies or substantial portions of this code.
# ================= </copyright> ======================

cd ~/source/repos/CS2-RoadRailSpeeds

python - <<'PY'
from pathlib import Path

for path in Path("RoadRailSpeeds").rglob("*"):
    if not path.is_file():
        continue
    if path.suffix.lower() not in {".cs", ".csproj", ".xml", ".json", ".tsx", ".ts", ".css", ".md", ".ps1"}:
        continue

    data = path.read_bytes()
    if b"\0" in data:
        continue

    new_data = data.replace(b"\r\n", b"\n").replace(b"\r", b"\n")
    if new_data != data:
        path.write_bytes(new_data)
        print(path)
PY

git diff --check
