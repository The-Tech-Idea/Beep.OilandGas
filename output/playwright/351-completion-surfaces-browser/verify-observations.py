from pathlib import Path
import json

root = Path(__file__).parent
read = lambda name: (root / name).read_text(encoding="utf-8-sig")
form, required, frozen, hidden = (read(name + ".txt") for name in ("form", "required", "frozen", "hidden"))
assert 'combobox "Configured completion review workflow"' in form
assert 'combobox "Current completion review sender role"' in form
assert 'textbox "Reason for independent completion review*"' in form
assert 'heading "Confirm original independent completion review"' not in form
assert 'Confirm the current claim, configured independent recipients, sender role/workspace and explicit review reason' in required
assert 'heading "Confirm original independent completion review"' not in required
assert 'heading "Confirm original independent completion review"' in frozen
assert 'Independent inspector must review the signed completion claim and failed test reference.' in frozen
assert 'sender role ConfiguredExecutionRecorder / DRILLING_RECORDER' in frozen
assert 'Fail (reported)' in frozen and 'Independent inspection discrepancy remains unresolved' in frozen
assert 'role ConfiguredCompletionReviewer; workspaces INDEPENDENT_COMPLETION' in frozen
assert 'button "Confirm and retain completion review"' in frozen
assert 'Confirm original independent completion review' not in hidden
assert 'Independent inspector must review the signed completion claim' not in hidden
assert 'Claim drilling-report-' not in hidden
before = json.loads(read("metadata-before-context.json"))
after = json.loads(read("metadata-after-context.json"))
assert before == after
assert after["recordedReports"] == 4 and after["completionRoutingReads"] == 1
for key in ("recordRequests", "recoverRequests", "assignmentWrites", "assignmentRecoveries", "reportConflictConfirmations"):
    assert after[key] == 0
result = {"status": "passed", "observations": 4, "scope": ["required explicit form and rejected empty preparation", "frozen original claim/reason/role/evidence/stage confirmation", "company switch hides private preparation", "unchanged controlled fixture diagnostics"], "browser_submission_or_recovery_tested": False, "screenshot_visually_inspected": True, "console": "Initial favicon404 retained; no application exception observed. No full keyboard or accessibility qualification."}
(root / "observed-evidence.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
print(json.dumps(result))
