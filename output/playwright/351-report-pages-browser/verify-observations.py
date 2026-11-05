import json
from pathlib import Path
p=Path(__file__).resolve().parent
raw=(p/'observations.log').read_text(encoding='utf-8-sig')
r=json.loads(raw.split('### Result\n',1)[1].split('\n### Ran',1)[0]);o={x['label']:x for x in r['observations']};checks={}
def check(name,value):
 assert value,name
 checks[name]=True
first=o['first-page'];last=o['last-page'];restored=o['restored-readonly-page']
check('utf8_observation_and_navigation_text',all('\u00c2' not in x['text'] and '\ufffd' not in x['text'] for x in r['observations']))
check('bounded_first_page_and_complete_head','Showing reports 1 to 25 of 53' in first['text'] and 'Current checked head: report 53' in first['text'] and 'Qualified observed site log 26' not in first['text'])
check('readonly_navigation_needs_no_recorder_profile',all(x['metadata']['profileLoads']==first['metadata']['profileLoads'] for x in r['observations']) and first['metadata']['readOnly'] and 'INSPECT CURRENT REPORT BASIS' not in first['text'])
check('keyboard_activation_advances_exact_page','Showing reports 26 to 50 of 53' in o['keyboard-next-page']['text'] and 'Qualified observed site log 25' not in o['keyboard-next-page']['text'])
check('last_page_is_exact_and_next_disabled','Showing reports 51 to 53 of 53' in last['text'] and r['disabledLast'] and 'Qualified observed site log 50' not in last['text'])
check('previous_returns_same_checked_first_page','Showing reports 1 to 25 of 53' in o['previous-first-page']['text'] and 'Current checked head: report 53' in o['previous-first-page']['text'])
changed=o['changed-history-refused']
check('changed_history_refuses_old_cursor_without_mixed_rows','The checked report history or current access is unconfirmed' in changed['text'] and 'Showing reports' not in changed['text'] and 'Qualified observed site log' not in changed['text'])
check('explicit_reload_reads_current_complete_head','Showing reports 1 to 25 of 54' in o['explicit-current-reload']['text'] and 'Current checked head: report 54' in o['explicit-current-reload']['text'])
for label in ['other-company-denied','other-account-denied']:
 x=o[label];check(label,'Showing reports' not in x['text'] and 'Qualified observed site log' not in x['text'] and 'could not be confirmed' in x['text'])
check('restored_scope_read_only_without_business_posts_or_original_storage','Showing reports 1 to 25 of 54' in restored['text'] and all(x['metadata']['recordRequests']==0 and x['metadata']['assignmentWrites']==0 and not x['storedOriginals'] for x in r['observations']))
(p/'observations.json').write_text(json.dumps(r,indent=2,ensure_ascii=False),encoding='utf-8');(p/'browser-verification.json').write_text(json.dumps({'passed':len(checks),'checks':checks,'final_metadata':restored['metadata']},indent=2),encoding='utf-8');print(json.dumps({'passed':len(checks)}))
