#include <windows.h>
#include <tchar.h>
#include <stdio.h>
#include <crtdbg.h>
#include <corhdr.h>
#include <math.h>
#include <rpc.h>

// This utility allows you to change the MVID on an assembly.  It's solves the problem that occurs because VSINSTR 
// doesn't change the MVID after instrumenting.  This screws up NGEN and other tools like Reflector for .NET that 
// use the MVID to try and figure out if the assembly has been modified.  
//
// So for example, you VSINSTR System.Data.dll, do a GACUTIL -i System.Data.dll then run NGEN System.Data.dll.  
// NGEN does not refresh the image.  It does if you do NGEN UPDATE, but that might end up recompiling a whole bunch 
// of stuff.   So, just run CHGMVID on the assembly first and the former NGEN will refresh the .NI image.

#pragma pack(8)
#include <dbghelp.h>

#pragma comment(lib, "dbghelp.lib")
#pragma comment(lib, "rpcrt4.lib")

#define Align(a, x)   ((a) * ((((x) - 1) / (a)) + 1))

typedef struct
{
	HANDLE hFile;
	HANDLE hMap;
	PVOID pBase;
	PIMAGE_NT_HEADERS pNtHeader;
} LOADED_IMAGE_DATA, *PLOADED_IMAGE_DATA;

#define RVA2VA(pdata, rva) ImageRvaToVa((pdata)->pNtHeader, (pdata)->pBase, (rva), NULL)

// See http://blogs.msdn.com/johnls/archive/2005/08/09/449655.aspx
int BitCount(UINT64 n)
{
	n = (n & 0x5555555555555555) + ((n >> 1) & 0x555555555555555); 
	n = (n & 0x3333333333333333) + ((n >> 2) & 0x3333333333333333); 
	n = (n & 0x0F0F0F0F0F0F0F0F) + ((n >> 4) & 0x0F0F0F0F0F0F0F0F); 
    return (int)(n % 0xFF);
}

BOOL IsBitSet(UINT64 n, int b)
{
	return (b < 0x40) && (n & ((UINT64)1 << b));
}

void UnmapImage(PLOADED_IMAGE_DATA pImageData)
{
	if (NULL != pImageData->pBase)
		UnmapViewOfFile(pImageData->pBase);
	
	if (NULL != pImageData->hMap)
		CloseHandle(pImageData->hMap);
		
	if (INVALID_HANDLE_VALUE != pImageData->hFile)
		CloseHandle(pImageData->hFile);
}


HRESULT MapImage(wchar_t* pImage, PLOADED_IMAGE_DATA pImageData)
{
	pImageData->hFile = NULL; // A bit redundant perhaps
	pImageData->hMap = NULL;
	pImageData->pBase = NULL;
	pImageData->pNtHeader = NULL;

	HRESULT hr = S_OK;

	if (INVALID_HANDLE_VALUE != (pImageData->hFile = CreateFile(pImage, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL)))
	{
		if (NULL != (pImageData->hMap = CreateFileMapping(pImageData->hFile, NULL, PAGE_READWRITE, 0, 0, NULL)))
		{
			if (NULL != (pImageData->pBase = MapViewOfFile(pImageData->hMap, FILE_MAP_ALL_ACCESS, 0, 0, 0)))
			{
				if (NULL != (pImageData->pNtHeader = ImageNtHeader(pImageData->pBase)))
					return S_OK;
				else
					hr = HRESULT_FROM_WIN32(ERROR_BAD_FORMAT);
			}
		}
	}
	
	if (hr == S_OK)
		hr = HRESULT_FROM_WIN32(GetLastError());
	
	UnmapImage(pImageData);
	return hr;
}

