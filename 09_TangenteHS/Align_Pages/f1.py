from common_fs import get_all_files

nf=0
with open(r'c:\temp\bds.txt', 'w', encoding='utf-8') as fout:
    for file in get_all_files(r'\\teraz\books\bd'):
        if not 'Comics' in file and not 'International' in file and not 'Revues' in file and not 'ZipRar' in file:
            fout.write(file+'\r\n')
            nf+=1
            if nf%1000==0:
                print(nf)

print('Done.')
