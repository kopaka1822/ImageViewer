#include "pch.h"
#include "ThumbnailProvider.h"
#include <Shlwapi.h>
#include <Wincrypt.h>   // For CryptStringToBinary.
#include <msxml6.h>
#include <vector>

#pragma comment(lib, "Shlwapi.lib")
#pragma comment(lib, "Crypt32.lib")
#pragma comment(lib, "msxml6.lib")

extern HINSTANCE g_hInst;
extern long g_cDllRef;

ThumbnailProvider::ThumbnailProvider()
	:
m_cRef(1)
{
	InterlockedIncrement(&g_cDllRef);
}

ThumbnailProvider::~ThumbnailProvider()
{
	InterlockedDecrement(&g_cDllRef);
}

#pragma region IUnknown

// Query to the interface the component supported.
IFACEMETHODIMP ThumbnailProvider::QueryInterface(REFIID riid, void** ppv)
{
	static const QITAB qit[] =
	{
		QITABENT(ThumbnailProvider, IThumbnailProvider),
		QITABENT(ThumbnailProvider, IInitializeWithStream),
		{ 0 },
	};
	return QISearch(this, qit, riid, ppv);
}

// Increase the reference count for an interface on an object.
IFACEMETHODIMP_(ULONG) ThumbnailProvider::AddRef()
{
	return InterlockedIncrement(&m_cRef);
}

// Decrease the reference count for an interface on an object.
IFACEMETHODIMP_(ULONG) ThumbnailProvider::Release()
{
	ULONG cRef = InterlockedDecrement(&m_cRef);
	if (0 == cRef)
	{
		delete this;
	}

	return cRef;
}

#pragma endregion

#pragma region IInitializeWithStream

HRESULT ThumbnailProvider::Initialize(LPCWSTR pszFilePath, DWORD grfMode)
{
	m_path = pszFilePath;
	return S_OK;
}

#pragma endregion

#pragma region IThumbnailProvider

bool EndsWith(const std::wstring& text, const std::wstring& ending)
{
	if (ending.length() > text.length()) return false;
	return text.compare(text.length() - ending.length(), ending.length(), ending) == 0;
}

// Gets a thumbnail image and alpha type. The GetThumbnail is called with the 
// largest desired size of the image, in pixels. Although the parameter is 
// called cx, this is used as the maximum size of both the x and y dimensions. 
// If the retrieved thumbnail is not square, then the longer axis is limited 
// by cx and the aspect ratio of the original image respected. On exit, 
// GetThumbnail provides a handle to the retrieved image. It also provides a 
// value that indicates the color format of the image and whether it has 
// valid alpha information.
IFACEMETHODIMP ThumbnailProvider::GetThumbnail(UINT cx, HBITMAP* phbmp,
	WTS_ALPHATYPE* pdwAlpha)
{
	// Create the COM imaging factory.
	IWICImagingFactory* pImagingFactory;
	HRESULT hr = CoCreateInstance(CLSID_WICImagingFactory, NULL,
		CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pImagingFactory));
	if (FAILED(hr)) return hr;

	uint32_t clearColor = 0;
	if (EndsWith(m_path, L".hdr"))
		clearColor = 0xFF;
	if (EndsWith(m_path, L".ktx"))
		clearColor = 0xFF00;
	if (EndsWith(m_path, L".dds"))
		clearColor = 0xFF0000;
	if (EndsWith(m_path, L".pfm"))
		clearColor = 0xFF000000;
	
	std::vector<uint32_t> colors;
	colors.resize(cx * cx, 0xFFFFFFFF);
	*phbmp = CreateBitmap(cx, cx, 1, 32, colors.data());
	*pdwAlpha = WTSAT_ARGB;

	return S_OK;
}

#pragma endregion