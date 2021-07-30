mergeInto(LibraryManager.library, {

  LogBrowser : function(data) {
    window.alert(Pointer_stringify(data));
  },

  SaveToLocalStorage : function(key, data) {
    localStorage.setItem(Pointer_stringify(key), Pointer_stringify(data));
  },

  LoadFromLocalStorage : function(key) {
    var returnStr = localStorage.getItem(Pointer_stringify(key));
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  RemoveFromLocalStorage : function(key) {
    localStorage.removeItem(Pointer_stringify(key));
  },

  HasKeyInLocalStorage : function(key) {
    if (localStorage.getItem(Pointer_stringify(key))) {
      return 1;
    }
    else {
      return 0;
    }
  }

});