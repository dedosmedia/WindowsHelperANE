#include "FreSharpMacros.h"
#include "WindowsHelperANE.h"
#include "FlashRuntimeExtensions.h"
#include "stdafx.h"
#include "FreSharpBridge.h"

extern "C" {

	CONTEXT_INIT(TRWH) {

		FREBRIDGE_INIT

			static FRENamedFunction extensionFunctions[] = {
			MAP_FUNCTION(init)
			,MAP_FUNCTION(findWindowByTitle)
			,MAP_FUNCTION(showWindow)
			,MAP_FUNCTION(hideWindow)
			,MAP_FUNCTION(setForegroundWindow)
			,MAP_FUNCTION(getDisplayDevices)
			,MAP_FUNCTION(setDisplayResolution)
			,MAP_FUNCTION(restartApp)
			,MAP_FUNCTION(registerHotKey)
			,MAP_FUNCTION(unregisterHotKey)
			,MAP_FUNCTION(getNumLogicalProcessors)
			
			,MAP_FUNCTION(findTaskbar)
			,MAP_FUNCTION(isProgramRunning)
			,MAP_FUNCTION(unzipFile)
			,MAP_FUNCTION(testCV)
			,MAP_FUNCTION(test)
			,MAP_FUNCTION(findTaskbar)
			,MAP_FUNCTION(makeTopMostWindow)
			,MAP_FUNCTION(makeNoTopMostWindow)
			,MAP_FUNCTION(makeBottomWindow)
			,MAP_FUNCTION(resizeWindow)

		};

		SET_FUNCTIONS

	}

	CONTEXT_FIN(TRWH) {
	}

	EXTENSION_INIT(TRWH)

	EXTENSION_FIN(TRWH)

}
