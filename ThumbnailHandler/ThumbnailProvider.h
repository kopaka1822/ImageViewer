#pragma once

#include <windows.h>
#include <thumbcache.h>     // For IThumbnailProvider
#include <wincodec.h>       // Windows Imaging Codecs
#include <string>
#include "../ConsoleTest/ImageFramework.h"

#pragma comment(lib, "windowscodecs.lib")


class ThumbnailProvider :
	public IInitializeWithFile,
	public IThumbnailProvider
{
public:
	// IUnknown
	IFACEMETHODIMP QueryInterface(REFIID riid, void** ppv);
	IFACEMETHODIMP_(ULONG) AddRef();
	IFACEMETHODIMP_(ULONG) Release();


	HRESULT Initialize(LPCWSTR pszFilePath, DWORD grfMode) override;

	// IThumbnailProvider
	IFACEMETHODIMP GetThumbnail(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha) override;

	ThumbnailProvider();

protected:
	~ThumbnailProvider();
	
private:
	// Reference count of component.
	long m_cRef;
	//ImageFramework::Model m_model;
	int m_width;
	int m_height;
};