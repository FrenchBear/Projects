pushd THS75\01Scans
for %%f in (*.jpg) do "c:\Program Files\ImageMagick-7\convert.exe" %%f -deskew 40%% ..\02Redresse\%%f
popd
