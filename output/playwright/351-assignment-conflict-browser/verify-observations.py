from pathlib import Path
import json,re
p=Path(__file__).resolve().parent
def result(name):
 s=(p/name).read_text(encoding='utf-8-sig');return json.loads(s.split('### Result\n',1)[1].split('\n### Ran Playwright code',1)[0])
v=result('observations.txt');o={x['label']:x for x in v['observations']};a=result('retained-observation.txt');b=o['inspected-retained'];c=o['clear-failed-known-proof'];d=o['outage-keeps-checked-proof'];e=o['explicit-rechecked-clear'];initial=result('initial-metadata.txt');checks=[]
def check(name,condition):
 assert condition,name
 checks.append({'check':name,'passed':True})
check('Readonly unmapped whole reload restores encrypted different original without another source/profile lookup',a['metadata']['readOnly'] and len(a['storage'])==1 and not a['storage'][0]['plaintext'] and a['metadata']['assignmentReads']==initial['metadata']['assignmentReads'] and a['metadata']['profileLoads']==initial['metadata']['profileLoads'])
check('Inspection preserves original and shows exact sender/role/persona/recipient/source',len(b['storage'])==1 and b['metadata']['assignmentConflictReads']==1 and 'Recorded recipient site-recorder-1' in b['text'] and 'ConfiguredReportingRole' in b['text'] and 'Different recorded reporting responsibility' in b['text'])
check('Injected removal failure keeps original/proof and explicit recheck guidance',len(c['storage'])==1 and 'Clearing the tab input is not confirmed' in c['text'] and 'again to recheck and finish' in c['text'])
stamp=lambda x:re.search(r'Conflict checked ([^\n]+)',x).group(1)
check('Confirmation outage preserves exact prior checked stamp and original',len(d['storage'])==1 and stamp(c['text'])==stamp(d['text']) and d['metadata']['recoveryUnavailable'])
for label in ['other-company-denied','other-account-denied']:
 x=o[label];check(label+' hides private recorded recipients and keeps protected original',len(x['storage'])==1 and 'Different recorded reporting responsibility' not in x['text'] and 'Recorded recipient site-recorder-1' not in x['text'])
check('Restored account explicitly rechecks and clears only retained input',not e['storage'] and e['metadata']['assignmentConflictReads']==2 and e['metadata']['assignmentConflictConfirmations']==3 and 'You kept the exact different recorded responsibility' in e['text'])
check('Reviewed source remains visible without false read-only warning','Different recorded reporting responsibility' in e['text'] and 'Read-only source.' not in e['text'])
check('Reconciliation neither writes another assignment nor records reports',all(x['metadata']['assignmentWrites']==1 and x['metadata']['recordRequests']==0 and x['metadata']['assignmentRecoveries']==1 for x in o.values()))
check('Final page has zero console errors/warnings','Errors: 0, Warnings: 0' in (p/'console.txt').read_text(encoding='utf-8-sig'))
r={'checks':checks,'encrypted_characters':a['storage'][0]['length'],'initial':initial,'retained_before_inspection':a,'observations':v['observations'],'limits':['Actual page/state/encrypted browser storage/component transport, with controlled typed business responses.','Initial assignment uses controlled lost acknowledgement; removal failure and recovery outage injected.','Native/API persistence/current authority have separate gates; no full host/OIDC/provider/concurrency/browser matrix/RTL/a11y/performance/multinational qualification.']};(p/'browser-verification.json').write_text(json.dumps(r,indent=2,ensure_ascii=False),encoding='utf-8');print(json.dumps({'checks_passed':len(checks),'encrypted_characters':r['encrypted_characters']}))
