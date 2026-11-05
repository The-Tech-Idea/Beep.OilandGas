import json
from pathlib import Path
root=Path(__file__).parent
d=json.loads((root/"observations.json").read_text(encoding="utf-8"));x={o["label"]:o for o in d["observations"]}
assert len(x)==18 and d["defaults"]=={"completion":"","result":"","reviewed":False}
base=x["verified-existing-completion-after-selector-failure"]
assert base["metadata"]["recordedReports"]==4 and base["metadata"]["recordRequests"]==1 and base["metadata"]["recoverRequests"]==1 and not base["storage"]
frozen=x["explicit-withdrawal-frozen"];assert "Confirming explicitly withdraws" in frozen["text"] and frozen["metadata"]["recordRequests"]==1
lost=x["withdrawal-lost-ack-retained"];assert lost["metadata"]["recordedReports"]==5 and lost["metadata"]["recordRequests"]==2 and len(lost["storage"])==1
recovered=x["readonly-withdrawal-recovered"];assert not recovered["storage"] and recovered["metadata"]["recoverRequests"]==2 and "withdrawn" in recovered["text"]
withdrawn=x["withdrawn-claim-remains-audited"];assert "No current completion report" in withdrawn["text"] and "explicitly withdrew" in withdrawn["text"] and d["reference"] in withdrawn["text"]
for label in ["missing-completion-refused","unreviewed-outstanding-refused"]:
 assert x[label]["metadata"]["recordRequests"]==2 and not x[label]["storage"] and "Confirm original observed report" not in x[label]["text"]
frozen=x["frozen-reported-completion"];assert "Confirm original observed report" in frozen["text"] and "Fail (reported)" in frozen["text"] and d["reference"] in frozen["text"] and "Inspection discrepancy requires independent resolution" in frozen["text"]
lost=x["completion-lost-ack-retained"];assert lost["metadata"]["recordedReports"]==6 and lost["metadata"]["recordRequests"]==3 and len(lost["storage"])==1
assert not lost["storage"][0]["hasPlainReference"] and not lost["storage"][0]["hasPlainWork"]
readonly=x["readonly-completion-original-reloaded"];assert readonly["metadata"]["readOnly"] and len(readonly["storage"])==1 and readonly["metadata"]["sourceRequests"]==lost["metadata"]["sourceRequests"] and readonly["metadata"]["profileLoads"]==lost["metadata"]["profileLoads"]
recovered=x["readonly-keyboard-completion-recovered"];assert not recovered["storage"] and recovered["metadata"]["recoverRequests"]==3 and "pending independent acceptance" in recovered["text"]
current=x["readonly-current-unaccepted-completion"];assert "Current completion report, pending independent acceptance" in current["text"] and "Fail (reported)" in current["text"] and "3 observed periods; 3 corrections; 6 immutable history entries" in current["text"]
assert "CORRECT OBSERVATIONS FROM ENTRY" not in current["text"]
recipient=x["configured-recipient-responsibility"];assert "Your original reporting responsibility" in recipient["text"] and "Current observed progress and reported completion" not in recipient["text"]
outage=x["recipient-progress-unavailable"];assert "could not be confirmed" in outage["text"] and "Your original reporting responsibility" in outage["text"] and "Observed periods: 0" not in outage["text"]
recipient=x["recipient-separate-fixture-completion-evidence"];assert "Current observed progress and reported completion" in recipient["text"] and "does not close this ongoing responsibility" in recipient["text"] and "Pass (reported)" in recipient["text"] and "Independent inspection acceptance remains outstanding" in recipient["text"]
assert "Signed test certificate" not in x["other-company-recipient-denied"]["text"] and "Current observed progress and reported completion" not in x["other-company-recipient-denied"]["text"]
restored=x["restored-recipient-completion-evidence"];assert "Current observed progress and reported completion" in restored["text"] and restored["metadata"]["assignmentWrites"]==0 and restored["metadata"]["recordRequests"]==3 and restored["metadata"]["recoverRequests"]==3
for o in x.values():assert "\ufffd" not in o["text"] and "\u00c2" not in o["text"]
print("18 saved Chromium observations passed: completion/withdrawal, explicit input, encrypted retention, readonly recovery, recipient progress/outage and company isolation. Recorder and recipient fixtures are separate operations.")
