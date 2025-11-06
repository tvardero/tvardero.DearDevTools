publish-first-mod:
	dotnet publish ./src/ta.FirstMod/ --output dist/src-ta.FirstMod
	rm -rf ./dist/ta.FirstMod/
	mkdir -p ./dist/ta.FirstMod/
	mkdir -p ./dist/ta.FirstMod/plugins/
	cp ./dist/src-ta.FirstMod/ta.FirstMod.dll ./dist/ta.FirstMod/plugins/
	cp ./dist/src-ta.FirstMod/ta.FirstMod.pdb ./dist/ta.FirstMod/plugins/
	cp ./src/ta.FirstMod/modinfo.json ./dist/ta.FirstMod/
	cp ./src/ta.FirstMod/workshopdata.json ./dist/ta.FirstMod/
	cp ./src/ta.FirstMod/thumbnail.png ./dist/ta.FirstMod/
	rm -rf ./dist/src-ta.FirstMod/
