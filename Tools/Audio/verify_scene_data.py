import pathlib,re,json
root=pathlib.Path(__file__).resolve().parents[2];copy=root/'Temp/AudioVerificationUnity';protected={'20','50','54','58','60','61'};guids=set()
for name in ['Player/LeoMovement','Player/MovementNoise','Player/SurvivalPlayer','World/Escarbador','Camera/SmoothCameraFollow','Camera/ForestCameraBounds']:
    text=(root/('Assets/Scripts/'+name+'.cs.meta')).read_text(encoding='utf-8-sig');guids.add(re.search(r'guid: (\w+)',text)[1])
def blocks(p):
    text=p.read_text(encoding='utf-8-sig');parts=re.split(r'(?m)^--- !u!(\d+) &([^\r\n ]+).*\n',text);return {(parts[i],parts[i+1]):parts[i+2].replace('\r','').strip() for i in range(1,len(parts),3)}
checked=0;differences=[]
for p in (root/'Assets/Scenes').rglob('*.unity'):
    q=copy/p.relative_to(root)
    if not q.exists():continue
    before=blocks(p);after=blocks(q)
    for key,value in before.items():
        if key[0] in protected or(key[0]=='114' and any(g in value for g in guids)):
            checked+=1
            if after.get(key)!=value:differences.append({'scene':str(p.relative_to(root)),'component':key})
report={'protected_components_compared':checked,'differences':differences};(root/'Temp/AudioInicial/ProteccionJugabilidad.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report,indent=2));raise SystemExit(bool(differences))
