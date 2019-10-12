#pragma once

#include <windows.h>


//
//   FUNCTION: RegisterInprocServer
//
//   PURPOSE: Register the in-process component in the registry.
//
//   PARAMETERS:
//   * pszModule - Path of the module that contains the component
//   * clsid - Class ID of the component
//   * pszFriendlyName - Friendly name
//   * pszThreadModel - Threading model
//
//   NOTE: The function creates the HKCR\CLSID\{<CLSID>} key in the registry.
// 
//   HKCR
//   {
//      NoRemove CLSID
//      {
//          ForceRemove {<CLSID>} = s '<Friendly Name>'
//          {
//              InprocServer32 = s '%MODULE%'
//              {
//                  val ThreadingModel = s '<Thread Model>'
//              }
//          }
//      }
//   }
//
HRESULT RegisterInprocServer(PCWSTR pszModule, const CLSID& clsid,
	PCWSTR pszFriendlyName, PCWSTR pszThreadModel);


//
//   FUNCTION: UnregisterInprocServer
//
//   PURPOSE: Unegister the in-process component in the registry.
//
//   PARAMETERS:
//   * clsid - Class ID of the component
//
//   NOTE: The function deletes the HKCR\CLSID\{<CLSID>} key in the registry.
//
HRESULT UnregisterInprocServer(const CLSID& clsid);


//
//   FUNCTION: RegisterShellExtThumbnailHandler
//
//   PURPOSE: Register the thumbnail handler.
//
//   PARAMETERS:
//   * pszFileType - The file type that the thumbnail handler is associated 
//     with. For example, '*' means all file types; '.txt' means all .txt 
//     files. The parameter must not be NULL.
//   * clsid - Class ID of the component
//
//   NOTE: The function creates the following key in the registry.
//
//   HKCR
//   {
//      NoRemove <File Type>
//      {
//          NoRemove shellex
//          {
//              {e357fccd-a995-4576-b01f-234630154e96} = s '{<CLSID>}'
//          }
//      }
//   }
//
HRESULT RegisterShellExtThumbnailHandler(PCWSTR pszFileType, const CLSID& clsid);


//
//   FUNCTION: UnregisterShellExtThumbnailHandler
//
//   PURPOSE: Unregister the thumbnail handler.
//
//   PARAMETERS:
//   * pszFileType - The file type that the thumbnail handler is associated 
//     with. For example, '*' means all file types; '.txt' means all .txt 
//     files. The parameter must not be NULL.
//
//   NOTE: The function removes the registry key
//   HKCR\<File Type>\shellex\{e357fccd-a995-4576-b01f-234630154e96}.
//
HRESULT UnregisterShellExtThumbnailHandler(PCWSTR pszFileType);