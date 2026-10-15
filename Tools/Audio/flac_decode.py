"""Local FLAC-to-PCM decoder for the supplied asset; stdlib only.
Format reference: https://www.rfc-editor.org/rfc/rfc9639.html
Checks frame CRC and the source STREAMINFO PCM MD5 before publishing decoded data.
Original file is always read-only. This is tooling, not a runtime game dependency.
"""
import pathlib, hashlib, array, json, struct
class Bits:
    def __init__(self,data,pos):self.data=data;self.pos=pos*8
    def read(self,n):
        if not n:return 0
        p=self.pos;first=p>>3;end=(p+n+7)>>3
        if end>len(self.data):raise ValueError('Truncated bitstream')
        v=int.from_bytes(self.data[first:end],'big');v=(v>>(end*8-p-n))&((1<<n)-1);self.pos+=n;return v
    def signed(self,n):
        v=self.read(n);return v-(1<<n) if n and v&(1<<(n-1)) else v
    def unary(self):
        count=0
        while self.read(1)==0:
            count+=1
            if count>1000000:raise ValueError('Invalid Rice code')
        return count
def crc_table(poly,bits):
    table=[]
    for b in range(256):
        v=b<<(bits-8)
        for _ in range(8):v=((v<<1)^poly if v&(1<<(bits-1)) else v<<1)&((1<<bits)-1)
        table.append(v)
    return table
C16=crc_table(0x8005,16);C8=crc_table(0x07,8)
def crc(data,table,bits):
    v=0
    for b in data:v=((v<<8)^table[(v>>(bits-8))^b])&((1<<bits)-1)
    return v
def residual(b,block,order):
    method=b.read(2)
    if method>1:raise ValueError('Reserved residual method')
    ricebits=4+method;partition=b.read(4);count=block>>partition;out=[]
    for p in range(1<<partition):
        parameter=b.read(ricebits);n=count-(order if p==0 else 0)
        if parameter==(1<<ricebits)-1:
            width=b.read(5);out.extend(b.signed(width) for _ in range(n))
        else:
            for _ in range(n):
                v=(b.unary()<<parameter)|b.read(parameter);out.append((v>>1)^-(v&1))
    return out
def subframe(b,block,depth):
    if b.read(1):raise ValueError('Reserved subframe bit')
    kind=b.read(6);wasted=b.unary()+1 if b.read(1) else 0;depth-=wasted
    if kind==0:out=[b.signed(depth)]*block
    elif kind==1:out=[b.signed(depth) for _ in range(block)]
    elif 8<=kind<=12:
        order=kind-8;out=[b.signed(depth) for _ in range(order)]
        for v in residual(b,block,order):
            if order==0:pred=0
            elif order==1:pred=out[-1]
            elif order==2:pred=2*out[-1]-out[-2]
            elif order==3:pred=3*out[-1]-3*out[-2]+out[-3]
            else:pred=4*out[-1]-6*out[-2]+4*out[-3]-out[-4]
            out.append(v+pred)
    elif 32<=kind<=63:
        order=kind-31;out=[b.signed(depth) for _ in range(order)];precision=b.read(4)
        if precision==15:raise ValueError('Reserved LPC precision')
        precision+=1;shift=b.signed(5);co=[b.signed(precision) for _ in range(order)]
        for v in residual(b,block,order):
            pred=sum(c*out[-i-1] for i,c in enumerate(co));pred=pred>>shift if shift>=0 else pred<<(-shift);out.append(v+pred)
    else:raise ValueError('Reserved subframe type '+str(kind))
    if wasted:out=[v<<wasted for v in out]
    return out
def decode_flac(path):
    data=path.read_bytes()
    if data[:4]!=b'fLaC':raise ValueError('FLAC signature absent')
    pos=4;info=None
    while True:
        flag=data[pos];size=int.from_bytes(data[pos+1:pos+4],'big');payload=data[pos+4:pos+4+size];pos+=4+size
        if flag&127==0:info=payload
        if flag&128:break
    packed=int.from_bytes(info[10:18],'big');rate=packed>>44;channels=((packed>>41)&7)+1;depth=((packed>>36)&31)+1;total=packed&((1<<36)-1);expected=info[18:34]
    mono=array.array('f');md5=hashlib.md5();frames=0;width=(depth+7)//8;minimum=-(1<<(depth-1));maximum=(1<<(depth-1))-1
    while len(mono)<total:
        start=pos;b=Bits(data,pos)
        if b.read(14)!=0x3ffe or b.read(1)!=0:raise ValueError('Invalid frame sync')
        b.read(1);blockcode=b.read(4);sr=b.read(4);assignment=b.read(4);samplecode=b.read(3)
        if b.read(1):raise ValueError('Reserved frame bit')
        first=b.read(8)
        if first>=128:
            count=0;mask=128
            while first&mask:count+=1;mask>>=1
            for _ in range(count-1):
                if b.read(8)&0xc0!=0x80:raise ValueError('Invalid coded frame number')
        if blockcode==1:block=192
        elif 2<=blockcode<=5:block=576<<(blockcode-2)
        elif blockcode==6:block=b.read(8)+1
        elif blockcode==7:block=b.read(16)+1
        elif blockcode>=8:block=256<<(blockcode-8)
        else:raise ValueError('Reserved block size')
        if sr==12:b.read(8)
        elif sr in (13,14):b.read(16)
        headerend=b.pos//8
        if b.read(8)!=crc(data[start:headerend],C8,8):raise ValueError('Header CRC mismatch')
        framedepth={0:depth,1:8,2:12,4:16,5:20,6:24,7:32}.get(samplecode)
        if framedepth!=depth:raise ValueError('Changing bit depth not supported')
        count=assignment+1 if assignment<8 else 2
        if count!=channels:raise ValueError('Channel mismatch')
        samples=[subframe(b,block,depth+(1 if (assignment in (8,10) and c==1)or(assignment==9 and c==0)else 0)) for c in range(count)]
        b.pos=(b.pos+7)//8*8;footer=b.pos//8;stored=b.read(16)
        if stored!=crc(data[start:footer],C16,16):raise ValueError('Frame CRC mismatch')
        pos=b.pos//8;pcm=bytearray()
        for i in range(block):
            if assignment<8:values=[a[i] for a in samples]
            elif assignment==8:values=[samples[0][i],samples[0][i]-samples[1][i]]
            elif assignment==9:values=[samples[0][i]+samples[1][i],samples[1][i]]
            else:
                side=samples[1][i];mid=(samples[0][i]<<1)|(side&1);values=[(mid+side)>>1,(mid-side)>>1]
            if any(v<minimum or v>maximum for v in values):raise ValueError('Decoded PCM overflow')
            for v in values:pcm.extend(v.to_bytes(width,'little',signed=True))
            mono.append(sum(values)/channels/(1<<(depth-1)))
        md5.update(pcm);frames+=1
    if len(mono)!=total or md5.digest()!=expected:raise ValueError('PCM count or STREAMINFO MD5 mismatch')
    return mono,rate,dict(file=path.name,rate=rate,channels=channels,bits=depth,samples=total,duration=total/rate,frames=frames,pcm_md5=md5.hexdigest(),md5_verified=True)
if __name__=='__main__':
    root=pathlib.Path(__file__).resolve().parents[2];x,sr,report=decode_flac(root/'Assets/Audio/Sonidos/agua_rio.flac');output=root/'Temp/AudioInicial';(output/'Rio-Decodificado.f32').write_bytes(x.tobytes());(output/'Rio-Decodificado.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report,indent=2))
