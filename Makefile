OUTPUT_DIR = ./Build/Output/
PROGRAM_NAME = TuxPaperEngine

build:
	rm -rf ${OUTPUT_DIR}/*
	
	mkdir -p ${OUTPUT_DIR}/${PROGRAM_NAME}
	dotnet publish AvaloniaUI/AvaloniaUI.csproj \
		-c Release \
		-r linux-x64 \
		--self-contained true \
		/p:PublishSingleFile=false \
		-o ${OUTPUT_DIR}/${PROGRAM_NAME}

publish:
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
	
	ARCH=x86_64 appimagetool ${OUTPUT_DIR}/${PROGRAM_NAME}.AppDir ${OUTPUT_DIR}/${PROGRAM_NAME}.appimage
	chmod +x ${OUTPUT_DIR}/${PROGRAM_NAME}.appimage