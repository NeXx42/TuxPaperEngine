OUTPUT_DIR = ./Build/Output/
PROGRAM_NAME = TuxPaperEngine

publish:

	rm -rf ${OUTPUT_DIR}
	
	# app
	dotnet publish AvaloniaUI/AvaloniaUI.csproj \
		-c Release \
		-r linux-x64 \
		--self-contained true \
		/p:PublishSingleFile=false \
		/p:IncludeAllContentForSelfExtract=true \
		-o ${OUTPUT_DIR}/${PROGRAM_NAME}
		
	cd Engine && mkdir -p build && cd build && cmake -DCMAKE_BUILD_TYPE='Release' .. && make
	cp -r Engine/build/output ${OUTPUT_DIR}/${PROGRAM_NAME}/Engine
		
	#tar -czvf ${OUTPUT_DIR}/GameLibrary.Avalonia.tar.gz -C ${OUTPUT_DIR} Avalonia
	
publish-appimage:
	rm -rf ${OUTPUT_DIR}/*
	
	mkdir -p ${OUTPUT_DIR}/${PROGRAM_NAME}
	dotnet publish AvaloniaUI/AvaloniaUI.csproj \
		-c Release \
		-r linux-x64 \
		--self-contained true \
		/p:PublishSingleFile=false \
		-o ${OUTPUT_DIR}/${PROGRAM_NAME}
		
	mkdir -p ${OUTPUT_DIR}/${PROGRAM_NAME}.AppDir/usr/bin
	
	cp -r ${OUTPUT_DIR}/${PROGRAM_NAME}/* ${OUTPUT_DIR}/${PROGRAM_NAME}.AppDir/usr/bin
	cp ./Build/AppImageData/* ${OUTPUT_DIR}/${PROGRAM_NAME}.AppDir
	
	appimagetool ${OUTPUT_DIR}/${PROGRAM_NAME}.AppDir ${OUTPUT_DIR}/${PROGRAM_NAME}.appimage
	chmod +x ${OUTPUT_DIR}/${PROGRAM_NAME}.appimage
	
#engine:
#	cd Engine && mkdir -p build && cd build && cmake -DCMAKE_BUILD_TYPE='Release' .. && make