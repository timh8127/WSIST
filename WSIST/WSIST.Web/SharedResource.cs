namespace WSIST.Web;

// Marker type for the single shared localization resource. All UI strings live
// in Resources/SharedResource.resx (English) and Resources/SharedResource.de.resx
// (German). Pages inject IStringLocalizer<SharedResource> and look strings up by
// key. A single shared resource (rather than one resx per component) keeps the
// German terminology consistent across pages and makes it easy to scan one file
// for any missing/untranslated key.
public sealed class SharedResource { }
