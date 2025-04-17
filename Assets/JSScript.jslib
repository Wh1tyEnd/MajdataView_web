mergeInto(LibraryManager.library,
{
	UnityLoaded: function () {
		console.log("Handle JS Message Ready")
    	if(window.onUnityLoaded !== undefined)
		window.onUnityLoaded();
  	},
	SetLocalStorge: function(name, value) {
		localStorage.setItem(UTF8ToString(name), UTF8ToString(value));
	},
	GetLocalStorge: function(name) {
		var returnStr = localStorage.getItem(UTF8ToString(name));
		if(returnStr == null || returnStr == undefined)
			return null;
		var bufferSize = lengthBytesUTF8(returnStr) + 1;
    	var buffer = _malloc(bufferSize);
    	stringToUTF8(returnStr, buffer, bufferSize);
    	return buffer;
	}
});