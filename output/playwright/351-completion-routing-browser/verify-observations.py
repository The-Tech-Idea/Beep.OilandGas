from pathlib import Path
import json

folder = Path(__file__).resolve().parent
observations = json.loads((folder / "observations.json").read_text(encoding="utf-8"))["observations"]
assert len(observations) == 10
by_label = {row["label"]: row for row in observations}
assert len(by_label) == 10
assert [row["metadata"]["completionRoutingReads"] for row in observations] == [1, 2, 3, 4, 5, 6, 6, 6, 6, 7]
for row in observations:
    facts = row["metadata"]
    assert facts["recordedReports"] == 4
    assert facts["recordRequests"] == facts["recoverRequests"] == facts["assignmentWrites"] == 0
    assert "\ufffd" not in row["text"] and "\u00c2" not in row["text"]
for label in ("initial-explicit-ready-routing", "keyboard-ready-routing", "restored-ready-routing", "editable-keyboard-routing-restored"):
    text = by_label[label]["text"]
    assert "INDEPENDENT_COMPLETION / completion-inspector" in text
    assert "Every configured stage has an eligible independent recipient." in text
    assert "Routing inspection only." in text and "reporting responsibility remains ongoing" in text
    assert "Fail (reported)" in text and "pending independent acceptance" in text
blocked = by_label["configured-stage-without-independent-recipient"]["text"]
assert "Routing is blocked" in blocked and "No eligible independent recipient" in blocked
assert "completion-inspector" not in blocked
empty = by_label["no-configured-completion-workflow"]["text"]
assert "No completion approval workflow is configured" in empty and "completion-inspector" not in empty
unavailable = by_label["routing-unavailable-is-error"]["text"]
assert "could not be confirmed" in unavailable
assert "Independent completion review routing" not in unavailable and "No completion approval workflow is configured" not in unavailable
for label in ("other-company-hides-private-routing", "restored-company-requires-explicit-routing-read", "readonly-routing-disabled-no-private-recipients"):
    assert "completion-inspector" not in by_label[label]["text"]
assert by_label["readonly-routing-disabled-no-private-recipients"]["disabled"] is True
assert by_label["editable-keyboard-routing-restored"]["disabled"] is False
assert "Errors: 0, Warnings: 0" in (folder / "final-console.log").read_text(encoding="utf-8-sig")
assert (folder / "routing-ready.png").is_file() and (folder / "routing-blocked.png").is_file()
print("10 saved Chromium routing observations pass: explicit/keyboard read, readiness/blocked/empty/outage, company isolation, readonly authority and zero record/assignment writes. Controlled canonical fixtures; no native approval or acceptance claim.")
