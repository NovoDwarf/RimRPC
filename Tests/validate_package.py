"""Check the release layout and localization without third-party Python packages."""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
mod = root / "RimWorldRPC"
assemblies = {path.name for path in (mod / "1.7/Assemblies").iterdir() if path.is_file()}
assert assemblies == {"RimRPC.dll", "DiscordRPC.dll", "Newtonsoft.Json.dll"}, assemblies
about = ET.parse(mod / "About/About.xml").getroot()
assert [item.text for item in about.find("supportedVersions")] == ["1.7"]
load = ET.parse(mod / "LoadFolders.xml").getroot()
assert [item.text for item in load.find("v1.7")] == ["/", "1.7"]
assert not (mod / "About/LoadFolders.xml").exists()

keys = {"RPC_State_Disabled", "RPC_State_Connecting", "RPC_State_Connected", "RPC_State_Error"}
for source in (root / "Sources").rglob("*.cs"):
    if any(part in {"bin", "obj", "Tests"} for part in source.parts):
        continue
    keys.update(re.findall(r'"([^"\n]+)"\.Translate\(', source.read_text(encoding="utf-8-sig")))
for language in ("English", "Russian"):
    tree = ET.parse(mod / f"Languages/{language}/Keyed/RimRPC.xml").getroot()
    translations = {item.tag: item.text for item in tree}
    assert len(translations) == len(tree), f"Duplicate keys: {language}"
    assert not (keys - translations.keys()), f"Missing translations: {language}: {keys - translations.keys()}"
    assert all(translations[key] and translations[key].strip() for key in keys)
workflow = (root / ".github/workflows/release.yml").read_text()
assert "steam-workshop-upload" not in workflow and "STEAM_" not in workflow
print("Package layout, dependency set, translations, and manual Steam publishing checks passed.")
