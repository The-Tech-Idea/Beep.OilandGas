import json
from pathlib import Path
root = Path(__file__).parent
d = json.loads((root / "observations.json").read_text(encoding="utf-8"))
x = {o["label"]: o for o in d["observations"]}
assert len(x) == 10
first=x["prefilled-earlier-period"]; base=first["metadata"]
assert "Correcting entry 1" in first["text"] and "Original source: Qualified observed site log 1" in first["text"] and base["recordedReports"] == 3
unchanged=x["unchanged-values-refused"]
assert "Confirm original observed report" not in unchanged["text"] and unchanged["metadata"]["recordRequests"] == base["recordRequests"] and not unchanged["storage"]
frozen=x["frozen-correction"]
assert "Confirm original observed report" in frozen["text"] and "The original remains immutable" in frozen["text"] and d["reason"] in frozen["text"] and d["summary"] in frozen["text"] and not frozen["storage"]
lost=x["lost-ack-retained"]
assert lost["metadata"]["recordRequests"] == base["recordRequests"]+1 and lost["metadata"]["recordedReports"] == 4 and len(lost["storage"]) == 1
assert not lost["storage"][0]["hasPlainReason"] and not lost["storage"][0]["hasPlainSummary"]
readonly=x["readonly-retained-reload"]
assert readonly["metadata"]["readOnly"] and len(readonly["storage"]) == 1 and readonly["metadata"]["sourceRequests"] == lost["metadata"]["sourceRequests"] and readonly["metadata"]["profileLoads"] == lost["metadata"]["profileLoads"]
recovered=x["readonly-keyboard-recovered"]
assert "Original report outcome" in recovered["text"] and d["reason"] in recovered["text"] and recovered["metadata"]["recoverRequests"] == base["recoverRequests"]+1 and not recovered["storage"]
history=x["current-superseded-readonly-history"]
assert "3 observed periods; 1 corrections; 4 immutable history entries" in history["text"] and "4 recorded history entries" in history["text"]
assert "SUPERSEDED" in history["text"] and "CURRENT CORRECTION" in history["text"] and "Observed operations: Observed hole drilling progress" in history["text"] and d["summary"] in history["text"]
assert history["metadata"]["previousEnd"] == base["previousEnd"] and "CORRECT OBSERVATIONS FROM ENTRY" not in history["text"]
for label in ["other-company-denied", "other-account-denied"]:
 assert d["reason"] not in x[label]["text"] and d["summary"] not in x[label]["text"] and "Qualified observed site log" not in x[label]["text"]
restored=x["restored-readonly-history"]
assert "CURRENT CORRECTION" in restored["text"] and d["reason"] in restored["text"] and restored["metadata"]["recordRequests"] == base["recordRequests"]+1 and restored["metadata"]["recoverRequests"] == base["recoverRequests"]+1
for o in x.values():
 assert "\ufffd" not in o["text"] and "\u00c2" not in o["text"]
print("10 selected Chromium observations passed: correction, audit chain, no-op, encrypted retention, readonly keyboard recovery and context isolation.")
