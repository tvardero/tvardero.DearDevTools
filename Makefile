publish-uikit:
	dotnet publish ./src/ta.UIKit/ --output dist/src-ta.UIKit
	rm -rf ./dist/ta.UIKit/
	mkdir -p ./dist/ta.UIKit/
	mkdir -p ./dist/ta.UIKit/plugins/
	cp -r ./dist/src-ta.UIKit/* ./dist/ta.UIKit/plugins/
	cp ./src/ta.UIKit/modinfo.json ./dist/ta.UIKit/
	cp ./src/ta.UIKit/workshopdata.json ./dist/ta.UIKit/
	cp ./src/ta.UIKit/thumbnail.png ./dist/ta.UIKit/
	rm -rf ./dist/src-ta.UIKit/