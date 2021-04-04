The thumbnail handler is configured for the following formats:
.dds
.ktx
.ktx2
.pfm
.hdr
.exr

REGISTER (PowerShell as administrator):
regsvr32.exe .\ThumbnailHandler.dll

UNREGISTER:
regsvr32.exe /u .\ThumbnailHandler.dll