from capstone import *
import struct
code = open("ov0.bin", "rb")
md = Cs(CS_ARCH_ARM, CS_MODE_ARM)
md.detail = True
insts = []
dataAddrs = []
data = code.read()
for i in md.disasm(data, 0x20aa420):
    if "[pc" in i.op_str:
        dataOff = 8 + int(i.op_str.split('x')[1].split(']')[0], 16) + i.address
        num = struct.unpack_from("<I", data, dataOff - 0x20aa420)
        insts.append("0x%x:\t%s\t%s\n" %(i.address, i.mnemonic, i.op_str.split(',')[0] + ", =0x%x" %(num[0])))
        dataAddrs.append(dataOff)
    else:
        if not i.address in dataAddrs:
            insts.append("0x%x:\t%s\t%s\n" %(i.address, i.mnemonic, i.op_str))
code.close()
o = open("ov0.s", "w")
o.writelines(insts)
o.close()