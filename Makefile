publish-rwimgui:
	make publish ta.RWImGui

publish: $(project)
	rm -rf dist/src-$(project)/
	dotnet publish src/$(project) --output dist/src-$(project)/
	mkdir -p dist/$(project)/
	cp -r dist/src-$(project)/* dist/$(project)/plugins/
	cp src/$(project)/modinfo.json dist/$(project) 
	(cp src/$(project)/workshopdata.json dist/$(project) || exit 0)
	(cp src/$(project)/thumbnail.png dist/$(project) || exit 0)
	rm -rf ./dist/src-$(project)/
	
clean: 
	rm -rf dist/