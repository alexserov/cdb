#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>
#include <process.h>
#include <stdio.h>
#include <exception>
#include <iostream>
#include <string>
#include <DbgEng.h>

// When you are using pre-compiled headers, this source file is necessary for compilation to succeed.

extern "C" HRESULT CALLBACK DebugExtensionInitialize(PULONG Version, PULONG Flags)
{
    *Version = 0x00010000;
    *Flags = 0;

    return S_OK;
}

extern "C" HRESULT CALLBACK executescript(PDEBUG_CLIENT Client, PCSTR Args) {
    return system(Args);
}