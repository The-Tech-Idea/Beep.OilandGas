"""Capture current files without claiming older runtimes compiled later edits."""
from pathlib import Path
from glob import glob
import hashlib
import json
import subprocess
import xml.etree.ElementTree as ET


HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
EVIDENCE = HERE / "observed-evidence.json"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def capture():
    evidence = json.loads(EVIDENCE.read_text(encoding="utf-8"))
    paths = set()

    def add(path):
        path = Path(path).resolve()
        assert path.is_file(), f"Missing capture input: {path}"
        paths.add(path)

    current_data = ROOT / "TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll"
    add(current_data)
    data_hash = digest(current_data)
    equality = {}
    for name, gate in evidence["gates"].items():
        assert gate["status"] == "passed" and gate["exit_code"] == 0
        project = Path(gate["project"])
        add(project)
        tree = ET.parse(project)
        for tag in ("Compile", "RazorComponent"):
            for node in tree.findall(f".//{tag}"):
                target = Path(node.attrib["Include"])
                target = target if target.is_absolute() else project.parent / target
                matches = glob(str(target), recursive=True)
                assert matches, f"Missing capture include: {target}"
                for match in matches:
                    add(match)
        for node in tree.findall(".//Reference/HintPath"):
            add(node.text)
        for target in project.parent.iterdir():
            if target.is_file() and target.suffix in (".cs", ".razor", ".json"):
                add(target)
        runtime = Path(gate["runtime"])
        for target in runtime.iterdir():
            if target.is_file() and target.suffix in (".dll", ".json"):
                add(target)
        if name != "regression":
            target = runtime / current_data.name
            assert digest(target) == data_hash, f"Different final Data DLL: {name}"
            equality[name] = data_hash

    host = Path(evidence["browser"]["host_root"])
    for target in host.iterdir():
        if target.is_file():
            add(target)
    for target in (host / "bin/Debug/net10.0").iterdir():
        if target.is_file() and target.suffix in (".dll", ".json"):
            add(target)
    assert digest(host / "bin/Debug/net10.0" / current_data.name) == data_hash
    equality["final-browser-host"] = data_hash

    for pattern in (
        "TheTechIdea.Data/OilGas/DrillingAndConstruction/*.cs",
        "Scripts/WorkflowProtectedTab/*",
        "output/playwright/351-completion-report-browser/**/*",
    ):
        for target in ROOT.glob(pattern):
            if target.is_file():
                add(target)
    for name in (
        "TheTechIdea.Data/TheTechIdea.Data.csproj",
        "Beep.OilandGas.Web/Beep.OilandGas.Web.csproj",
        "Scripts/Test-PurchaseOrderOwnership.ps1",
        "Scripts/Start-WorkflowProtectedTab.ps1",
        "MASTER-TODO-TRACKER.md",
        "Plans/FrameworkAppEnhancement/Implementation-351-Approved-Program-Drilling-Execution.md",
        "Plans/FrameworkAppEnhancement/Company-Phase-C-Engineering.md",
        "Plans/FrameworkAppEnhancement/Company-Plan-Index.md",
        "Plans/FrameworkAppEnhancement/Feature-07-Asset-Delivery-and-Interventions.md",
        "Plans/FrameworkAppEnhancement/Persona-07-Exploration-Drilling-and-Development.md",
        "Plans/FrameworkAppEnhancement/README.md",
    ):
        add(ROOT / name)
    for target in HERE.rglob("*"):
        if target.is_file() and target.name not in (
            "observed-evidence.json", "capture-paths.json"
        ) and "__pycache__" not in target.parts:
            add(target)

    branch = subprocess.check_output(
        ["git", "branch", "--show-current"], cwd=ROOT, text=True
    ).strip()
    head = subprocess.check_output(
        ["git", "rev-parse", "HEAD"], cwd=ROOT, text=True
    ).strip()
    staged = subprocess.check_output(
        ["git", "diff", "--cached", "--name-only"], cwd=ROOT, text=True
    ).strip()
    assert branch == evidence["branch"] == "master"
    assert head == evidence["git_head"] and not staged
    hashes = {str(path): digest(path) for path in sorted(paths)}
    (HERE / "capture-paths.json").write_text(
        json.dumps(sorted(hashes), indent=2) + "\n", encoding="utf-8"
    )
    evidence["sha256_at_capture"] = hashes
    evidence["sha256_at_capture_scope"] = (
        "Current linked production/test/component sources, canonical Data sources, "
        "generated projects/references/runtime DLLs, owner/six trackers, logs, "
        "reproduction and browser artifacts. Current file hashes do not assert "
        "older binaries compiled later edits. Broad precedes recipient progress "
        "UI and final reason wording; final UI precedes only reason wording; "
        "final screen/browser qualify final markup. Earlier ordinary regression "
        "predates removal of the invented pre-spud test constraint. API journeys "
        "retain their compiled fixture/test-helper scope. Manifest/path list excluded."
    )
    evidence["final_data_dll_equality"] = equality
    evidence["capture_validation"] = {
        "current_hashes": len(hashes),
        "branch": branch,
        "head": head,
        "staged_paths": 0,
        "full_goal": "active-partial",
        "last_promoted_checkpoint": 350,
    }
    EVIDENCE.write_text(json.dumps(evidence, indent=2) + "\n", encoding="utf-8")
    assert all(digest(Path(name)) == value for name, value in hashes.items())
    print(f"Captured and rechecked {len(hashes)} current hashes; final Data DLL equal in {len(equality)} runtimes; index empty.")


if __name__ == "__main__":
    capture()
