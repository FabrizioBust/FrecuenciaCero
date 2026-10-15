import pathlib, struct, array, json, math, statistics, base64, sys, wave
ROOT=pathlib.Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Assets/Audio/Sonidos'
OUT=ROOT/'Temp/AudioInicial'
def decode(path):
    data=path.read_bytes()
    if data[:4]==b'FORM':
        chunks={};pos=12
        while pos+8<=len(data):
            tag=data[pos:pos+4];size=int.from_bytes(data[pos+4:pos+8],'big');chunks[tag]=data[pos+8:pos+8+size];pos+=8+size+(size&1)
        comm=chunks[b'COMM'];channels,frames,bits=struct.unpack_from('>hIh',comm);exponent=int.from_bytes(comm[8:10],'big');mantissa=int.from_bytes(comm[10:18],'big');rate=round(mantissa*2.0**((exponent&32767)-16383-63));kind=comm[18:22] if data[8:12]==b'AIFC' else b'NONE';snd=chunks[b'SSND'];offset=int.from_bytes(snd[:4],'big');pcm=snd[8+offset:];width=bits//8;order='little' if kind==b'sowt' else 'big'
        if kind in [b'fl32',b'FL32',b'fl64',b'FL64']:values=array.array('f',(struct.unpack_from('>f' if bits==32 else '>d',pcm,i)[0] for i in range(0,len(pcm),width)))
        elif kind in [b'NONE',b'twos',b'sowt']:values=array.array('f',(int.from_bytes(pcm[i:i+width],order,signed=True)/(1<<(bits-1)) for i in range(0,min(len(pcm),frames*channels*width),width)))
        else:raise ValueError('Unsupported AIFC compression '+str(kind))
        mono=array.array('f',(sum(values[i:i+channels])/channels for i in range(0,len(values),channels))) if channels>1 else values
        return mono,rate,channels,bits
    if data[:4]!=b'RIFF': raise ValueError('Not RIFF WAV')
    chunks={};pos=12
    while pos+8<=len(data):
        tag=data[pos:pos+4];size=struct.unpack_from('<I',data,pos+4)[0];chunks[tag]=data[pos+8:pos+8+size];pos+=8+size+(size&1)
    fmt,channels,rate,byte_rate,alignment,bits=struct.unpack_from('<HHIIHH',chunks[b'fmt ']);pcm=chunks[b'data']
    if fmt==65534: fmt=struct.unpack_from('<H',chunks[b'fmt '],24)[0]
    if fmt==3:
        values=array.array('f' if bits==32 else 'd');values.frombytes(pcm)
    elif bits==16:
        ints=array.array('h');ints.frombytes(pcm);values=array.array('f',(v/32768 for v in ints))
    elif bits==24:
        values=array.array('f',((int.from_bytes(pcm[i:i+3],'little',signed=True))/8388608 for i in range(0,len(pcm),3)))
    elif bits==32:
        ints=array.array('i');ints.frombytes(pcm);values=array.array('f',(v/2147483648 for v in ints))
    else: raise ValueError('Unsupported bit depth '+str(bits))
    mono=array.array('f',(sum(values[i:i+channels])/channels for i in range(0,len(values),channels))) if channels>1 else values
    return mono,rate,channels,bits
def analyze(path):
    x,sr,ch,bits=decode(path);window=max(1,sr//100);rms=[]
    for i in range(0,len(x),window):
        part=x[i:i+window];rms.append(math.sqrt(sum(v*v for v in part)/len(part)))
    maxima=[(round(i*.01,2),round(20*math.log10(max(v,1e-10)),1)) for i,v in enumerate(rms) if i>0 and i<len(rms)-1 and v>rms[i-1] and v>=rms[i+1]]
    largest=sorted(maxima,key=lambda p:p[1],reverse=True);spaced=[]
    for p in largest:
        if all(abs(p[0]-q[0])>.2 for q in spaced):spaced.append(p)
        if len(spaced)>=32:break
    return dict(file=path.name,duration=len(x)/sr,rate=sr,channels=ch,bits=bits,peak=max(abs(v) for v in x),rms=math.sqrt(sum(v*v for v in x)/len(x)),peak_times=sorted(spaced),envelope=rms)
def write_wave(path,x,sr=22050):
    a=array.array('h',(int(max(-.999969,min(.999969,v))*32768) for v in x))
    with wave.open(str(path),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(sr);w.writeframes(a.tobytes())
if __name__=='__main__':
    if len(sys.argv)>1 and sys.argv[1]=='listen':
        x,sr,_,_=decode(SOURCE/sys.argv[2]);a=float(sys.argv[3]);b=float(sys.argv[4]);clip=x[int(a*sr):int(b*sr)];factor=max(1,sr//8000);clip=array.array('f',(sum(clip[i:i+factor])/len(clip[i:i+factor]) for i in range(0,len(clip),factor)));target=OUT/('Escucha-'+pathlib.Path(sys.argv[2]).stem+'-'+str(a)+'.wav');write_wave(target,clip,sr//factor);print(base64.b64encode(target.read_bytes()).decode())
    else:
        report=[analyze(p) for p in SOURCE.glob('*.wav')];(OUT/'Analisis.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps([{k:v for k,v in r.items() if k!='envelope'} for r in report],indent=2))
