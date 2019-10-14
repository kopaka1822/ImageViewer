#include "pch.h"
#include <new>
#include <Shlwapi.h>
#include "ThumbnailProvider.h"
#pragma comment(lib, "shlwapi.lib")


extern long g_cDllRef;
extern HINSTANCE g_hInst;

ClassFactory::ClassFactory() : m_cRef(1)
{
	InterlockedIncrement(&g_cDllRef);

	// obtain file name with path
	m_directory.resize(MAX_PATH, '\0');
	GetModuleFileNameA(g_hInst, m_directory.data(), DWORD(m_directory.size()));
	
	// cut out file name
	auto pos = m_directory.find_last_of('\\');
	if(pos != std::string::npos)
	{
		m_directory.resize(pos + 1);
	}
}

ClassFactory::~ClassFactory()
{
	InterlockedDecrement(&g_cDllRef);
}


//
// IUnknown
//

IFACEMETHODIMP ClassFactory::QueryInterface(REFIID riid, void** ppv)
{
	static const QITAB qit[] =
	{
		QITABENT(ClassFactory, IClassFactory),
		{ 0 },
	};
	return QISearch(this, qit, riid, ppv);
}

IFACEMETHODIMP_(ULONG) ClassFactory::AddRef()
{
	return InterlockedIncrement(&m_cRef);
}

IFACEMETHODIMP_(ULONG) ClassFactory::Release()
{
	ULONG cRef = InterlockedDecrement(&m_cRef);
	if (0 == cRef)
	{
		delete this;
	}
	return cRef;
}


// 
// IClassFactory
//

IFACEMETHODIMP ClassFactory::CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppv)
{
	HRESULT hr = CLASS_E_NOAGGREGATION;

	// pUnkOuter is used for aggregation. We do not support it in the sample.
	if (pUnkOuter == NULL)
	{
		hr = E_OUTOFMEMORY;

		// Create the COM component.
		ThumbnailProvider* pExt = new (std::nothrow) ThumbnailProvider(m_directory);
		if (pExt)
		{
			// Query the specified interface.
			hr = pExt->QueryInterface(riid, ppv);
			pExt->Release();
		}
	}

	return hr;
}

IFACEMETHODIMP ClassFactory::LockServer(BOOL fLock)
{
	if (fLock)
	{
		InterlockedIncrement(&g_cDllRef);
	}
	else
	{
		InterlockedDecrement(&g_cDllRef);
	}
	return S_OK;
}