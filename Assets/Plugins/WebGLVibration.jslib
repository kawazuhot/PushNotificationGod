mergeInto(LibraryManager.library, {
  PngVibrate: function (milliseconds) {
    if (typeof navigator !== "undefined" && navigator.vibrate) {
      navigator.vibrate(milliseconds);
    }
  }
});
