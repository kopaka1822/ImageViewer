// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <string>

// {C84C299F-F84E-41DB-A736-389953162C3D}
const CLSID CLSID_ThumbnailProvider =
{ 0xc84c299f, 0xf84e, 0x41db, { 0xa7, 0x36, 0x38, 0x99, 0x53, 0x16, 0x2c, 0x3d } };


HINSTANCE   g_hInst = NULL;
long        g_cDllRef = 0;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpReserved)
{
	switch (dwReason)
	{
	case DLL_PROCESS_ATTACH:
		// Hold the instance of this DLL module, we will use it to get the 
		// path of the DLL to register the component.
		g_hInst = hModule;
		DisableThreadLibraryCalls(hModule);
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}


//
//   FUNCTION: DllGetClassObject
//
//   PURPOSE: Create the class factory and query to the specific interface.
//
//   PARAMETERS:
//   * rclsid - The CLSID that will associate the correct data and code.
//   * riid - A reference to the identifier of the interface that the caller 
//     is to use to communicate with the class object.
//   * ppv - The address of a pointer variable that receives the interface 
//     pointer requested in riid. Upon successful return, *ppv contains the 
//     requested interface pointer. If an error occurs, the interface pointer 
//     is NULL. 
//
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv)
{
	HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;

	if (IsEqualCLSID(CLSID_ThumbnailProvider, rclsid))
	{
		hr = E_OUTOFMEMORY;

		ClassFactory* pClassFactory = new ClassFactory();
		if (pClassFactory)
		{
			hr = pClassFactory->QueryInterface(riid, ppv);
			pClassFactory->Release();
		}
	}

	return hr;
}


//
//   FUNCTION: DllCanUnloadNow
//
//   PURPOSE: Check if we can unload the component from the memory.
//
//   NOTE: The component can be unloaded from the memory when its reference 
//   count is zero (i.e. nobody is still using the component).
// 
STDAPI DllCanUnloadNow(void)
{
	return g_cDllRef > 0 ? S_FALSE : S_OK;
}


//
//   FUNCTION: DllRegisterServer
//
//   PURPOSE: Register the COM server and the thumbnail handler.
// 
STDAPI DllRegisterServer(void)
{
	HRESULT hr;

	wchar_t szModule[MAX_PATH];
	if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}

	// Register the component.
	hr = RegisterInprocServer(szModule, CLSID_ThumbnailProvider,
		L"TextureViewer.ThumbnailHandler.ThumbnailProvider Class",
		L"Apartment");
	if (SUCCEEDED(hr))
	{
		// Register the thumbnail handler.
		RegisterShellExtThumbnailHandler(L".dds", CLSID_ThumbnailProvider);
		RegisterShellExtThumbnailHandler(L".ktx", CLSID_ThumbnailProvider);
		RegisterShellExtThumbnailHandler(L".pfm", CLSID_ThumbnailProvider);
		RegisterShellExtThumbnailHandler(L".hdr", CLSID_ThumbnailProvider);	
		RegisterShellExtThumbnailHandler(L".exr", CLSID_ThumbnailProvider);	
		
		// This tells the shell to invalidate the thumbnail cache. It is 
		// important because any .* files viewed before registering 
		// this handler would otherwise show cached blank thumbnails.
		SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, NULL, NULL);

		// set registry fo IInitializeWithFile to work
		// 
		// HKEY_CLASSES_ROOT
		//  CLSID
		// 	 {C84C299F-F84E-41DB-A736-389953162C3D}
		// 	 DisableProcessIsolation = 1
		HKEY classKey = nullptr;
		DWORD one = 1;
		RegOpenKeyExA(HKEY_CLASSES_ROOT, "CLSID\\{C84C299F-F84E-41DB-A736-389953162C3D}", 0, KEY_WRITE, &classKey);
		RegSetValueExA(classKey, "DisableProcessIsolation", 0, REG_DWORD, reinterpret_cast<BYTE*>(&one), sizeof(one));
		RegCloseKey(classKey);
	}

	return hr;
}

//
//   FUNCTION: DllUnregisterServer
//
//   PURPOSE: Unregister the COM server and the thumbnail handler.
// 
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = S_OK;

	wchar_t szModule[MAX_PATH];
	if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		return hr;
	}

	// Unregister the component.
	hr = UnregisterInprocServer(CLSID_ThumbnailProvider);
	if (SUCCEEDED(hr))
	{
		// Unregister the thumbnail handler.
		UnregisterShellExtThumbnailHandler(L".dds");
		UnregisterShellExtThumbnailHandler(L".ktx");
		UnregisterShellExtThumbnailHandler(L".pfm");
		UnregisterShellExtThumbnailHandler(L".hdr");
		UnregisterShellExtThumbnailHandler(L".exr");
	}

	return hr;
}