import json,re
from pathlib import Path
p=Path(__file__).resolve().parent
raw=(p/'observations.log').read_text(encoding='utf-8-sig')
result=json.loads(raw.split('### Result\n',1)[1].split('\n### Ran',1)[0])
(p/'observations.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf-8')
o={x['label']:x for x in result['observations']}
a=o['recorded-lost-acknowledgement'];r=o['retained-readonly-reloaded'];i=o['inspected-retained'];c=o['clear-failed-known-proof'];u=o['outage-keeps-checked-proof'];f=o['explicit-rechecked-clear']
checks={}
def check(name,condition):
 assert condition,name
 checks[name]=True
check('one_controlled_record_and_lost_ack_original',a['metadata']['recordRequests']==1 and len(a['storage'])==1 and 'Retained original drilling report' in a['text'])
check('no_stale_history_or_recorder_form_after_submission','0 recorded observed reports' not in a['text'] and 'Report observed drilling period' not in a['text'] and 'Recording requires current' not in a['text'])
check('readonly_reload_precedes_source_and_profile_lookup',r['metadata']['readOnly'] and r['metadata']['sourceRequests']==a['metadata']['sourceRequests'] and r['metadata']['profileLoads']==a['metadata']['profileLoads'])
check('large_original_is_encrypted',len(r['storage'])==1 and r['storage'][0]['length']>16000 and not r['storage'][0]['plaintext'])
check('inspection_preserves_original_and_both_observation_sources',i['storage']==r['storage'] and 'Different unrecorded browser site log 471' in i['text'] and 'Recorded source: Recorded browser site log 472' in i['text'] and 'MD 100 to 120 m' in i['text'] and 'MD 100 to 150 m' in i['text'])
check('failed_clear_preserves_proof_and_original',len(c['storage'])==1 and 'Clearing the tab input is not confirmed' in c['text'] and 'Different recorded observations at the original report position' in c['text'])
stamp=lambda x:re.findall(r'Checked (.{20}) UTC',x['text'])
check('outage_preserves_earlier_checked_stamp',len(u['storage'])==1 and stamp(c)==stamp(u) and bool(stamp(c)) and 'The reviewed report history is unconfirmed' in u['text'])
for label in ['other-company-denied','other-account-denied']:
 x=o[label];check(label,len(x['storage'])==1 and 'Recorded source: Recorded browser site log 472' not in x['text'] and 'Different unrecorded browser site log 471' not in x['text'])
check('explicit_recheck_clears_only_original_without_another_write',not f['storage'] and f['metadata']['recordRequests']==1 and f['metadata']['recordedReports']==1 and f['metadata']['assignmentWrites']==0 and f['metadata']['reportConflictReads']==2 and f['metadata']['reportConflictConfirmations']==3 and 'You kept the exact different recorded report' in f['text'] and 'Choose the company with drilling' not in f['text'])
(p/'browser-verification.json').write_text(json.dumps({'passed':len(checks),'checks':checks,'encrypted_original_characters':r['storage'][0]['length'],'final_metadata':f['metadata']},indent=2),encoding='utf-8')
print(json.dumps({'passed':len(checks),'encrypted_original_characters':r['storage'][0]['length']}))
