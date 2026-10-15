import pathlib, json, wave, array, math, hashlib
root=pathlib.Path(__file__).resolve().parents[2];folder=root/'Assets/Audio/Procesados';recipe=json.loads((folder/'PreparacionAudio.json').read_text(encoding='utf-8'));results=[]
for name,expected in recipe['original_sha256'].items():
    if hashlib.sha256((root/'Assets/Audio/Sonidos'/name).read_bytes()).hexdigest()!=expected:raise ValueError('Original changed: '+name)
for clip in recipe['clips']:
    path=folder/clip['file']
    if hashlib.sha256(path.read_bytes()).hexdigest()!=clip['sha256']:raise ValueError('Processed clip changed: '+str(path))
    with wave.open(str(path),'rb') as w:
        if w.getnchannels()!=1 or w.getsampwidth()!=2 or w.getframerate()!=44100:raise ValueError('Unexpected PCM format '+clip['file'])
        x=array.array('h',w.readframes(w.getnframes()));peak=max(abs(v) for v in x)/32768
        if not 0<peak<.5:raise ValueError('Silent or over-level clip '+clip['file'])
        if not clip['loop'] and (abs(x[0])>1 or abs(x[-1])>1):raise ValueError('Nonzero cut edge '+clip['file'])
        if clip['file'].startswith('Pasos/') and len(x)/44100>.27:raise ValueError('Footstep is too long')
        results.append({'clip':clip['file'],'duration':len(x)/44100,'peak_db':20*math.log10(peak),'valid':True})
report={'originals_unchanged':len(recipe['original_sha256']),'processed_valid':len(results),'checks':results}
output=root/'Temp/AudioInicial/ValidacionSenal.json';output.parent.mkdir(parents=True,exist_ok=True);output.write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps({k:v for k,v in report.items() if k!='checks'},indent=2))