int _cdecl wmain(int argc, wchar_t* argv[])
{
	if (argc < 2)
	{
		wprintf(
			L"usage: chgmvid <image-file> [/c]\n\n"
			L"/c    If not specified, MVID will be displayed but not changed.\n");
		return 0;
	}

	bool changeMvid = false;

	if (argc == 3)
	{
		if (argv[2][0] == '/' || argv[2][0] == '-')
		{
			if (argv[2][1] == 'c' || argv[2][1] == 'C')
				changeMvid = true;				
		}
	}

	LOADED_IMAGE_DATA imageData;
	HRESULT hr = MapImage(argv[1], &imageData);
	
	if (FAILED(hr))
	{
		wprintf(L"ERROR: Unable to load image (%u)\n", HRESULT_CODE(hr));
		return 1;
	}

	PIMAGE_DATA_DIRECTORY pDataDirectories = NULL;
	
	switch (imageData.pNtHeader->OptionalHeader.Magic)
	{
		case IMAGE_NT_OPTIONAL_HDR32_MAGIC:
			pDataDirectories = reinterpret_cast<PIMAGE_NT_HEADERS32>(
				imageData.pNtHeader)->OptionalHeader.DataDirectory;
			break;

		case IMAGE_NT_OPTIONAL_HDR64_MAGIC:
			pDataDirectories = reinterpret_cast<PIMAGE_NT_HEADERS64>(
				imageData.pNtHeader)->OptionalHeader.DataDirectory;
			break;
		
		default:
			wprintf(L"ERROR: Not a PE/PE+ file\n");
			return 1;
	}
	
	// Make sure there is a CLR header
	_ASSERTE(pDataDirectories != NULL);

	if (0 == pDataDirectories[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].VirtualAddress ||
		0 == pDataDirectories[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].Size)
	{
		wprintf(L"ERROR: No CLR header\n");
		return 1;
	}

	// Get the CLR header
	PIMAGE_COR20_HEADER pCorHeader = reinterpret_cast<PIMAGE_COR20_HEADER>(
		RVA2VA(&imageData, pDataDirectories[IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].VirtualAddress));
	
	if (NULL == pCorHeader)
	{
		wprintf(L"ERROR: Could not get CLR header\n");
		return 1;
	}
	
	BYTE* pMetadataRoot = (BYTE*)RVA2VA(&imageData, pCorHeader->MetaData.VirtualAddress);
	
	if (*(UINT32*)pMetadataRoot != 0x424A5342)
	{
		wprintf(L"ERROR: Could not identify metadata root\n");
		return 1;
	}
	
	wprintf(L"\nCLR Target: %S\n", (CHAR*)(pMetadataRoot + 16));
	
	UINT32 paddedVersionLength = Align(4, *(UINT32*)(pMetadataRoot + 12));
	UINT16 numStreams = *(UINT16*)(pMetadataRoot + 16 + paddedVersionLength + 2);
	BYTE* pStreamHeader = (pMetadataRoot + 16 + paddedVersionLength + 4);
	BYTE* pTildeStream = NULL;
	BYTE* pGuidStream = NULL;
	
	for (UINT16 i = 0; i < numStreams; i++)
	{
		BYTE* pStreamData = pMetadataRoot + *(UINT32*)(pStreamHeader);
		char* pStreamName = (char*)(pStreamHeader + 8);
		
		if (strcmp(pStreamName, "#~") == 0)
			pTildeStream = pStreamData;
		else if (strcmp(pStreamName, "#GUID") == 0)
			pGuidStream = pStreamData;
				
		pStreamHeader += 8 + Align(4, strlen((char*)(pStreamHeader + 8)) + 1);
	}
	
	if (NULL == pTildeStream || NULL == pGuidStream)
	{
		wprintf(L"ERROR: Cannot find #~ and/or #GUID streams\n");
		return 1;
	}
	
	if (*(pTildeStream + 4) != 2 && *(pTildeStream + 5) != 0)
	{
		wprintf(L"ERROR: Expected version number 2.0 in metadata stream\n");
		return 1;
	}
	
	BOOL bigStringHeap = (*(pTildeStream + 6) & 0x01) != 0;
	BOOL bigGuidHeap = (*(pTildeStream + 6) & 0x02) != 0;
	
	// Check that the module table is present.  It's table 0x00.  The CLI spec. is not clear about the format 
	// of these bit arrays.  They are actually stored as little endian 64-bit integers.
	if (!IsBitSet(*(UINT64*)(pTildeStream + 8), 0x00))
	{
		wprintf(L"ERROR: No module table present\n");	
		return 1;
	}
	
	unsigned presentTables = BitCount(*(UINT64*)(pTildeStream + 8)); 	
	
	// Remember, the module table is number 0x00, so it's easy to find.
	BYTE* pModuleTable = pTildeStream + 24 + 4 * presentTables;
	
	// Indexes into the GUID table are 1 based
	UINT32 guidIndex = bigGuidHeap ? 
		(*(UINT16*)(pModuleTable + 2 + (bigStringHeap ? 4 : 2)) - 1) :	
		(*(UINT32*)(pModuleTable + 2 + (bigStringHeap ? 4 : 2)) - 1);
	BYTE* pMvid = pGuidStream + guidIndex * 16;
	GUID mvid;

	memcpy(&mvid, pMvid, sizeof(mvid));

	wprintf(L"Current MVID: {%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X}\n", 
		mvid.Data1, mvid.Data2, mvid.Data3,
		(int)mvid.Data4[0], (int)mvid.Data4[1], (int)mvid.Data4[2], (int)mvid.Data4[3],
		(int)mvid.Data4[4], (int)mvid.Data4[5], (int)mvid.Data4[6], (int)mvid.Data4[7]);
	
	if (changeMvid)
	{
		UuidCreate(&mvid);
		
		memcpy(pMvid, &mvid, sizeof(mvid));

		wprintf(L"New MVID: {%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X}\n", 
			mvid.Data1, mvid.Data2, mvid.Data3,
			(int)mvid.Data4[0], (int)mvid.Data4[1], (int)mvid.Data4[2], (int)mvid.Data4[3],
			(int)mvid.Data4[4], (int)mvid.Data4[5], (int)mvid.Data4[6], (int)mvid.Data4[7]);
	}
	
	// Clean-up
	UnmapImage(&imageData);	
	
	return 0;
}
