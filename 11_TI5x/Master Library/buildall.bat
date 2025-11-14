del /f /q all.txt
for %%f in (*.t59) do (
	echo # %%f >>all.txt
	rcat <%%f >>all.txt
	echo END >>all.txt
)
	