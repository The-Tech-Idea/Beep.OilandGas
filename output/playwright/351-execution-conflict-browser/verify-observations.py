from pathlib import Path
import json,re
p=Path(__file__).resolve().parent
s=(p/'verified-observations.txt').read_text(encoding='utf-8-sig');v=json.loads(s.split('### Result\n',1)[1].split('\n### Ran Playwright code',1)[0]);o={x['label']:x for x in v['observations']};checks=[]
def check(name,condition):
    assert condition,name
    checks.append({'check':name,'passed':True})
a=o['retained-readonly-reloaded'];b=o['inspected-retained'];c=o['clear-failed-known-proof'];d=o['outage-keeps-checked-proof'];e=o['explicit-rechecked-clear']
check('Readonly unmapped whole reload restores encrypted original before another source/profile lookup',a['metadata']['readOnly'] and len(a['storage'])==1 and not a['storage'][0]['plaintext'] and a['metadata']['sourceRequests']==2 and a['metadata']['profileLoads']==2)
check('Inspection keeps original and exposes exact different recorded source before acknowledgement',len(b['storage'])==1 and 'Different recorded execution requires your review' in b['text'] and b['metadata']['conflictReads']==1 and b['metadata']['conflictConfirmations']==0)
check('Removal failure preserves original and names explicit conflict confirmation',len(c['storage'])==1 and 'Clearing the tab input is not confirmed' in c['text'] and 'again to recheck the conflict and finish' in c['text'] and 'recover again to finish' not in c['text'])
stamp=lambda t:re.search(r'Conflict checked ([^\n]+)',t).group(1)
check('Later lookup outage preserves earlier checked proof and original',len(d['storage'])==1 and stamp(c['text'])==stamp(d['text']) and d['metadata']['recoveryUnavailable'])
for label in ['other-company-denied','other-account-denied']:
    x=o[label];check(label+' hides private source and keeps original',len(x['storage'])==1 and 'Different recorded execution requires your review' not in x['text'] and 'Actual observed start reported by the configured company recorder' not in x['text'])
check('Restored-scope explicit recheck clears exact original',not e['storage'] and 'You kept the exact different recorded execution' in e['text'] and e['metadata']['conflictConfirmations']==3 and e['metadata']['conflictReads']==2)
check('Completed review has no unrelated company-selection warning','Different recorded execution requires your review' in e['text'] and 'Choose a company with drilling, workflow, planning and financial view access' not in e['text'])
check('No report/assignment write or blind original-recovery request',all(x['metadata']['recordRequests']==x['metadata']['assignmentWrites']==x['metadata']['startRecoverRequests']==0 for x in o.values()))
check('Final page console has zero errors/warnings','Errors: 0, Warnings: 0' in (p/'verified-console.txt').read_text(encoding='utf-8-sig'))
result={'checks':checks,'encrypted_characters':a['storage'][0]['length'],'observations':v['observations'],'limits':['Controlled business responses; actual page/state/encrypted storage/component transport; removal/outage injected.','Native/API authority and persistence are separately qualified.','No full host/OIDC/provider/concurrency/browser matrix/RTL/a11y/performance/multinational proof.']};(p/'browser-verification.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf-8');print(json.dumps({'passed':len(checks),'encrypted_characters':result['encrypted_characters']}))
