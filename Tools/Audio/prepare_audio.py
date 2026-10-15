"""Reproducible preparation of Frecuencia Cero's own supplied sounds.
Python standard library only; never modifies originals. Writes 16-bit mono PCM WAV.
"""
from audio_formats import decode, write_wave
from flac_decode import decode_flac
import pathlib, array, math, json, hashlib, random
ROOT=pathlib.Path(__file__).resolve().parents[2];SOURCE=ROOT/'Assets/Audio/Sonidos';OUTPUT=ROOT/'Assets/Audio/Procesados'
records=[];cache={}
def load(name):
    if name not in cache:
        if name.endswith('.flac'):
            x,sr,info=decode_flac(SOURCE/name);cache[name]=(x,sr);(OUTPUT/'ConversionRio.json').write_text(json.dumps(info,indent=2),encoding='utf-8')
        else:
            x,sr,_,_=decode(SOURCE/name);cache[name]=(x,sr)
    return cache[name]
def resample(x,sr,target=44100):
    if sr==target:return x
    # Windowed-sinc polyphase coefficients. Anti-aliasing before downsampling.
    ratio=sr/target;cutoff=min(1,target/sr)*.94;radius=16;phases=512;table=[]
    for p in range(phases):
        frac=p/phases;co=[]
        for k in range(-radius+1,radius+1):
            d=k-frac;z=math.pi*d*cutoff;v=cutoff*(math.sin(z)/z if z else 1)*(.5+.5*math.cos(math.pi*d/radius));co.append(v)
        total=sum(co);table.append([v/total for v in co])
    result=array.array('f');last=len(x)-1
    for i in range(round(len(x)/ratio)):
        position=i*ratio;base=int(position);co=table[min(phases-1,int((position-base)*phases))];value=0
        for j,v in enumerate(co):value+=x[min(last,max(0,base+j-radius+1))]*v
        result.append(value)
    return result
def emit(name,source,start,end,target_db,peak_db=-7,fade_in=.004,fade_out=.022,hp=35,loop=False):
    raw,sr=load(source);x=array.array('f',raw[int(start*sr):min(len(raw),int(end*sr))]);x=resample(x,sr);sr=44100
    mean=sum(x)/len(x);alpha=math.exp(-2*math.pi*hp/sr);old=0;filtered=0
    for i,v in enumerate(x):v-=mean;filtered=alpha*(filtered+v-old);old=v;x[i]=filtered
    rms=math.sqrt(sum(v*v for v in x)/len(x));peak=max(abs(v) for v in x);gain=min(10**(target_db/20)/max(rms,1e-9),10**(peak_db/20)/max(peak,1e-9),10)
    for i in range(len(x)):x[i]*=gain
    if loop:
        n=min(round(.5*sr),len(x)//8);tail=x[-n:]
        for i in range(n):t=(i+.5)/n;x[i]=tail[i]*math.cos(t*math.pi/2)+x[i]*math.sin(t*math.pi/2)
        x=x[:-n]
    else:
        ni=min(round(fade_in*sr),len(x)//3);no=min(round(fade_out*sr),len(x)//3)
        for i in range(ni):x[i]*=(i/max(1,ni-1))**2
        for i in range(no):x[-1-i]*=(i/max(1,no-1))**2
    # No hard limiter: scale the whole clip if a crossfade raises its true sample peak.
    p=max(abs(v) for v in x);ceiling=10**(peak_db/20)
    if p>ceiling:
        for i in range(len(x)):x[i]*=ceiling/p
    path=OUTPUT/name;path.parent.mkdir(parents=True,exist_ok=True);write_wave(path,x,sr)
    records.append(dict(file=name,original=source,source_start=start,source_end=end,duration=len(x)/sr,rate=sr,channels=1,format='PCM16',gain_db=round(20*math.log10(gain),3),peak_db=round(20*math.log10(max(abs(v) for v in x)),3),rms_db=round(20*math.log10(math.sqrt(sum(v*v for v in x)/len(x))),3),fade_in=fade_in,fade_out=fade_out,high_pass_hz=hp,loop=loop,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
def footstep(bank,source,centers):
    for i,c in enumerate(centers):emit('Pasos/'+bank+'/'+bank+'_'+str(i+1).zfill(2)+'.wav',source,max(0,c-.065),c+.185,-24,-9,.003,.022,55)
def cabin_centers(source):
    x,sr=load(source);size=sr//100;energy=[math.sqrt(sum(v*v for v in x[i:i+size])/len(x[i:i+size])) for i in range(0,len(x),size)];peaks=[(i*.01,v) for i,v in enumerate(energy) if i>8 and i<len(energy)-22 and v>energy[i-1] and v>=energy[i+1]];chosen=[]
    for t,v in sorted(peaks,key=lambda a:a[1],reverse=True):
        if all(abs(t-c)>.55 for c in chosen):chosen.append(t)
        if len(chosen)==8:break
    return sorted(chosen)
def main():
    OUTPUT.mkdir(parents=True,exist_ok=True)
    footstep('Tierra','pasos_en_tierra.wav',[.12,.88,1.64,2.24])
    footstep('Cesped','pasos_en_grass.wav',[.39,1.41,1.97,2.31,2.87,3.72])
    footstep('MaderaReten','pasos_en_retenv2.wav',[.93,1.62,3.95,6.09,10.86,13.77,14.23,15.66])
    if(SOURCE/'pasos_en_retenv1.aiff').exists():footstep('MaderaCabana','pasos_en_retenv1.aiff',cabin_centers('pasos_en_retenv1.aiff'))
    emit('Vehiculo/MotorEnMarcha.wav','sonido_auto.wav',4,12,-24,-9,hp=25,loop=True)
    emit('Vehiculo/MotorApagandose.wav','auto_apagandose.wav',0,2.23,-27,-10,.012,.10,25)
    emit('Vehiculo/PuertaCamioneta.wav','puerta_cerrandose.wav',4.65,5.30,-26,-10,.004,.055,45)
    emit('Radio/Interferencia_01.wav','radio_interferencia.wav',.25,1.35,-31,-14,.018,.055,120)
    emit('Radio/Interferencia_02.wav','radio_interferencia.wav',7.8,8.9,-31,-14,.018,.055,120)
    emit('Radio/CorteRadio.wav','radio_interferencia.wav',9.2,9.38,-31,-14,.005,.028,120)
    emit('Agua/RioCercano.wav','agua_rio.flac',20,44,-31,-13,hp=35,loop=True)
    report=dict(missing=['pasos_en_retenv1.aiff'] if not(SOURCE/'pasos_en_retenv1.aiff').exists() else [],original_sha256={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in SOURCE.iterdir() if p.suffix.lower() in ['.wav','.flac','.aiff','.aif']},clips=records,notes=['Source WAVs/AIFF decoded as PCM16/24 or IEEE float32 without installation.','Motor initial transient 0-4 seconds excluded; only sustained engine segment selected.','Perceptual audition on headphones is required; automation validates signal levels, not semantic sound quality.','Cabin V1 foot textures preserve their natural creaks; no automatic ambient creaking is added.'])
    (OUTPUT/'PreparacionAudio.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8');print(json.dumps(dict(clips=len(records),total_seconds=round(sum(r['duration'] for r in records),2),missing=report['missing']),indent=2))
if __name__=='__main__':main()
